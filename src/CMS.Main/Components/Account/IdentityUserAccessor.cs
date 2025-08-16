using CMS.Main.Data;
using Microsoft.AspNetCore.Identity;

namespace CMS.Main.Components.Account;

internal sealed class IdentityUserAccessor(
    UserManager<ApplicationUser> userManager,
    IdentityRedirectManager redirectManager,
    DbContextConcurrencyHelper concurrencyHelper)
{
    public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await concurrencyHelper.ExecuteAsync(_ => userManager.GetUserAsync(context.User));

        if (user is null)
            redirectManager.RedirectToWithStatus("Account/InvalidUser",
                $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);

        return user;
    }
}