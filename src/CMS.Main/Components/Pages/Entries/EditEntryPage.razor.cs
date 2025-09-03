using CMS.Main.Abstractions;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs.Entry;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Entries;

public partial class EditEntryPage : ComponentBase
{
    [Parameter]
    public Guid EntryId { get; set; }

    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;

    [Inject]
    private IEntryService EntryService { get; set; } = default!;
    
    private EntryDto? Entry { get; set; }
    
    private StatusIndicator? statusIndicator;

    protected override async Task OnInitializedAsync()
    {
        if (!await AuthHelper.CanEditEntry(EntryId.ToString()))
        {
            statusIndicator?.Show("You do not have access to this entry or it does not exist.", 
                StatusIndicator.StatusSeverity.Error);
            return;
        }

        var result = await EntryService.GetEntryByIdAsync(
            EntryId.ToString(),
            opt =>
            {
                opt.IncludeSchema = true;
                opt.SchemaGetOptions.IncludeProperties = true;
            });

        if (result.IsSuccess)
        {
            Entry = result.Value;
        }
        else
        {
            statusIndicator?.Show("Failed to load entry.", 
                StatusIndicator.StatusSeverity.Error);
        }
    }

    private async Task UpdateEntry(Dictionary<SchemaPropertyDto, object?> entry)
    {
        if (Entry is null)
            return;

        if (!await AuthHelper.CanEditEntry(EntryId.ToString()))
        {
            statusIndicator?.Show("You do not have access to this entry or it does not exist.",
                StatusIndicator.StatusSeverity.Error);
            return;
        }

        Entry.Id = EntryId.ToString();
        Entry.Properties = entry;

        var result = await EntryService.UpdateEntryAsync(Entry);

        if (result.IsSuccess)
        {
            statusIndicator?.Show("Entry updated successfully.",
                StatusIndicator.StatusSeverity.Success);
        }
        else
        {
            statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error.",
                StatusIndicator.StatusSeverity.Error);
        }
    }
}