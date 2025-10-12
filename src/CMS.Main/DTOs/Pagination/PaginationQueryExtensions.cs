namespace CMS.Main.DTOs.Pagination;

public static class PaginationQueryExtensions
{
    public static IQueryable<T> TakePage<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        query = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        return query;
    }
}
