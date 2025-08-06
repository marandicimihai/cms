using CMS.Backend.Data;
using CMS.Backend.Emails;
using CMS.Backend.Emails.Models;
using CMS.Backend.Services;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
IConfiguration config = builder.Configuration;

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument();
builder.Services.AddOpenApi();

// ? Authentication and Authorization
builder.Services.AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();

builder.Services.AddDbContext<ApplicationDbContext>(opt => 
    opt.UseNpgsql(config.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

// ? Settings
builder.Services
    .AddOptions<EmailSettings>()
    .Bind(config.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ? Custom Services
builder.Services
    .AddSingleton<IEmailSender<ApplicationUser>, IdentityEmailQueuer>();

builder.Services
    .ConfigureFluentEmail(config, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGroup("auth")
    .MapIdentityApi<ApplicationUser>();

app.UseFastEndpoints()
    .UseSwaggerGen();

app.Run();