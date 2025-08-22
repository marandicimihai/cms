using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.Models;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Schema;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Services;

public class SchemaService(
    IDbContextConcurrencyHelper dbHelper,
    ILogger<SchemaService> logger
) : ISchemaService
{
    public async Task<Result<SchemaDto>> GetSchemaByIdAsync(
        string schemaId, 
        Action<SchemaGetOptions>? optionsAction = null)
    {
        var options = new SchemaGetOptions();
        optionsAction?.Invoke(options);
        try
        {
            var schema = await dbHelper.ExecuteAsync(async dbContext =>
            {
                var query = dbContext.Schemas.AsQueryable();

                if (options.IncludeProperties)
                {
                    query = query.Include(s => s.Properties);
                }

                if (options.IncludeProject)
                {
                    query = query.Include(s => s.Project);
                }

                return await query.FirstOrDefaultAsync(s => s.Id == schemaId);
            });

            if (schema is null)
                return Result.NotFound();

            var dto = schema.Adapt<SchemaDto>();
            dto.Properties = options.IncludeProperties ? dto.Properties.OrderBy(p => p.CreatedAt).ToList() : [];

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when getting schema {schemaId}.", schemaId);
            return Result.Error($"There was an error when getting schema {schemaId}.");
        }
    }

    public async Task<Result<SchemaDto>> CreateSchemaAsync(SchemaDto dto)
    {
        try
        {
            // Check if the project exists
            var project = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Projects.FindAsync(dto.ProjectId));

            if (project is null)
                return Result.NotFound($"Project {dto.ProjectId} was not found.");

            var schema = dto.Adapt<Schema>();

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                await dbContext.AddAsync(schema);
                await dbContext.SaveChangesAsync();
            });

            return Result.Success(schema.Adapt<SchemaDto>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when creating the schema for project {projectId}.",
                dto.ProjectId);
            return Result.Error(
                $"There was an error when creating the schema for project {dto.ProjectId}.");
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