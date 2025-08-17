using System.ComponentModel.DataAnnotations;

namespace CMS.Shared.DTOs.Project;

public class ProjectBaseDto
{
    [Required]
    [StringLength(36)]
    public string OwnerId { get; set; } = default!;

    [Required(ErrorMessage = "Project name is required.")]
    [Length(3, 50, ErrorMessage = "Project name must be between 3 and 50 characters long.")]
    public string Name { get; set; } = default!;
}