using Ardalis.Result;
using CMS.Shared.DTOs.Pagination;
using CMS.Shared.DTOs.Project;

namespace CMS.Shared.Abstractions;

public interface IProjectService
{
    public const int MaxPageSize = 50;

    Task<Result<(List<ProjectWithIdDto>, PaginationMetadata)>> GetProjectsForUserAsync(
        string userId,
        PaginationParams? paginationParams = null,
        Action<ProjectQueryOptions>? configureOptions = null);

    Task<Result<ProjectWithIdDto>> GetProjectByIdAsync(
        string projectId,
        Action<ProjectQueryOptions>? configureOptions = null);

    Task<Result<ProjectWithIdDto>> CreateProjectAsync(
        ProjectCreationDto projectDto);

    Task<Result<ProjectWithIdDto>> UpdateProjectAsync(
        ProjectUpdateDto projectDto);

    Task<Result> DeleteProjectAsync(
        string projectId);

    Task<Result<bool>> OwnsProject(string userId, string projectId);
}