namespace CMS.Shared.Abstractions;

public class EntryGetOptions
{
    public EntrySortingOption SortingOption { get; set; } = EntrySortingOption.None;
    public bool Descending { get; set; }
}

public enum EntrySortingOption
{
    None,
    CreatedAt,
    UpdatedAt
}