using System.ComponentModel.DataAnnotations;

namespace CMS.Shared.DTOs.Schema;

public class SchemaBaseDto
{
    [Required]
    [Length(3, 50)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(36)]
    public string ProjectId { get; set; } = default!;
}