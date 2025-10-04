using System.ComponentModel.DataAnnotations;
using CMS.Main.DTOs.SchemaProperty;

namespace CMS.Main.DTOs;

public class SchemaPropertyDto
{
    public string Id { get; set; } = default!;
    
    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = default!;
    
    [Required]
    [Length(3, 100)]
    [RegularExpression("^[A-Za-z][A-Za-z0-9_]*[A-Za-z0-9]$", ErrorMessage = "Name must start with a letter and end with a letter or number, and only contain letters, numbers, and underscores in between.")]
    public string Name { get; set; } = default!;
    
    [Required]
    public SchemaPropertyType Type { get; set; }
    
    // For enums
    public string[]? Options { get; set; }
    
    public bool IsRequired { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}