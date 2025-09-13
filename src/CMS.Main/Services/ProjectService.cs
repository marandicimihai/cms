using Ardalis.Result;
using CMS.Main.Abstractions;
using CMS.Main.Data;
using CMS.Main.DTOs.Pagination;
using CMS.Main.DTOs.Project;
using CMS.Main.Models;
using CMS.Main.Models.MappingConfig;
using CMS.Main.Services.State;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Services;

public class ProjectService(
    IDbContextConcurrencyHelper dbHelper,
    ProjectStateService projectStateService,
    ILogger<ProjectService> logger
) : IProjectService
{
    public async Task<Result<(List<ProjectDto>, PaginationMetadata)>> GetProjectsForUserAsync(
        string userId,
        PaginationParams? paginationParams = null,
        Action<ProjectQueryOptions>? configureOptions = null)
    {
        paginationParams ??= new PaginationParams(1, 10);
        var cappedPageSize = Math.Clamp(paginationParams.PageSize, 1, IProjectService.MaxPageSize);
        var cappedPageNumber = Math.Max(paginationParams.PageNumber, 1);

        var options = new ProjectQueryOptions();
        configureOptions?.Invoke(options);

        try
        {
            var result = await dbHelper.ExecuteAsync(async dbContext =>
            {
                var query = dbContext.Projects
                    .Where(p => p.OwnerId == userId);
                if (options.IncludeSchemas) query = query.Include(p => p.Schemas);
                if (options.IncludeApiKeys) query = query.Include(p => p.ApiKeys);
                var projects = await query
                    .OrderByDescending(p => p.LastUpdated)
                    .Skip((cappedPageNumber - 1) * cappedPageSize)
                    .Take(cappedPageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var paginationMetadata = new PaginationMetadata(
                    await dbContext.Projects.CountAsync(p => p.OwnerId == userId),
                    cappedPageNumber,
                    cappedPageSize,
                    IProjectService.MaxPageSize
                );

                var dtos = projects.Adapt<List<ProjectDto>>();
                if (!options.IncludeSchemas)
                    foreach (var dto in dtos)
                        dto.Schemas = [];

                return (dtos, paginationMetadata);
            });

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when retrieving projects for user {userId}.", userId);
            return Result.Error($"There was an error when retrieving projects for user {userId}.");
        }
    }

    public async Task<Result<ProjectDto>> GetProjectByIdAsync(
        string projectId,
        Action<ProjectQueryOptions>? configureOptions = null)
    {
        var options = new ProjectQueryOptions();
        configureOptions?.Invoke(options);
        try
        {
            var result = await dbHelper.ExecuteAsync(async dbContext =>
            {
                var query = dbContext.Projects.AsQueryable();
                if (options.IncludeSchemas) query = query.Include(p => p.Schemas);
                if (options.IncludeApiKeys) query = query.Include(p => p.ApiKeys);
                var project = await query.FirstOrDefaultAsync(p => p.Id == projectId);
                return project;
            });
            if (result is null)
                return Result.NotFound();
            var dto = result.Adapt<ProjectDto>();
            if (!options.IncludeSchemas) dto.Schemas = [];
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when retrieving project {projectId}.", projectId);
            return Result.Error($"There was an error when retrieving project {projectId}.");
        }
    }

    public async Task<Result<ProjectDto>> CreateProjectAsync(ProjectDto dto)
    {
        if (!Guid.TryParse(dto.OwnerId, out _))
            return Result.Invalid(new ValidationError("OwnerID must be a valid GUID."));

        try
        {
            var project = dto.Adapt<Project>();
            await dbHelper.ExecuteAsync(async dbContext =>
            {
                await dbContext.Projects.AddAsync(project);
                await dbContext.SaveChangesAsync();
            });

            var adapted = project.Adapt<ProjectDto>();
            projectStateService.NotifyCreated([adapted]);

            return Result.Success(adapted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when creating a project for user {ownerId}.", dto.OwnerId);
            return Result.Error($"There was an error when creating a project for user {dto.OwnerId}.");
        }
    }

    public async Task<Result> UpdateProjectAsync(ProjectDto dto)
    {
        try
        {
            var project = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Projects.FindAsync(dto.Id));

            if (project is null)
                return Result.NotFound();

            dto.Adapt(project, MapsterConfig.EditProjectConfig);
            project.LastUpdated = DateTime.UtcNow;
            await dbHelper.ExecuteAsync(async dbContext => { await dbContext.SaveChangesAsync(); });

            var adapted = project.Adapt<ProjectDto>();
            projectStateService.NotifyUpdated([adapted]);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when updating project {projectId}.", dto.Id);
            return Result.Error($"There was an error when updating project {dto.Id}.");
        }
    }

    public async Task<Result> DeleteProjectAsync(string projectId)
    {
        try
        {
            var project = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Projects.FindAsync(projectId));

            if (project is null)
                return Result.NotFound();

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                dbContext.Remove(project);
                await dbContext.SaveChangesAsync();
            });

            projectStateService.NotifyDeleted([projectId]);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when deleting project {projectId}.", projectId);
            return Result.Error($"There was an error when deleting project {projectId}.");
        }
    }
}