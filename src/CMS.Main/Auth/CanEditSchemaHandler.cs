using System.Security.Claims;
using CMS.Shared.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Auth;

public class CanEditSchemaHandler(
    ISchemaService schemaService
) : AuthorizationHandler<CanEditSchemaRequirement, string>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CanEditSchemaRequirement requirement, 
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
                    context.Succeed(requirement);
                    return;
                }
            }
        }

        context.Fail();
    }
}