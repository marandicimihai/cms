using Microsoft.AspNetCore.Identity;
using CMS.Main.Components;
using CMS.Main.Components.Account;
using CMS.Main.Data;
using CMS.Main.Emails;
using CMS.Main.Emails.Config;
using CMS.Main.Services;
using CMS.Shared.Abstractions;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);
IConfiguration config = builder.Configuration;

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

var connectionString = config.GetConnectionString("DefaultConnection") ?? 
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.ConfigureDataServices(connectionString);

// ? Settings
builder.Services
    .AddOptions<EmailSettings>()
    .Bind(config.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ? Custom Services
builder.Services
    .AddScoped<DbContextConcurrencyHelper>()
    .AddScoped<IProjectService, ProjectService>()
    .AddSingleton<IEmailSender<ApplicationUser>, IdentityEmailSender>();

builder.Services
    .ConfigureFluentEmail(config, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CMS.Main.Client._Imports).Assembly);

app.UseFastEndpoints()
    .UseSwaggerGen();

app.MapAdditionalIdentityEndpoints();

app.Run();