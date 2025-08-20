using CMS.Shared.DTOs.Schema;

namespace CMS.Shared.DTOs.Project;

public class ProjectWithIdDto : ProjectBaseDto
{
    public string Id { get; set; } = default!;

    public DateTime LastUpdated { get; set; }

    public List<SchemaDto> Schemas { get; set; } = [];
}