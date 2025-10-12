using CMS.Main.DTOs;

namespace CMS.Main.Services.State;

public class EntryStateService
{
    public event Action<List<EntryDto>>? EntriesCreated;
    
    public void NotifyCreated(List<EntryDto> entries)
    {
        EntriesCreated?.Invoke(entries);
    }
}