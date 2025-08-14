using Ardalis.Result;
using CMS.Shared.DTOs.Pagination;
using CMS.Shared.DTOs.Project;

namespace CMS.Shared.Abstractions;

public interface IProjectService
{
    public const int MaxPageSize = 50;
    
    Task<Result<(List<ProjectWithIdDto>, PaginationMetadata)>> GetProjectsForUserAsync(
        string userId, 
        PaginationParams? paginationParams = null);
    
    Task<Result<ProjectWithIdDto>> CreateProjectAsync(
        ProjectCreationDto projectDto);
}