using System.ComponentModel.DataAnnotations;

namespace CMS.Main.DTOs;

public class ProjectDto
{
    public string Id { get; set; } = default!;
    
    [Required]
    [StringLength(36)]
    public string OwnerId { get; set; } = default!;

    [Required(ErrorMessage = "Project name is required.")]
    [Length(3, 50, ErrorMessage = "Project name must be between 3 and 50 characters long.")]
    public string Name { get; set; } = default!;

    public DateTime LastUpdated { get; set; }

    public List<SchemaDto> Schemas { get; set; } = [];
    public List<ApiKeyDto> ApiKeys { get; set; } = [];
}