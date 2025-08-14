using System.ComponentModel.DataAnnotations;

namespace CMS.Shared.DTOs.Project;

public class ProjectCreationDto : ProjectBaseDto
{
    [Required]
    [StringLength(36)]
    public string OwnerId { get; set; } = default!;
}