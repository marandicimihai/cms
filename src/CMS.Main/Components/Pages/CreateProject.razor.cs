using Microsoft.AspNetCore.Components;
using CMS.Shared.DTOs.Project;
using System.Security.Claims;
using CMS.Main.Client.Components.Shared;
using CMS.Shared.Abstractions;

namespace CMS.Main.Components.Pages;

public partial class CreateProject : ComponentBase
{
    [SupplyParameterFromForm]
    private ProjectCreationDto ProjectDto { get; set; } = new();
    
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    private StatusIndicator? statusIndicator;
    private string? statusText;
    private bool isLoading;

    protected override async Task OnInitializedAsync()
    {
        // Get the current user's ID and set it in the DTO
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                ProjectDto.OwnerId = userId;
            }
        }
    }

    private async Task HandleValidSubmit()
    {
        isLoading = true;
        StateHasChanged();
        await Task.Yield();
        try
        {
            var result = await ProjectService.CreateProjectAsync(ProjectDto);

            if (!result.IsSuccess)
            {
                statusText = "Something went wrong when creating the project."; 
                statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
                return;
            }
            
            statusText = "The project was successfully created."; 
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Success);
        }
        catch
        {
            statusText = "Something went wrong when creating the project.";
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
        }
        finally
        {
            isLoading = false;
        }
    }
}