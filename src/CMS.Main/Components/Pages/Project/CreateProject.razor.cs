using System.Security.Claims;
using CMS.Main.Client.Components;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Project;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Project;

public partial class CreateProject : ComponentBase
{
    private bool isLoading;

    private string? projectUrl;

    [SupplyParameterFromForm]
    private ProjectCreationDto ProjectDto { get; set; } = new();

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    private StatusIndicator? statusIndicator;

    protected override async Task OnInitializedAsync()
    {
        // Get the current user's ID and set it in the DTO
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId)) ProjectDto.OwnerId = userId;
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
                statusIndicator?.Show("Something went wrong when creating the project.",
                    StatusIndicator.StatusSeverity.Error);
                return;
            }

            statusIndicator?.Show("The project was successfully created.",
                StatusIndicator.StatusSeverity.Success);

            projectUrl = $"/project/{result.Value.Id}";
        }
        catch
        {
            statusIndicator?.Show("Something went wrong when creating the project.",
                StatusIndicator.StatusSeverity.Error);
        }
        finally
        {
            isLoading = false;
        }
    }
}