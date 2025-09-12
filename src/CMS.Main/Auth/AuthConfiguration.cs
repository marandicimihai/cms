using CMS.Main.Auth.ApiKeyScheme;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CMS.Main.Auth;

public static class AuthConfiguration
{
    public static void ConfigureAuth(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("ProjectPolicies.CanEditProject", policy =>
                policy.Requirements.Add(new CanEditProjectRequirement()))
            .AddPolicy("SchemaPolicies.CanEditSchema", policy =>
                policy.Requirements.Add(new CanEditSchemaRequirement()))
            .AddPolicy("EntryPolicies.CanEditEntry", policy =>
                policy.Requirements.Add(new CanEditEntryRequirement()));
        services.AddScoped<IAuthorizationHandler, CanEditProjectHandler>();
        services.AddScoped<IAuthorizationHandler, CanEditSchemaHandler>();
        services.AddScoped<IAuthorizationHandler, CanEditEntryHandler>();
        
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddScheme<ApiKeySchemeOptions, ApiKeySchemeHandler>(
                AuthConstants.ApiKeyScheme, opts => { })
            .AddIdentityCookies();
    }
}