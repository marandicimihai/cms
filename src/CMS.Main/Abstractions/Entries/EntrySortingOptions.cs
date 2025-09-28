namespace CMS.Main.Abstractions.Entries;

public class EntrySortingOptions
{
    public string PropertyName { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;
}
