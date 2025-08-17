using System.Linq.Expressions;
using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.Models;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.SchemaProperty;
using Mapster;

namespace CMS.Main.Services;

public class SchemaPropertyService(
    DbContextConcurrencyHelper dbHelper,
    ILogger<SchemaPropertyService> logger
) : ISchemaPropertyService
{
    public async Task<Result<SchemaPropertyWithIdDto>> CreateSchemaPropertyAsync(
        SchemaPropertyCreationDto creationDto)
    {
        try
        {
            var schema = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Schemas.FindAsync(creationDto.SchemaId));

            if (schema is null)
                return Result.NotFound();
            
            var property = creationDto.Adapt<SchemaProperty>();

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                await dbContext.AddAsync(property);
                await dbContext.SaveChangesAsync();
            });

            var resultDto = property.Adapt<SchemaPropertyWithIdDto>();
            return Result.Success(resultDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating schema property for schema {schemaId}", creationDto.SchemaId);
            return Result.Error($"Error creating schema property for schema {creationDto.SchemaId}");
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
                await dbContext.SaveChangesAsync();
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting schema property {propertyId}", propertyId);
            return Result.Error($"Error deleting schema property {propertyId}");
        }
    }
}