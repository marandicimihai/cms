using Ardalis.Result;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.Data;
using CMS.Main.DTOs;
using CMS.Main.Models;
using CMS.Main.Models.MappingConfig;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Services;

public class PropertyService(
    IDbContextConcurrencyHelper dbHelper,
    ILogger<PropertyService> logger
) : IPropertyService
{
    public async Task<Result<PropertyDto>> CreatePropertyAsync(
        PropertyDto dto)
    {
        try
        {
            var schema = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Schemas
                    .Include(s => s.Properties)
                    .FirstOrDefaultAsync(s => s.Id == dto.SchemaId));

            if (schema is null)
                return Result.NotFound();

            if (schema.Properties.Any(p => p.Name == dto.Name))
                return Result.Error("Property name must be unique within the schema.");

            var property = dto.Adapt<Property>();

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                await dbContext.AddAsync(property);
                await dbContext.SaveChangesAsync();
            });

            var resultDto = property.Adapt<PropertyDto>();
            return Result.Success(resultDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating schema property for schema {schemaId}.", dto.SchemaId);
            return Result.Error($"Error creating schema property for schema {dto.SchemaId}.");
        }
    }

    public async Task<Result<PropertyDto>> UpdatePropertyAsync(PropertyDto dto)
    {
        try
        {
            var property = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Properties.FindAsync(dto.Id));

            if (property is null)
                return Result.NotFound();

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                if (dto.Name != property.Name && dbContext.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
                {
                    await dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE \"Entries\" SET \"Data\" = \"Data\" - {property.Name} WHERE \"SchemaId\" = {property.SchemaId}");
                }
                else
                {
                    logger.LogInformation("Skipping SQL execution as the database is not npgsql and might not support json.");
                }
                
                dto.Adapt(property, MapsterConfig.EditSchemaPropertyConfig);
                
                await dbContext.SaveChangesAsync();
            });

            return Result.Success(property.Adapt<PropertyDto>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating schema property {propertyId}.", dto.Id);
            return Result.Error($"Error updating schema property {dto.Id}.");
        }
    }

    public async Task<Result> DeletePropertyAsync(string propertyId)
    {
        try
        {
            var property = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Properties.FindAsync(propertyId));

            if (property is null)
                return Result.NotFound();

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                dbContext.Remove(property);

                if (dbContext.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
                {
                    await dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE \"Entries\" SET \"Data\" = \"Data\" - {property.Name} WHERE \"SchemaId\" = {property.SchemaId}");
                }
                else
                {
                    logger.LogInformation("Skipping SQL execution as the database is not npgsql and might not support json.");
                }
                
                await dbContext.SaveChangesAsync();
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting schema property {propertyId}.", propertyId);
            return Result.Error($"Error deleting schema property {propertyId}.");
        }
    }
}