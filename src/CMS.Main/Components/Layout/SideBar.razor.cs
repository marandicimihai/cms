using System.Security.Claims;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Pagination;
using CMS.Shared.DTOs.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Layout;

[StreamRendering]
[Authorize]
public partial class SideBar : ComponentBase
{
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;
    
    private List<ProjectWithIdDto> Projects { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Get the current user's ID and set it in the DTO
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var result = await ProjectService.GetProjectsForUserAsync(
                        userId,
                        new PaginationParams(1, 20));

                    if (result.IsSuccess)
                    {
                        Projects = result.Value.Item1;
                    }
                }
            }
        }
        catch
        {
            // ignored
        }
    }
}