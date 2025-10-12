using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Schemas;

public partial class SchemaPage : ComponentBase
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

    private PropertyCreateForm? createForm;
    private PropertyUpdateForm? updateForm;
    
    private bool createFormVisible;
    private bool updateFormVisible;
    private bool hasAccess;

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

        hasAccess = true;

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

    private void ShowCreateForm()
    {
        createForm?.ResetForm();
        createFormVisible = true;
        updateFormVisible = false;
    }
    
    private void HideCreateForm()
    {
        createFormVisible = false;
    }
    
    private void ShowUpdateForm(PropertyDto property)
    {
        updateForm?.SetModel(property);
        updateFormVisible = true;
        createFormVisible = false;
    }
    
    private void HideUpdateForm()
    {
        updateFormVisible = false;
    }
}