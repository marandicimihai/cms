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
using CMS.Main.Auth;
using NSwag;
using Microsoft.AspNetCore.DataProtection;
using CMS.Main.Abstractions.Entries;
using CMS.Main.Services.Entries;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.Services.SchemaProperties;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

MapsterConfig.ConfigureMapster();

var builder = WebApplication.CreateBuilder(args);
IConfiguration config = builder.Configuration;

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(opt =>
    {
        opt.EnableJWTBearerAuth = false;
        opt.DocumentSettings = s =>
        {
            s.DocumentName = "Initial Release";
            s.Title = "CMS API";
            s.Version = "v0";
            s.AddAuth(AuthConstants.ApiKeyScheme, new()
            {
                Name = "ApiKey",
                In = OpenApiSecurityApiKeyLocation.Header,
                Scheme = AuthConstants.ApiKeyScheme,
                Type = OpenApiSecuritySchemeType.ApiKey
            });
        };
    });

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
    .AddSingleton<ConfirmationService>()
    .AddSingleton<ISchemaPropertyTypeHandlerFactory, SchemaPropertyTypeHandlerFactory>()
    .AddScoped<ISchemaPropertyValidator, SchemaPropertyValidator>();

builder.Services
    .ConfigureFluentEmail(config, builder.Environment);

var keysFolder = "/var/DataProtection-Keys"; // Ensure this path is writable in your Docker container
builder.Services.AddDataProtection()
    .SetApplicationName("cms.app")
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

// Register the custom converter for server-side JSON binding (HTTP requests)
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opts =>
{
    opts.SerializerOptions.Converters.Add(new CMS.Main.Serialization.DictionaryStringJsonConverter());
});

// Also register for MVC/Controllers if used by any components
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

app.UseFastEndpoints(cfg =>
    {
        cfg.Versioning.Prefix = "v";
    })
    .UseSwaggerGen();

app.MapAdditionalIdentityEndpoints();

app.Run();
