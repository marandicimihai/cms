using Ardalis.Result;
using CMS.Main.DTOs.Pagination;
using CMS.Main.DTOs.Project;

namespace CMS.Main.Abstractions;

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
        ProjectDto dto);

    Task<Result> UpdateProjectAsync(
        ProjectDto dto);

    Task<Result> DeleteProjectAsync(
        string projectId);
}