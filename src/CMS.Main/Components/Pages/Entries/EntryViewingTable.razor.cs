using CMS.Main.Abstractions.Entries;
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
    private List<EntryDto> SelectedEntries { get; set; } = [];

    // Sorting options (backing fields call shared handler when updated)
    private string sortByPropertyBacking = "CreatedAt";
    private string SortByProperty
    {
        get => sortByPropertyBacking;
        set
        {
            if (value == sortByPropertyBacking) return;
            sortByPropertyBacking = value;
            _ = InvokeAsync(OnSortChangedAsync);
        }
    }

    private bool descendingBacking = false;
    private bool Descending
    {
        get => descendingBacking;
        set
        {
            if (value == descendingBacking) return;
            descendingBacking = value;
            _ = InvokeAsync(OnSortChangedAsync);
        }
    }
    
    private StatusIndicator? statusIndicator;
    
    // Pagination state (mirrors sidebar pattern)
    private readonly int pageSize = 20;
    private bool isLoadingMore;
    private int totalCount;
    private bool HasMoreEntries => Entries.Count < totalCount;

    private string? queuedStatusMessage;
    private StatusIndicator.StatusSeverity? queuedStatusSeverity;

    protected override async Task OnInitializedAsync()
    {
        EntryStateService.EntriesCreated += EntriesCreated;
        
        var result = await EntryService.GetEntriesForSchema(
            SchemaId,
            new PaginationParams(1, pageSize));

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

        // Default the selected property to the first available property name if not already set
        if (SortByProperty == null && Properties != null && Properties.Count > 0)
        {
            SortByProperty = Properties[0].Name;
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

    // Called whenever the sort property or direction changes
    private async Task OnSortChangedAsync()
    {
        var result = await EntryService.GetEntriesForSchema(
            SchemaId,
            new PaginationParams(1, pageSize),
            opt =>
            {
                opt.PropertyName = SortByProperty;
                opt.Descending = Descending;
            });

        if (result.IsSuccess)
        {
            (Entries, var pagination) = result.Value;
            StateHasChanged();
            totalCount = pagination.TotalCount;
        }
        else
        {
            queuedStatusMessage = result.Errors.FirstOrDefault() ?? "There was an error";
            queuedStatusSeverity = StatusIndicator.StatusSeverity.Error;
        }
    }
    
    private void EntriesCreated(List<EntryDto> created)
    {
        // Prepend new entries; keep newest-first order consistent with sort
        Entries.InsertRange(0, created);
        // Keep pagination total in sync when new entries are created elsewhere
        if (created?.Count > 0)
        {
            totalCount = Math.Max(0, totalCount + created.Count);
        }
        StateHasChanged();
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
            Entries.Remove(entry);
            // Keep pagination metadata in sync with user-visible list
            totalCount = Math.Max(0, totalCount - 1);
            statusIndicator?.Show("Successfully deleted entry.", 
                StatusIndicator.StatusSeverity.Success);
        }
        else
        {
            statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error", 
                StatusIndicator.StatusSeverity.Error);
        }
    }

    private async Task LoadMoreEntriesAsync()
    {
        if (isLoadingMore || !HasMoreEntries) return;
        isLoadingMore = true;
        StateHasChanged();
        await Task.Yield();

        try
        {
            // Calculate next page (1-based)
            var nextPage = (Entries.Count / pageSize) + 1;
            var result = await EntryService.GetEntriesForSchema(
                SchemaId,
                new PaginationParams(nextPage, pageSize));

            if (result.IsSuccess)
            {
                var (newEntries, pagination) = result.Value;
                foreach (var e in newEntries)
                {
                    if (Entries.All(existing => existing.Id != e.Id))
                        Entries.Add(e);
                }
                totalCount = pagination.TotalCount;
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            isLoadingMore = false;
            StateHasChanged();
        }
    }

    private void RedirectToEdit(string entryId)
    {
        NavigationManager.NavigateTo($"/entry/{entryId}/edit");
    }

    private bool IsAllSelected
    {
        get => Entries.Count > 0 && SelectedEntries.Count == Entries.Count;
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
            SelectedEntries = Entries.ToList();
        }
        else
        {
            SelectedEntries.Clear();
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
                Entries.RemoveAll(e => e.Id == entry.Id);
                deletedCount++;
            }
            // Optionally, show error for failed deletes
        }

        // Decrease totalCount by the number of successfully deleted entries
        if (deletedCount > 0)
        {
            totalCount = Math.Max(0, totalCount - deletedCount);
        }

        SelectedEntries.Clear();
        StateHasChanged();
    }

    public void Dispose()
    {
        EntryStateService.EntriesCreated -= EntriesCreated;
        GC.SuppressFinalize(this);
    }
}