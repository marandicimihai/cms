using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CMS.Shared.DTOs.SchemaProperty;

namespace CMS.Main.Models;

public class SchemaProperty
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = default!;
    
    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = string.Empty;
    public Schema Schema { get; set; } = default!;
    
    [Required]
    [Length(3, 100)]
    [RegularExpression("^[A-Za-z][A-Za-z0-9_]*[A-Za-z]$", ErrorMessage = "Name must start and end with a letter, and only contain letters, numbers, and underscores in between.")]
    public string Name { get; set; } = default!;
    
    [Required]
    public SchemaPropertyType Type { get; set; }
    
    // For enums
    public string[]? Options { get; set; }
}