using System.Security.Claims;
using CMS.Main.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Auth;

public class OwnsProjectHandler(IProjectService projectService)
    : AuthorizationHandler<OwnsProjectRequirement, string>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OwnsProjectRequirement requirement, string projectId)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var result = await projectService.GetProjectByIdAsync(projectId);

                if (result.IsSuccess && result.Value.OwnerId == userId)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }

        context.Fail();
    }
}