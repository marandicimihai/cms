using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CMS.Main.DTOs.SchemaProperty;

namespace CMS.Main.Models;

public class SchemaProperty : IValidatableObject
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = default!;
    
    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = string.Empty;
    public Schema Schema { get; set; } = default!;
    
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Options == null) yield break;
        
        foreach (var option in Options)
        {
            if (option.Contains(' '))
            {
                yield return new ValidationResult("Options cannot contain spaces.", [nameof(Options)]);
            }
        }
    }
}