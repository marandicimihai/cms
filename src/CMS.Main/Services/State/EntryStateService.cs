using CMS.Main.DTOs;

namespace CMS.Main.Services.State;

public class EntryStateService
{
    public event Action<List<EntryDto>>? EntriesCreated;
    public event Action<List<EntryDto>>? EntriesUpdated;
    
    public void NotifyCreated(List<EntryDto> entries)
    {
        EntriesCreated?.Invoke(entries);
    }
    
    public void NotifyUpdated(List<EntryDto> entries)
    {
        EntriesUpdated?.Invoke(entries);
    }
}