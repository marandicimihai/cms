namespace CMS.Shared.DTOs.Entry;

public class EntryWithIdDto : EntryBaseDto
{
    public string Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}