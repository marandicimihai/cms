using System.Security.Claims;
using CMS.Main.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Auth;

public class OwnsSchemaHandler(
    ISchemaService schemaService
) : AuthorizationHandler<OwnsSchemaRequirement, string>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        OwnsSchemaRequirement requirement, 
        string schemaId)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var result = await schemaService.GetSchemaByIdAsync(schemaId,
                    opt => opt.IncludeProject = true);

                if (result.IsSuccess && result.Value.Project.OwnerId == userId)
                {
                    // If authenticated via API key, enforce project scope
                    var apiKeyProjectId = context.User.FindFirst(AuthConstants.ProjectIdClaimType)?.Value;
                    if (!string.IsNullOrEmpty(apiKeyProjectId))
                    {
                        // API key authentication: must match the schema's project
                        if (apiKeyProjectId == result.Value.Project.Id)
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