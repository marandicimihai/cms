using CMS.Main.Abstractions;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs.Entry;
using CMS.Main.DTOs.Pagination;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Services;
using CMS.Main.Services.State;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Entries;

public partial class EntryViewingTable : ComponentBase, IDisposable
{
    [Parameter, EditorRequired]
    public string SchemaId { get; set; } = default!;

    [Parameter, EditorRequired]
    public List<SchemaPropertyDto> Properties { get; set; } = default!;
    
    [Inject]
    private IEntryService EntryService { get; set; } = default!;
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;

    [Inject]
    private EntryStateService EntryStateService { get; set; } = default!;
    
    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    
    private List<EntryDto> Entries { get; set; } = [];
    
    private StatusIndicator? statusIndicator;
    
    // Used later for loading more entries
    private int totalCount;

    private string? queuedStatusMessage;
    private StatusIndicator.StatusSeverity? queuedStatusSeverity;

    protected override async Task OnInitializedAsync()
    {
        EntryStateService.EntriesCreated += EntriesCreated;
        
        var result = await EntryService.GetEntriesForSchema(
            SchemaId,
            new PaginationParams(1, 100),
            opt =>
            {
                opt.SortingOption = EntrySortingOption.CreatedAt;
                opt.Descending = true;
            });

        if (result.IsSuccess)
        {
            (Entries, var pagination) = result.Value;
            totalCount = pagination.TotalCount;
        }
        else
        {
            queuedStatusMessage = result.Errors.FirstOrDefault() ?? "There was an error";
            queuedStatusSeverity = StatusIndicator.StatusSeverity.Error;
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (queuedStatusMessage != null && queuedStatusSeverity != null)
        {
            statusIndicator?.Show(queuedStatusMessage, queuedStatusSeverity.Value);
            queuedStatusMessage = null;
            queuedStatusSeverity = null;
        }
    }
    
    private void EntriesCreated(List<EntryDto> created)
    {
        Entries.InsertRange(0, created);
    }
    
    private object? GetEntryPropertyValue(EntryDto entry, string propertyName)
    {
        foreach (var kv in entry.Properties)
        {
            if (kv.Key.Name == propertyName)
                return kv.Value;
        }
        return null;
    }

    private RenderFragment FormatCell(object? value) => builder =>
    {
        switch (value)
        {
            case null:
                builder.AddContent(0, "null");
                return;
            case string s:
            {
                // Always parse as UTC and convert to local time
                if (DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out var dt))
                {
                    var display = dt.ToLocalTime().ToString("g");
                    builder.OpenElement(0, "span");
                    builder.AddAttribute(1, "class", "whitespace-pre");
                    builder.AddContent(2, display);
                    builder.CloseElement();
                }
                else
                {
                    builder.AddContent(0, s);
                }
                return;
            }
            case bool b:
                builder.AddContent(0, b.ToString().ToLowerInvariant());
                return;
            case DateTime d:
            {
                var display = d.ToLocalTime().ToString("g");
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "whitespace-pre");
                builder.AddContent(2, display);
                builder.CloseElement();
                break;
            }
            default:
                builder.AddContent(0, value.ToString());
                break;
        }
    };

    private async Task OnDeleteEntry(EntryDto entry)
    {
        if (!await AuthHelper.CanEditSchema(SchemaId))
        {
            statusIndicator?.Show("You can not delete entries.", 
                StatusIndicator.StatusSeverity.Error);
            return;
        }
        
        var isConfirmed = await ConfirmationService.ShowAsync(
            "Delete Entry",
            "Are you sure you want to delete this entry? This action cannot be undone.",
            "Delete");

        if (!isConfirmed)
            return;
        
        var result = await EntryService.DeleteEntryAsync(entry.Id);
        
        if (result.IsSuccess)
        {
            Entries.Remove(entry);
            statusIndicator?.Show("Successfully deleted entry.", 
                StatusIndicator.StatusSeverity.Success);
        }
        else
        {
            statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error", 
                StatusIndicator.StatusSeverity.Error);
        }
    }

    private void EditEntry(string entryId)
    {
        NavigationManager.NavigateTo($"/entry/{entryId}/edit");
    }

    public void Dispose()
    {
        EntryStateService.EntriesCreated -= EntriesCreated;
        GC.SuppressFinalize(this);
    }
}