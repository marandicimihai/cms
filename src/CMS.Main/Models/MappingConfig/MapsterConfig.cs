using CMS.Main.Abstractions.Entries;
using CMS.Main.DTOs;
using CMS.Main.Endpoints.Entries;
using Mapster;
using System.Linq;

namespace CMS.Main.Models.MappingConfig;

public class MapsterConfig
{
    public static void ConfigureMapster()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

        TypeAdapterConfig<PropertyDto, Property>
            .NewConfig()
            .Map(dest => dest.SchemaId, src => src.SchemaId.Trim())
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.Options, src => TrimArray(src.Options))
            .Ignore(s => s.Id)
            .Ignore(s => s.Schema)
            .Ignore(s => s.CreatedAt);

        TypeAdapterConfig<SchemaDto, Schema>
            .NewConfig()
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.ProjectId, src => src.ProjectId.Trim())
            .Ignore(s => s.Id)
            .Ignore(s => s.Project)
            .Ignore(s => s.Properties);

        TypeAdapterConfig<ProjectDto, Project>
            .NewConfig()
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.OwnerId, src => src.OwnerId.Trim())
            .Ignore(p => p.Id)
            .Ignore(p => p.Schemas)
            .Ignore(p => p.ApiKeys)
            .Ignore(p => p.LastUpdated);

        TypeAdapterConfig<EntryDto, Entry>
            .NewConfig()
            .Map(dest => dest.SchemaId, src => src.SchemaId.Trim())
            .Ignore(e => e.Id)
            .Ignore(e => e.CreatedAt)
            .Ignore(e => e.UpdatedAt)
            .Ignore(e => e.Schema);

        TypeAdapterConfig<ApiKeyDto, ApiKey>
            .NewConfig()
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.ProjectId, src => src.ProjectId.Trim())
            .Ignore(k => k.Id)
            .Ignore(k => k.HashedKey)
            .Ignore(k => k.Project)
            .Ignore(k => k.CreatedAt);
    }

    // Helper used from expression trees to transform string[] -> string[] with trimming
    // Must be public/static so expression trees can bind to it.
    public static string[]? TrimArray(string[]? input)
    {
        return input == null ? null : input.Select(s => (s ?? string.Empty).Trim()).ToArray();
    }

    public static readonly TypeAdapterConfig EditSchemaPropertyConfig = new TypeAdapterConfig()
        .NewConfig<PropertyDto, Property>()
        .Map(dest => dest.Name, src => src.Name.Trim())
        .Map(dest => dest.Options, src => TrimArray(src.Options))
        .Ignore(s => s.Id)
        .Ignore(s => s.Schema)
        .Ignore(s => s.CreatedAt)
        .Ignore(p => p.SchemaId)
        .Ignore(p => p.Type).Config;
    
    public static readonly TypeAdapterConfig EditProjectConfig = new TypeAdapterConfig()
        .NewConfig<ProjectDto, Project>()
        .Map(dest => dest.Name, src => src.Name.Trim())
        .Map(dest => dest.OwnerId, src => src.OwnerId.Trim())
        .Ignore(p => p.Id)
        .Ignore(p => p.Schemas)
        .Ignore(p => p.ApiKeys)
        .Ignore(p => p.LastUpdated)
        .Ignore(p => p.OwnerId).Config;
    
    public static readonly TypeAdapterConfig EditApiKeyConfig = new TypeAdapterConfig()
        .NewConfig<ApiKeyDto, ApiKey>()
        .Map(dest => dest.Name, src => src.Name.Trim())
        .Map(dest => dest.ProjectId, src => src.ProjectId.Trim())
        .Ignore(k => k.Id)
        .Ignore(k => k.HashedKey)
        .Ignore(k => k.Project)
        .Ignore(k => k.CreatedAt)
        .Ignore(k => k.ProjectId).Config;
}
