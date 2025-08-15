namespace CMS.Shared.DTOs.Pagination;

public record PaginationMetadata(int TotalCount, int CurrentPage, int PageSize, int MaxPageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}