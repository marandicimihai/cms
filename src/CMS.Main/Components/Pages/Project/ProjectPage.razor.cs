using CMS.Main.Client.Components;
using CMS.Main.Services;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Project;
using Mapster;
using Microsoft.AspNetCore.Authorization;
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
    private ConfirmationService ConfirmationService { get; set; } = default!;
    
    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [SupplyParameterFromForm]
    private ProjectUpdateDto ProjectDto { get; set; } = new();

    private StatusIndicator? statusIndicator;
    private string? statusText;
    private bool showStatusOnRender;

    protected override async Task OnInitializedAsync()
    {
        if (!await IsOwner())
        {
            statusText = "Could not load project. You do not have permission to access this project.";
            showStatusOnRender = true;
            return;
        }
        
        var result = await ProjectService.GetProjectByIdAsync(ProjectId.ToString(), opt =>
        {
            opt.IncludeSchemas = true;
        });
        
        if (result.IsSuccess)
        {
            ProjectDto = result.Value.Adapt<ProjectUpdateDto>();
        }
        else
        {
            statusText = result.Errors.First();
            showStatusOnRender = true;
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (showStatusOnRender && statusIndicator != null)
        {
            statusIndicator.Show(StatusIndicator.StatusSeverity.Error);
            showStatusOnRender = false;
            StateHasChanged();
        }
    }

    private async Task OnSaveName()
    {
        if (!await IsOwner())
            return;

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
        if (!await IsOwner())
            return;

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

    private async Task<bool> IsOwner()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var authorizationResult = await AuthorizationService.AuthorizeAsync(user, ProjectId.ToString(), "MustOwnProject");

        if (authorizationResult.Succeeded) return true;
        
        statusText = "Could not load project. You do not have permission to access this project.";
        statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
        
        return false;
    }
}