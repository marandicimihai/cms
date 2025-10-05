using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace CMS.Main.Services;

public class AuthorizationHelperService(
    IAuthorizationService authorizationService,
    AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<bool> OwnsProject(string projectId)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var authorizationResult =
            await authorizationService.AuthorizeAsync(user, projectId, "ProjectPolicies.CanEditProject");

        return authorizationResult.Succeeded;
    }
    
    public async Task<bool> OwnsSchema(string schemaId)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var authorizationResult =
            await authorizationService.AuthorizeAsync(user, schemaId, "SchemaPolicies.CanEditSchema");

        return authorizationResult.Succeeded;
    }
    
    public async Task<bool> OwnsEntry(string entryId)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var authorizationResult =
            await authorizationService.AuthorizeAsync(user, entryId, "EntryPolicies.CanEditEntry");

        return authorizationResult.Succeeded;
    }
}