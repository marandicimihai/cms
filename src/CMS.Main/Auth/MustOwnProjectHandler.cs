using System.Security.Claims;
using CMS.Shared.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Auth;

public class MustOwnProjectHandler(IProjectService projectService)
    : AuthorizationHandler<MustOwnProjectRequirement, string>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        MustOwnProjectRequirement requirement, string projectId)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var ownershipResult = await projectService.OwnsProject(userId, projectId);

                if (ownershipResult is { IsSuccess: true, Value: true })
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }

        context.Fail();
    }
}