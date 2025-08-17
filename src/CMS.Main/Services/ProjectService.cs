using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.Models;
using CMS.Main.Services.State;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Pagination;
using CMS.Shared.DTOs.Project;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Services;

public class ProjectService(
    DbContextConcurrencyHelper dbHelper,
    ProjectStateService projectStateService,
    ILogger<ProjectService> logger
) : IProjectService
{
    public async Task<Result<(List<ProjectWithIdDto>, PaginationMetadata)>> GetProjectsForUserAsync(
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

                var dtos = projects.Adapt<List<ProjectWithIdDto>>();
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

    public async Task<Result<ProjectWithIdDto>> GetProjectByIdAsync(
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
                var project = await query.FirstOrDefaultAsync(p => p.Id == projectId);
                return project;
            });
            if (result is null)
                return Result.NotFound();
            var dto = result.Adapt<ProjectWithIdDto>();
            if (!options.IncludeSchemas) dto.Schemas = [];
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when retrieving project {projectId}.", projectId);
            return Result.Error($"There was an error when retrieving project {projectId}.");
        }
    }

    public async Task<Result<ProjectWithIdDto>> CreateProjectAsync(ProjectCreationDto projectDto)
    {
        if (!Guid.TryParse(projectDto.OwnerId, out _))
            return Result.Invalid(new ValidationError("OwnerID must be a valid GUID."));

        try
        {
            var project = projectDto.Adapt<Project>();
            await dbHelper.ExecuteAsync(async dbContext =>
            {
                await dbContext.Projects.AddAsync(project);
                await dbContext.SaveChangesAsync();
            });

            var adapted = project.Adapt<ProjectWithIdDto>();
            projectStateService.NotifyCreated([adapted]);

            return Result.Success(adapted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when creating a project for user {ownerId}.", projectDto.OwnerId);
            return Result.Error($"There was an error when creating a project for user {projectDto.OwnerId}.");
        }
    }

    public async Task<Result<ProjectWithIdDto>> UpdateProjectAsync(ProjectUpdateDto projectDto)
    {
        try
        {
            var project = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Projects.FindAsync(projectDto.Id));

            if (project is null)
                return Result.NotFound();

            projectDto.Adapt(project);
            project.LastUpdated = DateTime.UtcNow;
            await dbHelper.ExecuteAsync(async dbContext => { await dbContext.SaveChangesAsync(); });

            var adapted = project.Adapt<ProjectWithIdDto>();
            projectStateService.NotifyUpdated([adapted]);

            return Result.Success(adapted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when updating project {projectId}.", projectDto.Id);
            return Result.Error($"There was an error when updating project {projectDto.Id}.");
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

    public async Task<Result<bool>> OwnsProject(string userId, string projectId)
    {
        try
        {
            var project = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Projects.FindAsync(projectId));

            return project is null ? Result.NotFound() : Result.Success(project.OwnerId == userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when checking ownership of project {projectId} for user {userId}.",
                projectId, userId);
            return Result.Error(
                $"There was an error when checking ownership of project {projectId} for user {userId}.");
        }
    }
}