using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Main.Models;

public class ApiKey
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = default!;
    
    [Required]
    public string HashedKey { get; set; } = default!;
    
    [Required]
    [Length(3, 50)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(36)]
    public string ProjectId { get; set; } = default!;
    public Project Project { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; }
}