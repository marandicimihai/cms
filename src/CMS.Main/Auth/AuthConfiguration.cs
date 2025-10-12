using System.Reflection.Metadata;
using CMS.Main.Auth.ApiKeyScheme;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CMS.Main.Auth;

public static class AuthConfiguration
{
    public static void ConfigureAuth(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthConstants.OwnsProject, policy =>
                policy.Requirements.Add(new OwnsProjectRequirement()))
            .AddPolicy(AuthConstants.OwnsSchema, policy =>
                policy.Requirements.Add(new OwnsSchemaRequirement()))
            .AddPolicy(AuthConstants.OwnsEntry, policy =>
                policy.Requirements.Add(new OwnsEntryRequirement()));
        services.AddScoped<IAuthorizationHandler, OwnsProjectHandler>();
        services.AddScoped<IAuthorizationHandler, OwnsSchemaHandler>();
        services.AddScoped<IAuthorizationHandler, OwnsEntryHandler>();

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