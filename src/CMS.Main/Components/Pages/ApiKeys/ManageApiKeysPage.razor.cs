using CMS.Main.Abstractions;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs;
using CMS.Main.Abstractions.Notifications;

namespace CMS.Main.Components.Pages.ApiKeys;

public partial class ManageApiKeysPage : ComponentBase
{
    [Parameter]
    public Guid ProjectId { get; set; }

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;
    
    private ProjectDto? ProjectDto { get; set; } = new();
    
    private ApiKeyCreateForm? createForm;
    
    // queued status for showing after render when initialized
    private bool createFormVisible;

    protected override async Task OnInitializedAsync()
    {
        if (!await AuthHelper.OwnsProject(ProjectId.ToString()))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        var result =
            await ProjectService.GetProjectByIdAsync(ProjectId.ToString(), opt => { opt.IncludeApiKeys = true; });

        if (result.IsSuccess)
        {
            ProjectDto = result.Value;
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
    }
    
    private void HideCreateForm() => createFormVisible = false;
}