using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CMS.Shared.DTOs.Schema;
using CMS.Shared.DTOs.SchemaProperty;

namespace CMS.Shared.DTOs.Entry;

public class EntryBaseDto
{
    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = default!;
    public SchemaWithIdDto Schema { get; set; } = default!;
    
    public Dictionary<SchemaPropertyDto, object?> Properties { get; set; } = new();
}