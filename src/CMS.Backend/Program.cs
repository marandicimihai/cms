using CMS.Backend.Abstractions;
using CMS.Backend.Data;
using CMS.Backend.Data.Jobs;
using CMS.Backend.Services;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFastEndpoints()
    .AddJobQueues<JobRecord, JobStorageProvider>()
    .SwaggerDocument();
builder.Services.AddOpenApi();

// ? Authentication and Authorization
builder.Services.AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();

builder.Services.AddDbContext<ApplicationDbContext>(opt => 
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

// ? Custom Services
builder.Services
    .AddSingleton<IEmailSender<ApplicationUser>, IdentityEmailQueuer>()
    .AddScoped<ISendGridEmailSender, SendGridEmailSender>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGroup("auth")
    .MapIdentityApi<ApplicationUser>();

app.UseFastEndpoints()
    .UseJobQueues()
    .UseSwaggerGen();

app.Run();