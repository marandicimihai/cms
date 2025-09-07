using CMS.Main.Auth;
using FastEndpoints;

namespace CMS.Main.Endpoints.Entries;

public sealed class EntriesGroup : Group
{
    public EntriesGroup()
    {
        Configure("api/{schemaId}/entries", _ => { });
    }
}