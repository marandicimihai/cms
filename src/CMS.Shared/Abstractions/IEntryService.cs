using Ardalis.Result;
using CMS.Shared.DTOs.Entry;
using CMS.Shared.DTOs.Pagination;

namespace CMS.Shared.Abstractions;

public interface IEntryService
{
    public const int MaxPageSize = 100;
    
    Task<Result<(List<EntryWithIdDto>, PaginationMetadata)>> GetEntriesForSchema(
        string schemaId,
        PaginationParams? paginationParams = null,
        Action<EntryGetOptions>? configureOptions = null);
    
    Task<Result<EntryWithIdDto>> AddEntryAsync(
        EntryCreationDto creationDto);
}