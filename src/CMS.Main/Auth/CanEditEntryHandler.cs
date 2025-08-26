using System.Security.Claims;
using CMS.Main.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Auth;

public class CanEditEntryHandler(
    IDbContextConcurrencyHelper dbHelper
) : AuthorizationHandler<CanEditEntryRequirement, string>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CanEditEntryRequirement requirement, 
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
                    context.Succeed(requirement);
                    return;
                }
            }
        }

        context.Fail();
    }
}