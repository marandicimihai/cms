using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using CMS.Main.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CMS.Main.Auth.ApiKeyScheme;

public class ApiKeySchemeHandler(
    IOptionsMonitor<ApiKeySchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IDbContextConcurrencyHelper dbHelper,
    UserManager<ApplicationUser> userManager)
    : AuthenticationHandler<ApiKeySchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? rawKey = Request.Headers["ApiKey"];

        if (rawKey is null)
            return AuthenticateResult.NoResult();

        var hash = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawKey)));
        var hashedKey = await dbHelper.ExecuteAsync(async dbContext =>
        {
            return await dbContext.ApiKeys
                .AsNoTracking()
                .Include(k => k.Project)
                .FirstOrDefaultAsync(k => k.HashedKey == hash);
        });
        
        if (hashedKey is null)
            return AuthenticateResult.Fail("Invalid api key.");
        
        var user = await dbHelper.ExecuteAsync(async _ => await userManager.FindByIdAsync(hashedKey.Project.OwnerId));
        
        if (user is null)
            return AuthenticateResult.Fail("Invalid api key.");
        
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, user.Id, ClaimValueTypes.String, Options.ClaimsIssuer)],
                Scheme.Name));
        var ticket = new AuthenticationTicket(principal, AuthConstants.ApiKeyScheme);

        return AuthenticateResult.Success(ticket);
    }
}