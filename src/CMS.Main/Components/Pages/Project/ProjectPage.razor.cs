using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Project;

public partial class ProjectPage : ComponentBase
{
    [Parameter]
    public Guid ProjectId { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;

    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    [SupplyParameterFromForm]
    private ProjectDto ProjectDto { get; set; } = default!;

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
            await ProjectService.GetProjectByIdAsync(ProjectId.ToString(), opt => { opt.IncludeSchemas = true; });

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

    private async Task OnSaveName()
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

        var result = await ProjectService.UpdateProjectAsync(ProjectDto);

        if (!result.IsSuccess)
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    $"There was an error when updating project named {ProjectDto.Name}.",
                Type = NotificationType.Error
            });
        }
    }

    private async Task OnDeleteProject()
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

        var confirmed = await ConfirmationService.ShowAsync(
            "Delete Project",
            "Are you sure you want to delete this project? This action cannot be undone.",
            "Delete",
            "Cancel"
        );

        if (confirmed)
        {
            var result = await ProjectService.DeleteProjectAsync(ProjectDto.Id);
            if (result.IsSuccess)
            {
                NavigationManager.NavigateTo("/");
            }
            else
            {
                await Notifications.NotifyAsync(new()
                {
                    Message = result.Errors.FirstOrDefault() ??
                        $"There was an error when deleting project named {ProjectDto.Name}.",
                    Type = NotificationType.Error
                });
            }
        }
    }
}