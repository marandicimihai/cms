namespace CMS.Main.Abstractions.Entries;

public class EntryGetOptions
{
    public string SortByPropertyName { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;

    public List<EntryFilter> Filters { get; set; } = [];
}

public class EntryFilter
{
    public string PropertyName { get; set; } = string.Empty;
    public PropertyFilter? FilterType { get; set; }
    public object? ReferenceValue { get; set; }
}

public enum PropertyFilter
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    Contains,
    StartsWith,
    EndsWith
}