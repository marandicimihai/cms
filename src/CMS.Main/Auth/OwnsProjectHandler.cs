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
                    // If authenticated via API key, enforce project scope
                    var apiKeyProjectId = context.User.FindFirst(AuthConstants.ProjectIdClaimType)?.Value;
                    if (!string.IsNullOrEmpty(apiKeyProjectId))
                    {
                        // API key authentication: must match the key's project
                        if (apiKeyProjectId == projectId)
                        {
                            context.Succeed(requirement);
                        }
                        else
                        {
                            context.Fail();
                        }
                        return;
                    }
                    
                    // Regular user authentication: user owns the project
                    context.Succeed(requirement);
                    return;
                }
            }
        }

        context.Fail();
    }
}