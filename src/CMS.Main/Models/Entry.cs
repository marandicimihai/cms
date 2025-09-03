using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace CMS.Main.Models;

public class Entry : IDisposable
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = default!;

    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = default!;
    public Schema Schema { get; set; } = default!;

    [Required]
    public JsonDocument Data { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void Dispose()
    {
        Data?.Dispose();
    }
}