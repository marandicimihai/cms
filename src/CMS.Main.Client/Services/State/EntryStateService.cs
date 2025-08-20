using CMS.Shared.DTOs.Entry;

namespace CMS.Main.Client.Services.State;

public class EntryStateService
{
    public event Action<List<EntryDto>>? EntriesCreated;
    
    public void NotifyCreated(List<EntryDto> entries)
    {
        EntriesCreated?.Invoke(entries);
    }
}