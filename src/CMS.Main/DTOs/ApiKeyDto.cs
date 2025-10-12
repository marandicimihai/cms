using System.ComponentModel.DataAnnotations;

namespace CMS.Main.DTOs;

public class ApiKeyDto
{
    public string Id { get; set; } = default!;
    
    [Required]
    [Length(3, 50, ErrorMessage = "Api key name must be between 3 and 50 characters long.")]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(36)]
    public string ProjectId { get; set; } = default!;
    public ProjectDto Project { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; }
    
    public bool IsActive { get; set; }
}