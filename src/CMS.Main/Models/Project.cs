using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Main.Models;

public class Project
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = default!;

    [Required]
    [StringLength(36)]
    public string OwnerId { get; set; } = default!;

    [Required]
    [Length(3, 50)]
    public string Name { get; set; } = default!;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}