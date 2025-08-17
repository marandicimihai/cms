using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Shared.DTOs.SchemaProperty;

public class SchemaPropertyWithIdDto : SchemaPropertyBaseDto
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = default!;
}