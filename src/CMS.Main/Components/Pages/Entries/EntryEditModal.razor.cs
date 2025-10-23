using CMS.Main.Abstractions.Entries;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs;
using CMS.Main.Services;
using CMS.Main.Services.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CMS.Main.Components.Pages.Entries;

public partial class EntryEditModal : ComponentBase
{
    private bool _isOpen;
    private EntryDto? _entry;

    [Parameter]
    public EventCallback OnEntryUpdated { get; set; }

    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;

    [Inject]
    private IEntryService EntryService { get; set; } = default!;

    [Inject]
    private EntryStateService EntryStateService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    private DynamicEntryForm? entryEditForm;

    public EntryDto? Entry
    {
        get => _entry;
        private set
        {
            _entry = value;
            StateHasChanged();
        }
    }

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen != value)
            {
                _isOpen = value;
                StateHasChanged();
            }
        }
    }

    public async Task OpenAsync(string entryId)
    {
        if (!await AuthHelper.OwnsEntry(entryId))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        var result = await EntryService.GetEntryByIdAsync(entryId);

        if (result.IsSuccess)
        {
            Entry = result.Value;
            IsOpen = true;
        }
        else
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ?? "Could not retrieve resource.",
                Type = NotificationType.Error
            });
        }
    }

    public void CloseModal()
    {
        IsOpen = false;
        Entry = null;
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (!IsOpen) return;
        if (e.Key == "Escape")
        {
            CloseModal();
        }
    }

    private async Task HandleUpdateSubmit(Dictionary<string, object?> fields)
    {
        if (Entry is null)
            return;

        if (!await AuthHelper.OwnsEntry(Entry.Id))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        Entry.Fields = fields;

        var result = await EntryService.UpdateEntryAsync(Entry);

        if (result.IsSuccess)
        {
            EntryStateService.NotifyUpdated([Entry]);

            await Notifications.NotifyAsync(new()
            {
                Message = "Entry updated successfully.",
                Type = NotificationType.Success
            });

            CloseModal();
            await OnEntryUpdated.InvokeAsync();
        }
        else if (result.Errors is not null)
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    $"There was an error when updating entry with id {Entry.Id}.",
                Type = NotificationType.Error
            });
        }
    }
}
