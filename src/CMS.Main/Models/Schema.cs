using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Main.Models;

public class Schema
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = default!;
    
    [Required]
    [Length(3, 50)]
    public string Name { get; set; } = default!;
    
    [Required]
    [StringLength(36)]
    public string ProjectId { get; set; } = default!;
    public Project Project { get; set; } = default!;
}