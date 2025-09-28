using Ardalis.Result;
using CMS.Main.DTOs.Entry;
using CMS.Main.DTOs.Pagination;

namespace CMS.Main.Abstractions.Entries;

public interface IEntryService
{
    public const int MaxPageSize = 100;
    
    Task<Result<(List<EntryDto>, PaginationMetadata)>> GetEntriesForSchema(
        string schemaId,
        PaginationParams? paginationParams = null,
        Action<EntrySortingOptions>? configureOptions = null);
    
    Task<Result<EntryDto>> GetEntryByIdAsync(
        string entryId,
        Action<EntrySortingOptions>? configureOptions = null);
    
    Task<Result<EntryDto>> AddEntryAsync(
        EntryDto dto);
    
    Task<Result> UpdateEntryAsync(
        EntryDto dto);
    
    Task<Result> DeleteEntryAsync(
        string entryId);
}