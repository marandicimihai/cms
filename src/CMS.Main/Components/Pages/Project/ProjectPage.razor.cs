using CMS.Main.Abstractions;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs.Project;
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

    [SupplyParameterFromForm]
    private ProjectDto ProjectDto { get; set; } = new();

    private StatusIndicator? statusIndicator;

    private string? queuedStatusMessage;
    private StatusIndicator.StatusSeverity? queuedStatusSeverity;

    protected override async Task OnInitializedAsync()
    {
        if (!await AuthHelper.CanEditProject(ProjectId.ToString()))
        {
            queuedStatusMessage = "You do not have access to this resource or it does not exist.";
            queuedStatusSeverity = StatusIndicator.StatusSeverity.Error;
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
            queuedStatusMessage = result.Errors.FirstOrDefault() ?? "There was an error";
            queuedStatusSeverity = StatusIndicator.StatusSeverity.Error;
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (queuedStatusMessage != null && queuedStatusSeverity != null)
        {
            statusIndicator?.Show(queuedStatusMessage, queuedStatusSeverity.Value);
            queuedStatusMessage = null;
            queuedStatusSeverity = null;
        }
    }

    private async Task OnSaveName()
    {
        if (!await AuthHelper.CanEditProject(ProjectId.ToString()))
        {
            statusIndicator?.Show("You do not have access to this project or it does not exist.",
                StatusIndicator.StatusSeverity.Error);
            return;
        }

        var result = await ProjectService.UpdateProjectAsync(ProjectDto);

        if (result.IsSuccess)
        {
            statusIndicator?.Show("Project updated successfully.",
                StatusIndicator.StatusSeverity.Success);
        }
        else
        {
            statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error",
                StatusIndicator.StatusSeverity.Error);
        }
    }

    private async Task OnDeleteProject()
    {
        if (!await AuthHelper.CanEditProject(ProjectId.ToString()))
        {
            statusIndicator?.Show("You do not have access to this project or it does not exist.",
                StatusIndicator.StatusSeverity.Error);
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
                statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error",
                    StatusIndicator.StatusSeverity.Error);
            }
        }
    }
}