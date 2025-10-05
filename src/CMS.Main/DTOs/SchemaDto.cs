using System.ComponentModel.DataAnnotations;

namespace CMS.Main.DTOs;

public class SchemaDto
{
    public string Id { get; set; } = default!;

    [Required]
    [Length(3, 50)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(36)]
    public string ProjectId { get; set; } = default!;
    public ProjectDto Project { get; set; } = default!;

    private List<PropertyDto> properties = [];
    public List<PropertyDto> Properties
    {
        get => properties;
        set
        {
            properties = value.OrderBy(p => p.CreatedAt).ToList();
        }
    }
}