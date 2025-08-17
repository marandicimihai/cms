using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Auth;

public class MustOwnProjectRequirement : IAuthorizationRequirement
{
}