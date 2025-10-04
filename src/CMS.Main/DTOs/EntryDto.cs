using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CMS.Main.DTOs;

public class EntryDto
{
    public string Id { get; set; } = default!;
    
    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = default!;
    
    [JsonIgnore]
    public SchemaDto Schema { get; set; } = default!;
    
    [JsonExtensionData]
    public Dictionary<string, object?> Fields { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}