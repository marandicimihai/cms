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
    public string Name { get; set; } = default!;
    
    [Required]
    public SchemaPropertyType Type { get; set; }
    
    // For enums
    public string[]? Options { get; set; }
}