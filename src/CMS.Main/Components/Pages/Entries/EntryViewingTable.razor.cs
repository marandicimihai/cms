using CMS.Main.Abstractions.Entries;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs.Entry;
using CMS.Main.DTOs.Pagination;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Services;
using CMS.Main.Services.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using static CMS.Main.Components.Pages.Entries.SortAndFilterOptions;

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
    
    private List<EntryDto> SelectedEntries { get; set; } = [];
    private List<EntryDto> CurrentPageEntries { get; set; } = [];
    private SortAndFilterOptionsChangedEventArgs? cachedArgs;
    private StatusIndicator? statusIndicator;
    private QuickGrid<EntryDto>? quickGrid;
    private PaginationState pagination = new() { ItemsPerPage = 2 };

    private string? queuedStatusMessage;
    private StatusIndicator.StatusSeverity? queuedStatusSeverity;

    protected override void OnInitialized()
    {
        EntryStateService.EntriesCreated += EntriesCreated;
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

    // ItemsProvider for QuickGrid - handles pagination and fetching data
    private async ValueTask<GridItemsProviderResult<EntryDto>> LoadEntriesAsync(GridItemsProviderRequest<EntryDto> request)
    {
        try
        {
            // Calculate page number (1-based) from startIndex
            var pageNumber = (request.StartIndex / pagination.ItemsPerPage) + 1;
            var pageSize = request.Count ?? pagination.ItemsPerPage;

            var result = await EntryService.GetEntriesForSchema(
                SchemaId,
                new PaginationParams(pageNumber, pageSize),
                opt =>
                {
                    if (cachedArgs is not null)
                    {
                        opt.SortByPropertyName = cachedArgs.SortByProperty;
                        opt.Descending = cachedArgs.Descending;
                        opt.Filters = cachedArgs.Filters;
                    }
                    else
                    {
                        // Default sort
                        opt.SortByPropertyName = "CreatedAt";
                        opt.Descending = true;
                    }
                });

            if (result.IsSuccess)
            {
                var (entries, paginationInfo) = result.Value;
                CurrentPageEntries = entries;
                return GridItemsProviderResult.From(entries, paginationInfo.TotalCount);
            }
            else
            {
                queuedStatusMessage = result.Errors.FirstOrDefault() ?? "There was an error loading entries";
                queuedStatusSeverity = StatusIndicator.StatusSeverity.Error;
                return GridItemsProviderResult.From(new List<EntryDto>(), 0);
            }
        }
        catch (Exception)
        {
            return GridItemsProviderResult.From(new List<EntryDto>(), 0);
        }
    }

    // Called whenever the sort property or direction changes from SortAndFilterOptions
    private async Task OnOptionsChangedAsync(SortAndFilterOptionsChangedEventArgs args)
    {
        cachedArgs = args;
        
        // Refresh the grid to apply new sort/filter options
        if (quickGrid != null)
        {
            await quickGrid.RefreshDataAsync();
        }
    }

    private void EntriesCreated(List<EntryDto> created)
    {
        // When new entries are created, refresh the grid
        if (quickGrid != null && created?.Count > 0)
        {
            _ = quickGrid.RefreshDataAsync();
        }
    }
    
    private object? GetEntryPropertyValue(EntryDto entry, string propertyName)
    {
        foreach (var kv in entry.Fields)
        {
            if (kv.Key == propertyName)
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
            statusIndicator?.Show("Successfully deleted entry.", 
                StatusIndicator.StatusSeverity.Success);
            
            // Refresh the grid
            if (quickGrid != null)
            {
                await quickGrid.RefreshDataAsync();
            }
        }
        else
        {
            statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error", 
                StatusIndicator.StatusSeverity.Error);
        }
    }

    private void RedirectToEdit(string entryId)
    {
        NavigationManager.NavigateTo($"/entry/{entryId}/edit");
    }

    private bool IsAllSelected
    {
        get => CurrentPageEntries.Count > 0 && SelectedEntries.Count == CurrentPageEntries.Count && 
               CurrentPageEntries.All(e => SelectedEntries.Any(s => s.Id == e.Id));
        set => ToggleSelectAll(value);
    }

    private bool IsEntrySelected(EntryDto entry)
    {
        return SelectedEntries.Any(e => e.Id == entry.Id);
    }

    private void ToggleEntrySelection(EntryDto entry, bool selected)
    {
        if (selected)
        {
            if (SelectedEntries.All(e => e.Id != entry.Id))
                SelectedEntries.Add(entry);
        }
        else
        {
            SelectedEntries.RemoveAll(e => e.Id == entry.Id);
        }
        StateHasChanged();
    }

    private void ToggleSelectAllHandler(ChangeEventArgs e)
    {
        var selected = e.Value as bool? ?? false;
        ToggleSelectAll(selected);
    }

    private void ToggleSelectAll(bool selected)
    {
        if (selected)
        {
            // Select all entries on current page
            foreach (var entry in CurrentPageEntries)
            {
                if (SelectedEntries.All(e => e.Id != entry.Id))
                    SelectedEntries.Add(entry);
            }
        }
        else
        {
            // Deselect all entries on current page
            foreach (var entry in CurrentPageEntries)
            {
                SelectedEntries.RemoveAll(e => e.Id == entry.Id);
            }
        }
        StateHasChanged();
    }

    private void ClearSelection()
    {
        SelectedEntries.Clear();
        StateHasChanged();
    }

    private async Task DeleteSelectedEntriesAsync()
    {
        if (SelectedEntries.Count == 0)
            return;

        var isConfirmed = await ConfirmationService.ShowAsync(
            "Delete Entries",
            $"Are you sure you want to delete {SelectedEntries.Count} selected entr{(SelectedEntries.Count == 1 ? "y" : "ies")}? This action cannot be undone.",
            "Delete");

        if (!isConfirmed)
            return;

        var deletedCount = 0;
        foreach (var entry in SelectedEntries.ToList())
        {
            var result = await EntryService.DeleteEntryAsync(entry.Id);
            if (result.IsSuccess)
            {
                deletedCount++;
            }
        }

        SelectedEntries.Clear();
        
        if (deletedCount > 0)
        {
            statusIndicator?.Show($"Successfully deleted {deletedCount} entr{(deletedCount == 1 ? "y" : "ies")}.", 
                StatusIndicator.StatusSeverity.Success);
            
            // Refresh the grid
            if (quickGrid != null)
            {
                await quickGrid.RefreshDataAsync();
            }
        }
        
        StateHasChanged();
    }

    public void Dispose()
    {
        EntryStateService.EntriesCreated -= EntriesCreated;
        GC.SuppressFinalize(this);
    }
}