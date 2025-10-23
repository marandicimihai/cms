using CMS.Main.Abstractions.Entries;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs;
using CMS.Main.Services;
using CMS.Main.Services.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CMS.Main.Components.Pages.Entries;

public partial class EntryCreateModal : ComponentBase
{
    private bool _isOpen;
    
    [Parameter, EditorRequired]
    public Guid SchemaId { get; set; }
    
    [Parameter, EditorRequired]
    public List<PropertyDto> Properties { get; set; } = [];

    [Parameter]
    public EventCallback OnEntryCreated { get; set; }

    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;

    [Inject]
    private IEntryService EntryService { get; set; } = default!;

    [Inject]
    private EntryStateService EntryStateService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    private DynamicEntryForm? entryCreateForm;

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen != value)
            {
                _isOpen = value;
                if (_isOpen)
                {
                    ResetForm();
                }
                StateHasChanged();
            }
        }
    }

    public void Open()
    {
        IsOpen = true;
    }

    public void CloseModal()
    {
        IsOpen = false;
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (!IsOpen) return;
        if (e.Key == "Escape")
        {
            CloseModal();
        }
    }

    private void ResetForm()
    {
        entryCreateForm?.Reset();
    }

    private async Task HandleCreateSubmit(Dictionary<string, object?> entry)
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
                Message = "Entry created successfully.",
                Type = NotificationType.Success
            });

            CloseModal();
            await OnEntryCreated.InvokeAsync();
        }
        else if (result.Errors is not null)
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    "There was an error when creating the entry.",
                Type = NotificationType.Error
            });
        }
    }
}
