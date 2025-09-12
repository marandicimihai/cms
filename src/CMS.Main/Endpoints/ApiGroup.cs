using CMS.Main.Auth;
using FastEndpoints;

namespace CMS.Main.Endpoints;

public sealed class ApiGroup : Group
{
    public ApiGroup()
    {
        Configure("api", ep =>
        {
            ep.AuthSchemes(AuthConstants.ApiKeyScheme);
        });
    }
}