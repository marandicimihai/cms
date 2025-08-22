using CMS.Main.Client.Components;
using CMS.Main.Client.Services.State;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Entry;
using CMS.Shared.DTOs.Pagination;
using CMS.Shared.DTOs.SchemaProperty;
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
    private EntryStateService EntryStateService { get; set; } = default!;
    
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
            default:
                builder.AddContent(0, value.ToString());
                break;
        }
    };

    public void Dispose()
    {
        EntryStateService.EntriesCreated -= EntriesCreated;
        GC.SuppressFinalize(this);
    }
}