using System.Security.Claims;
using CMS.Main.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Auth;

public class OwnsEntryHandler(
    IDbContextConcurrencyHelper dbHelper
) : AuthorizationHandler<OwnsEntryRequirement, string>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        OwnsEntryRequirement requirement, 
        string entryId)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var entry = await dbHelper.ExecuteAsync(async dbContext => 
                        await dbContext.Entries
                            .AsNoTracking()
                            .Include(e => e.Schema)
                            .ThenInclude(e => e.Project)
                            .FirstOrDefaultAsync(e => e.Id == entryId));
                
                var ownerId = entry?.Schema.Project.OwnerId;

                if (ownerId == userId)
                {
                    // If authenticated via API key, enforce project scope
                    var apiKeyProjectId = context.User.FindFirst(AuthConstants.ProjectIdClaimType)?.Value;
                    if (!string.IsNullOrEmpty(apiKeyProjectId))
                    {
                        // API key authentication: must match the entry's project
                        if (apiKeyProjectId == entry?.Schema.Project.Id)
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