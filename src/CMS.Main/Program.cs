using System.Globalization;
using CMS.Main.Abstractions;
using CMS.Main.Components;
using CMS.Main.Components.Account;
using CMS.Main.Data;
using CMS.Main.Emails;
using CMS.Main.Emails.Config;
using CMS.Main.Models.MappingConfig;
using CMS.Main.Services;
using CMS.Main.Services.State;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using System.Runtime.InteropServices;
using CMS.Main.Auth;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

MapsterConfig.ConfigureMapster();

var builder = WebApplication.CreateBuilder(args);
IConfiguration config = builder.Configuration;

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = config.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.ConfigureDataServices(connectionString);
builder.Services.ConfigureAuth();

// ? Settings
builder.Services
    .AddOptions<EmailSettings>()
    .Bind(config.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ? Custom Services
builder.Services
    .AddScoped<IDbContextConcurrencyHelper, DbContextConcurrencyHelper>()
    .AddScoped<ProjectStateService>()
    .AddScoped<EntryStateService>()
    .AddScoped<ApiKeyStateService>()
    .AddScoped<AuthorizationHelperService>()
    .AddScoped<IProjectService, ProjectService>()
    .AddScoped<ISchemaService, SchemaService>()
    .AddScoped<ISchemaPropertyService, SchemaPropertyService>()
    .AddScoped<IEntryService, EntryService>()
    .AddScoped<IApiKeyService, ApiKeyService>()
    .AddSingleton<IEmailSender<ApplicationUser>, IdentityEmailSender>()
    .AddSingleton<ConfirmationService>();

builder.Services
    .ConfigureFluentEmail(config, builder.Environment);

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    var keysFolder = "/var/data-protection-keys"; // Ensure this path is writable in your Docker container
    builder.Services.AddDataProtection()
        .SetApplicationName("cms.app")
        .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseAuthentication()
    .UseAuthorization()
    .UseFastEndpoints()
    .UseSwaggerGen();

app.MapAdditionalIdentityEndpoints();

app.Run();
