using System.ComponentModel.DataAnnotations;
using CMS.Shared.DTOs.Project;
using CMS.Shared.DTOs.SchemaProperty;

namespace CMS.Shared.DTOs.Schema;

public class SchemaDto
{
    public string Id { get; set; } = default!;

    [Required]
    [Length(3, 50)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(36)]
    public string ProjectId { get; set; } = default!;
    public ProjectWithIdDto Project { get; set; } = default!;
    
    public List<SchemaPropertyDto> Properties { get; set; } = [];
}