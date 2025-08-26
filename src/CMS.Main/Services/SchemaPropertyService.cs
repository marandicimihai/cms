using Ardalis.Result;
using CMS.Main.Abstractions;
using CMS.Main.Data;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Services;

public class SchemaPropertyService(
    IDbContextConcurrencyHelper dbHelper,
    ILogger<SchemaPropertyService> logger
) : ISchemaPropertyService
{
    public async Task<Result<SchemaPropertyDto>> CreateSchemaPropertyAsync(
        SchemaPropertyDto dto)
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

            var property = dto.Adapt<SchemaProperty>();

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                await dbContext.AddAsync(property);
                await dbContext.SaveChangesAsync();
            });

            var resultDto = property.Adapt<SchemaPropertyDto>();
            return Result.Success(resultDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating schema property for schema {schemaId}.", dto.SchemaId);
            return Result.Error($"Error creating schema property for schema {dto.SchemaId}.");
        }
    }

    public async Task<Result<SchemaPropertyDto>> UpdateSchemaPropertyAsync(SchemaPropertyDto dto)
    {
        try
        {
            var property = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.SchemaProperties.FindAsync(dto.Id));

            if (property is null)
                return Result.NotFound();
            
            dto.Adapt(property, new TypeAdapterConfig()
                .NewConfig<SchemaPropertyDto, SchemaProperty>()
                .Inherits<SchemaPropertyDto, SchemaProperty>()
                .Ignore(p => p.SchemaId)
                .Ignore(p => p.Schema)
                .Ignore(p => p.Type)
                .Config);

            await dbHelper.ExecuteAsync(async dbContext => { await dbContext.SaveChangesAsync(); });

            return Result.Success(property.Adapt<SchemaPropertyDto>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating schema property {propertyId}.", dto.Id);
            return Result.Error($"Error updating schema property {dto.Id}.");
        }
    }

    public async Task<Result> DeleteSchemaPropertyAsync(string propertyId)
    {
        try
        {
            var property = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.SchemaProperties.FindAsync(propertyId));

            if (property is null)
                return Result.NotFound();

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                dbContext.Remove(property);

                await dbContext.Database.ExecuteSqlAsync(
                    $"UPDATE \"Entries\" SET \"Data\" = \"Data\" - {property.Name} WHERE \"SchemaId\" = {property.SchemaId}");
                
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