using System.ComponentModel.DataAnnotations;

namespace CMS.Shared.DTOs.SchemaProperty;

public class SchemaPropertyBaseDto
{
    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = default!;
    
    [Required]
    [Length(3, 100)]
    public string Name { get; set; } = default!;
    
    [Required]
    public SchemaPropertyType Type { get; set; }
    
    // For enums
    public string[]? Options { get; set; }
}