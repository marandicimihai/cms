using CMS.Shared.DTOs.Entry;

namespace CMS.Main.Services.State;

public class EntryStateService
{
    public event Action<List<EntryWithIdDto>>? EntriesCreated;
    
    public void NotifyCreated(List<EntryWithIdDto> entries)
    {
        EntriesCreated?.Invoke(entries);
    }
}