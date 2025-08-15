using System.Security.Claims;
using CMS.Main.Client.Components;
using CMS.Main.Services;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Project;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

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
    private ConfirmationService ConfirmationService { get; set; } = default!;

    [SupplyParameterFromForm]
    private ProjectUpdateDto ProjectDto { get; set; } = new();

    private StatusIndicator? statusIndicator;
    private string? statusText;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var result = await ProjectService.GetProjectByIdAsync(ProjectId.ToString());
                
                if (result.IsSuccess && result.Value.OwnerId == userId)
                {
                    ProjectDto = result.Value.Adapt<ProjectUpdateDto>();
                }
                else
                {
                    statusText = result.Errors.First();
                    statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
                }

                return;
            }
        }
        
        statusText = "Project could not be retrieved because you are not authenticated. Please log in";
        statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
    }

    private async Task OnSaveName()
    {
        var result = await ProjectService.UpdateProjectAsync(ProjectDto);

        if (result.IsSuccess)
        {
            statusText = "Project updated successfully.";
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Success);
        }
        else
        {
            statusText = result.Errors.First();
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
        }
    }

    private async Task OnDeleteProject()
    {
        var confirmed = await ConfirmationService.ShowAsync(
            title: "Delete Project",
            message: "Are you sure you want to delete this project? This action cannot be undone.",
            confirmText: "Delete",
            cancelText: "Cancel"
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
                statusText = result.Errors.First();
                statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
            }
        }
    }
}