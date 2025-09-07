using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CMS.Main.DTOs.Schema;
using CMS.Main.DTOs.SchemaProperty;

namespace CMS.Main.DTOs.Entry;

public class EntryDto
{
    public string Id { get; set; } = default!;
    
    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = default!;
    
    [JsonIgnore]
    public SchemaDto Schema { get; set; } = default!;
    
    [JsonConverter(typeof(SchemaPropertyDtoDictionaryConverter))]
    public Dictionary<SchemaPropertyDto, object?> Properties { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}