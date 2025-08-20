using System.ComponentModel.DataAnnotations;
using CMS.Shared.DTOs.Schema;
using CMS.Shared.DTOs.SchemaProperty;

namespace CMS.Shared.DTOs.Entry;

public class EntryDto
{
    public string Id { get; set; } = default!;
    
    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = default!;
    public SchemaDto Schema { get; set; } = default!;
    
    public Dictionary<SchemaPropertyDto, object?> Properties { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}