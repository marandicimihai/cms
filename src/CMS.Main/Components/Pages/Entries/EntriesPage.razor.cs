using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Entries;

public partial class EntriesPage : ComponentBase
{
    [Parameter]
    public Guid SchemaId { get; set; }
    
    private SchemaDto Schema { get; set; } = new();
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    private EntryCreateModal? createEntryModal;

    protected override async Task OnInitializedAsync()
    {
        if (!await AuthHelper.OwnsSchema(SchemaId.ToString()))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        var result = await SchemaService.GetSchemaByIdAsync(SchemaId.ToString());

        if (result.IsSuccess)
        {
            Schema = result.Value;
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
    
    private void OpenCreateEntryModal()
    {
        createEntryModal?.Open();
    }

    private void HandleEntryCreated()
    {
        StateHasChanged();
    }
}