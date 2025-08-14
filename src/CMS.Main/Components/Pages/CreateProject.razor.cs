using Microsoft.AspNetCore.Components;
using CMS.Shared.DTOs.Project;
using System.Security.Claims;

namespace CMS.Main.Components.Pages;

public partial class CreateProject : ComponentBase
{
    [SupplyParameterFromForm]
    private ProjectCreationDto ProjectDto { get; set; } = new();

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
        
    }
}