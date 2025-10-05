using CMS.Main.Abstractions.Entries;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.DTOs;
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

    [Inject]
    private INotificationService Notifications { get; set; } = default!;
    
    private EntryDto? Entry { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        if (!await AuthHelper.CanEditEntry(EntryId.ToString()))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        var result = await EntryService.GetEntryByIdAsync(EntryId.ToString());

        if (result.IsSuccess)
        {
            Entry = result.Value;
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

    private async Task UpdateEntry(Dictionary<string, object?> entry)
    {
        if (Entry is null)
            return;

        if (!await AuthHelper.CanEditEntry(EntryId.ToString()))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        Entry.Id = EntryId.ToString();
        Entry.Fields = entry;

        var result = await EntryService.UpdateEntryAsync(Entry);

        if (result.IsSuccess)
        {
            await Notifications.NotifyAsync(new()
            {
                Message = $"Updated entry with id {Entry.Id}.",
                Type = NotificationType.Info
            });
        }
        else
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