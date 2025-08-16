using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.Models;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Schema;
using Mapster;

namespace CMS.Main.Services;

public class SchemaService(
    DbContextConcurrencyHelper dbHelper,
    ILogger<SchemaService> logger
) : ISchemaService
{
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
            logger.LogError(ex, "There was an error when creating the schema for project {projectId}.", schemaCreationDto.ProjectId);
            return Result.Error($"There was an error when creating the schema for project {schemaCreationDto.ProjectId}.");
        }
    }
}