using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CMS.Shared.DTOs.Schema;

namespace CMS.Shared.DTOs.Entry;

public class EntryBaseDto : IDisposable
{
    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = default!;
    public SchemaWithIdDto Schema { get; set; } = default!;

    [Required]
    public JsonDocument Data { get; set; } = default!;

    public void Dispose()
    {
        Data?.Dispose();
    }
}