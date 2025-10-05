using CMS.Main.Abstractions.Entries;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.Abstractions.Properties.PropertyTypes;
using CMS.Main.DTOs;
using CMS.Main.DTOs.Pagination;
using CMS.Main.Services;
using CMS.Main.Services.State;
using Microsoft.AspNetCore.Components;
using static CMS.Main.Components.Pages.Entries.SortAndFilterOptions;

namespace CMS.Main.Components.Pages.Entries;

public partial class EntryViewingTable : ComponentBase, IDisposable
{
    [Parameter, EditorRequired]
    public string SchemaId { get; set; } = default!;

    [Parameter, EditorRequired]
    public List<PropertyDto> Properties { get; set; } = default!;
    
    [Inject]
    private IEntryService EntryService { get; set; } = default!;
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;

    [Inject]
    private EntryStateService EntryStateService { get; set; } = default!;
    
    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    
    private List<EntryDto> Entries { get; set; } = [];
    private List<EntryDto> SelectedEntries { get; set; } = [];

    private SortAndFilterOptionsChangedEventArgs? cachedArgs;

    private List<string> SortableProperties => Properties
        .Where(p => p.Type == PropertyType.Text || p.Type == PropertyType.Number || p.Type == PropertyType.DateTime)
        .Select(p => p.Name)
        .Append("CreatedAt")
        .Append("UpdatedAt")
        .ToList();
    
    private readonly int pageSize = 20;
    private bool isLoadingMore;
    private int totalCount;
    private bool HasMoreEntries => Entries.Count < totalCount;

    protected override async Task OnInitializedAsync()
    {
        // TODO: Add auth here
        
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
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    "Could not retrieve resource.",
                Type = NotificationType.Error
            });
        }
    }

    // Called whenever the sort property or direction changes
    private async Task OnOptionsChangedAsync(SortAndFilterOptionsChangedEventArgs args)
    {
        // TODO: Add auth here

        var result = await EntryService.GetEntriesForSchema(
            SchemaId,
            new PaginationParams(1, pageSize),
            opt =>
            {
                opt.SortByPropertyName = args.SortByProperty;
                opt.Descending = args.Descending;
                opt.Filters = args.Filters;
            });

        cachedArgs = args;

        if (result.IsSuccess)
        {
            (Entries, var pagination) = result.Value;
            StateHasChanged();
            totalCount = pagination.TotalCount;
        }
        else
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    "Could not retrieve resource.",
                Type = NotificationType.Error
            });
        }
    }

    private async Task LoadMoreEntriesAsync()
    {
        // TODO: Add auth here

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
                new PaginationParams(nextPage, pageSize),
                opt =>
                {
                    if (cachedArgs is not null)
                    {
                        opt.SortByPropertyName = cachedArgs.SortByProperty;
                        opt.Descending = cachedArgs.Descending;
                        opt.Filters = cachedArgs.Filters;
                    }
                });

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
            throw;
        }
        finally
        {
            isLoadingMore = false;
            StateHasChanged();
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
        if (!await AuthHelper.OwnsSchema(SchemaId))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "You do not have permission to delete entries.",
                Type = NotificationType.Error
            });
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
            await Notifications.NotifyAsync(new()
            {
                Message = $"Deleted entry with id {entry.Id}.",
                Type = NotificationType.Info
            });
        }
        else
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    $"There was an error when deleting entry with id {entry.Id}.",
                Type = NotificationType.Error
            });
        }
    }

    private bool IsAllSelected
    {
        get => Entries.Count > 0 && SelectedEntries.Count == Entries.Count;
        set => ToggleSelectAll(value);
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
        // TODO: Add auth here

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