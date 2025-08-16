using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.Models;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Pagination;
using CMS.Shared.DTOs.Project;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Services;

public class ProjectService(
    DbContextConcurrencyHelper dbHelper,
    ILogger<ProjectService> logger
) : IProjectService
{
    public async Task<Result<(List<ProjectWithIdDto>, PaginationMetadata)>> GetProjectsForUserAsync(
        string userId, 
        PaginationParams? paginationParams = null)
    {
        paginationParams ??= new PaginationParams(1, 10);
        var cappedPageSize = Math.Clamp(paginationParams.PageSize, 1, IProjectService.MaxPageSize);
        var cappedPageNumber = Math.Max(paginationParams.PageNumber, 1);

        try
        {
            var result = await dbHelper.ExecuteAsync(async dbContext =>
            {
                var projects = await dbContext.Projects
                    .Where(p => p.OwnerId == userId)
                    .OrderByDescending(p => p.LastUpdated)
                    .Skip((cappedPageNumber - 1) * cappedPageSize)
                    .Take(cappedPageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var paginationMetadata = new PaginationMetadata(
                    TotalCount: await dbContext.Projects.CountAsync(p => p.OwnerId == userId),
                    CurrentPage: cappedPageNumber,
                    PageSize: cappedPageSize,
                    MaxPageSize: IProjectService.MaxPageSize
                );

                return (projects.Adapt<List<ProjectWithIdDto>>(), paginationMetadata);
            });

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an error when retrieving projects for user {userId}.", userId);
            return Result.Error($"There was an error when retrieving projects for user {userId}.");
        }
    }

    public async Task<Result<ProjectWithIdDto>> GetProjectByIdAsync(string projectId)
    {
        try
        {
            var result = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Projects.FindAsync(projectId));
            
            return result is null ? Result.NotFound() : Result.Success(result.Adapt<ProjectWithIdDto>());
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

            return Result.Success(project.Adapt<ProjectWithIdDto>());
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

            return Result.Success(project.Adapt<ProjectWithIdDto>());
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
            logger.LogError(ex, "There was an error when checking ownership of project {projectId} for user {userId}.", projectId, userId);
            return Result.Error($"There was an error when checking ownership of project {projectId} for user {userId}.");
        }
    }
}