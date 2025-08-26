using Ardalis.Result;
using CMS.Main.DTOs.Entry;
using CMS.Main.DTOs.Pagination;

namespace CMS.Main.Abstractions;

public interface IEntryService
{
    public const int MaxPageSize = 100;
    
    Task<Result<(List<EntryDto>, PaginationMetadata)>> GetEntriesForSchema(
        string schemaId,
        PaginationParams? paginationParams = null,
        Action<EntryGetOptions>? configureOptions = null);
    
    Task<Result<EntryDto>> GetEntryByIdAsync(
        string entryId,
        Action<EntryGetOptions>? configureOptions = null);
    
    Task<Result<EntryDto>> AddEntryAsync(
        EntryDto dto);
    
    Task<Result> UpdateEntryAsync(
        EntryDto dto);
    
    Task<Result> DeleteEntryAsync(
        string entryId);
}