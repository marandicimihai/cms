using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.Models;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Schema;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Services;

public class SchemaService(
    DbContextConcurrencyHelper dbHelper,
    ILogger<SchemaService> logger
) : ISchemaService
{
    public async Task<Result<SchemaWithIdDto>> GetSchemaByIdAsync(
        string schemaId, 
        Action<SchemaGetOptions>? optionsAction = null)
    {
        var options = new SchemaGetOptions();
        optionsAction?.Invoke(options);
        try
        {
            var schema = await dbHelper.ExecuteAsync(async dbContext =>
                options.IncludeProperties
                    ? await dbContext.Schemas
                        .Include(s => s.Properties)
                        .FirstOrDefaultAsync(s => s.Id == schemaId)
                    : await dbContext.Schemas
                        .FirstOrDefaultAsync(s => s.Id == schemaId)
            );

            if (schema is null)
                return Result.NotFound();

            var dto = schema.Adapt<SchemaWithIdDto>();
            if (!options.IncludeProperties)
                dto.Properties = [];

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when getting schema {schemaId}.", schemaId);
            return Result.Error($"There was an error when getting schema {schemaId}.");
        }
    }

    public async Task<Result<SchemaWithIdDto>> CreateSchemaAsync(SchemaCreationDto schemaCreationDto)
    {
        try
        {
            // Check if project exists
            var project = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Projects.FindAsync(schemaCreationDto.ProjectId));

            if (project is null)
                return Result.NotFound($"Project {schemaCreationDto.ProjectId} was not found.");

            var schema = schemaCreationDto.Adapt<Schema>();

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                await dbContext.AddAsync(schema);
                await dbContext.SaveChangesAsync();
            });

            return Result.Success(schema.Adapt<SchemaWithIdDto>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when creating the schema for project {projectId}.",
                schemaCreationDto.ProjectId);
            return Result.Error(
                $"There was an error when creating the schema for project {schemaCreationDto.ProjectId}.");
        }
    }

    public async Task<Result> DeleteSchemaAsync(string schemaId)
    {
        try
        {
            var schema = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Schemas.FindAsync(schemaId));

            if (schema is null)
                return Result.NotFound($"Schema {schemaId} was not found.");

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                dbContext.Remove(schema);
                await dbContext.SaveChangesAsync();
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when deleting schema {schemaId}.", schemaId);
            return Result.Error($"There was an error when deleting schema {schemaId}.");
        }
    }
}