namespace CMS.Main.Abstractions;

public class EntryGetOptions
{
    public bool IncludeSchema { get; set; } = false;
    public SchemaGetOptions SchemaGetOptions { get; set; } = new();
    
    public EntrySortingOption SortingOption { get; set; } = EntrySortingOption.None;
    public bool Descending { get; set; }
}

public enum EntrySortingOption
{
    None,
    CreatedAt,
    UpdatedAt
}