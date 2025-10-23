using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Schemas;

public partial class SchemaPage : ComponentBase
{
    [Parameter]
    public Guid SchemaId { get; set; }

    private SchemaDto? Schema { get; set; }
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;
    
    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    private PropertyCreateForm? createPropertyModal;
    private PropertyUpdateForm? updatePropertyModal;

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

    private void OpenCreatePropertyModal()
    {
        createPropertyModal?.Open();
    }
    
    private async Task HandlePropertyCreated()
    {
        // Reload the schema to get the updated properties list
        var result = await SchemaService.GetSchemaByIdAsync(SchemaId.ToString());
        if (result.IsSuccess)
        {
            Schema = result.Value;
            StateHasChanged();
        }
    }
    
    private void OpenUpdatePropertyModal(PropertyDto property)
    {
        updatePropertyModal?.Open(property);
    }
    
    private async Task HandlePropertyUpdated()
    {
        // Reload the schema to get the updated properties list
        var result = await SchemaService.GetSchemaByIdAsync(SchemaId.ToString());
        if (result.IsSuccess)
        {
            Schema = result.Value;
            StateHasChanged();
        }
    }
}