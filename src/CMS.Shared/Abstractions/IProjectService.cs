using Ardalis.Result;
using CMS.Shared.DTOs.Pagination;
using CMS.Shared.DTOs.Project;

namespace CMS.Shared.Abstractions;

public interface IProjectService
{
    public const int MaxPageSize = 50;

    Task<Result<(List<ProjectDto>, PaginationMetadata)>> GetProjectsForUserAsync(
        string userId,
        PaginationParams? paginationParams = null,
        Action<ProjectQueryOptions>? configureOptions = null);

    Task<Result<ProjectDto>> GetProjectByIdAsync(
        string projectId,
        Action<ProjectQueryOptions>? configureOptions = null);

    Task<Result<ProjectDto>> CreateProjectAsync(
        ProjectDto projectDto);

    Task<Result<ProjectDto>> UpdateProjectAsync(
        ProjectDto projectDto);

    Task<Result> DeleteProjectAsync(
        string projectId);
}