using CMS.Main.Client.Components;
using CMS.Main.Client.Services.State;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace CMS.Main.Client.Services;

public class AuthorizationHelperService(
    IAuthorizationService authorizationService,
    AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<bool> CanAccessProject(string projectId)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var authorizationResult =
            await authorizationService.AuthorizeAsync(user, projectId, "ProjectPolicies.CanEditProject");

        return authorizationResult.Succeeded;
    }
}