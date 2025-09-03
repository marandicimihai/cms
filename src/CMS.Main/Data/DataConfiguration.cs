using CMS.Main.Auth;
using CMS.Main.Components.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Data;

public static class DataConfiguration
{
    public static void ConfigureDataServices(this IServiceCollection services, string connectionString)
    {
        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ProjectPolicies.CanEditProject", policy =>
                policy.Requirements.Add(new CanEditProjectRequirement()));
            options.AddPolicy("SchemaPolicies.CanEditSchema", policy =>
                policy.Requirements.Add(new CanEditSchemaRequirement()));
            options.AddPolicy("EntryPolicies.CanEditEntry", policy =>
                policy.Requirements.Add(new CanEditEntryRequirement()));
        });
        services.AddScoped<IAuthorizationHandler, CanEditProjectHandler>();
        services.AddScoped<IAuthorizationHandler, CanEditSchemaHandler>();
        services.AddScoped<IAuthorizationHandler, CanEditEntryHandler>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
    }
}