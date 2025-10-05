using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Entries;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs;
using CMS.Main.Services;
using CMS.Main.Services.State;
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
    private IEntryService EntryService { get; set; } = default!;

    [Inject]
    private EntryStateService EntryStateService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    private DynamicEntryForm? entryCreateForm;
    
    private bool showCreateForm;

    protected override async Task OnInitializedAsync()
    {
        if (!await AuthHelper.CanEditSchema(SchemaId.ToString()))
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
    
    private async Task OnEntryCreateSubmit(Dictionary<string, object?> entry)
    {
        if (!await AuthHelper.CanEditSchema(SchemaId.ToString()))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        var dto = new EntryDto
        {
            SchemaId = SchemaId.ToString(),
            Fields = entry
        };

        var result = await EntryService.AddEntryAsync(dto);

        if (result.IsSuccess)
        {
            EntryStateService.NotifyCreated([result.Value]);

            await Notifications.NotifyAsync(new()
            {
                Message = $"Created entry with id {result.Value.Id}.",
                Type = NotificationType.Info
            });
            showCreateForm = false;

            entryCreateForm?.Reset();
            StateHasChanged();
        }
        else
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    "There was an error when creating the entry.",
                Type = NotificationType.Error
            });
        }
    }

    private void ShowAddForm()
    {
        entryCreateForm?.Reset();
        showCreateForm = true;
    }
    
    private void HideAddForm()
    {
        showCreateForm = false;
    }
}