using FastEndpoints;

namespace CMS.Main.Endpoints.Entries;

public sealed class EntriesGroup : SubGroup<ApiGroup>
{
    public EntriesGroup()
    {
        Configure("", _ => { });
    }
}