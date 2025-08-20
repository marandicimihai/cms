using CMS.Shared.DTOs.Project;
using CMS.Shared.DTOs.SchemaProperty;

namespace CMS.Shared.DTOs.Schema;

public class SchemaWithIdDto : SchemaBaseDto
{
    public string Id { get; set; } = default!;

    public ProjectWithIdDto Project { get; set; } = default!;
    public List<SchemaPropertyDto> Properties { get; set; } = [];
}