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
    
    private EntryDto Entry { get; set; } = new();
    
    private DynamicEntryForm? entryForm;
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

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            entryForm?.SetValues(Entry.Properties);
        }
    }

    private async Task UpdateEntry(Dictionary<SchemaPropertyDto, object?> entry)
    {
        if (!await AuthHelper.CanEditEntry(EntryId.ToString()))
        {
            statusIndicator?.Show("You do not have access to this entry or it does not exist.", 
                StatusIndicator.StatusSeverity.Error);
            return;
        }
    }
}