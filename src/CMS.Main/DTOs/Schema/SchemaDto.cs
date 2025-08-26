using System.ComponentModel.DataAnnotations;
using CMS.Main.DTOs.Project;
using CMS.Main.DTOs.SchemaProperty;

namespace CMS.Main.DTOs.Schema;

public class SchemaDto
{
    public string Id { get; set; } = default!;

    [Required]
    [Length(3, 50)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(36)]
    public string ProjectId { get; set; } = default!;
    public ProjectDto Project { get; set; } = default!;
    
    public List<SchemaPropertyDto> Properties { get; set; } = [];
}