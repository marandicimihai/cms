using CMS.Shared.DTOs.Entry;
using CMS.Shared.DTOs.Project;
using CMS.Shared.DTOs.Schema;
using CMS.Shared.DTOs.SchemaProperty;
using Mapster;

namespace CMS.Main.Models.Config;

public class MapsterConfig
{
    public static void ConfigureMapster()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

        TypeAdapterConfig<SchemaPropertyDto, SchemaProperty>
            .NewConfig()
            .Ignore(s => s.Id)
            .Ignore(s => s.Schema)
            .Ignore(s => s.CreatedAt);

        TypeAdapterConfig<SchemaDto, Schema>
            .NewConfig()
            .Ignore(s => s.Id)
            .Ignore(s => s.Project)
            .Ignore(s => s.Properties);

        TypeAdapterConfig<ProjectDto, Project>
            .NewConfig()
            .Ignore(p => p.Id)
            .Ignore(p => p.Schemas)
            .Ignore(p => p.LastUpdated);

        TypeAdapterConfig<EntryDto, Entry>
            .NewConfig()
            .Ignore(e => e.Id)
            .Ignore(e => e.CreatedAt)
            .Ignore(e => e.UpdatedAt)
            .Ignore(e => e.Schema);
    }
}