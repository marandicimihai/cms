using CMS.Main.DTOs.ApiKey;
using CMS.Main.DTOs.Entry;
using CMS.Main.DTOs.Project;
using CMS.Main.DTOs.Schema;
using CMS.Main.DTOs.SchemaProperty;
using Mapster;

namespace CMS.Main.Models.MappingConfig;

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
            .Ignore(p => p.ApiKeys)
            .Ignore(p => p.LastUpdated);

        TypeAdapterConfig<EntryDto, Entry>
            .NewConfig()
            .Ignore(e => e.Id)
            .Ignore(e => e.CreatedAt)
            .Ignore(e => e.UpdatedAt)
            .Ignore(e => e.Schema);

        TypeAdapterConfig<ApiKeyDto, ApiKey>
            .NewConfig()
            .Ignore(k => k.Id)
            .Ignore(k => k.HashedKey)
            .Ignore(k => k.Project)
            .Ignore(k => k.CreatedAt);
    }

    public static readonly TypeAdapterConfig EditSchemaPropertyConfig = new TypeAdapterConfig()
        .NewConfig<SchemaPropertyDto, SchemaProperty>()
        .Ignore(s => s.Id)
        .Ignore(s => s.Schema)
        .Ignore(s => s.CreatedAt)
        .Ignore(p => p.SchemaId)
        .Ignore(p => p.Type).Config;
    
    public static readonly TypeAdapterConfig EditProjectConfig = new TypeAdapterConfig()
        .NewConfig<ProjectDto, Project>()
        .Ignore(p => p.Id)
        .Ignore(p => p.Schemas)
        .Ignore(p => p.ApiKeys)
        .Ignore(p => p.LastUpdated)
        .Ignore(p => p.OwnerId).Config;
    
    public static readonly TypeAdapterConfig EditApiKeyConfig = new TypeAdapterConfig()
        .NewConfig<ApiKeyDto, ApiKey>()
        .Ignore(k => k.Id)
        .Ignore(k => k.HashedKey)
        .Ignore(k => k.Project)
        .Ignore(k => k.CreatedAt)
        .Ignore(k => k.ProjectId).Config;
}
