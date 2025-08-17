using Ardalis.Result;
using CMS.Shared.DTOs.Entry;

namespace CMS.Shared.Abstractions;

public interface IEntryService
{
    Task<Result<EntryWithIdDto>> AddEntryAsync(
        EntryCreationDto creationDto);
}