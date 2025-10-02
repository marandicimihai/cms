using CMS.Main.Abstractions.Entries;
using Microsoft.AspNetCore.Components;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Components.Shared;

namespace CMS.Main.Components.Pages.Entries;

public partial class SortAndFilterOptions : ComponentBase
{
    public record SortAndFilterOptionsChangedEventArgs(string SortByProperty, bool Descending, List<EntryFilter> Filters);

    [Parameter]
    public EventCallback<SortAndFilterOptionsChangedEventArgs> OnOptionsChanged { get; set; }

    protected override void OnInitialized()
    {
        SortByProperty = InitialSortByProperty;
        Descending = InitialDescending;
    }

    #region Sorting

    [Parameter]
    public List<string> SortableProperties { get; set; } = [];

    [Parameter]
    public string InitialSortByProperty { get; set; } = "CreatedAt";

    [Parameter]
    public bool InitialDescending { get; set; } = false;

    private string SortByProperty { get; set; } = "CreatedAt";
    private bool Descending { get; set; }

    #endregion

    #region Filtering

    [Parameter]
    public List<SchemaPropertyDto> FilterableProperties { get; set; } = [];

    private List<FilterRow> FilterRows { get; set; } = [];

    private List<EntryFilter> Filters => FilterRows.Select(r => r.Filter).ToList();

    private void AddFilter()
    {
        var row = new FilterRow();
        row.Filter.PropertyName = FilterableProperties.FirstOrDefault()?.Name ?? string.Empty;
        row.Filter.FilterType = PropertyFilter.Equals;
        FilterRows.Add(row);
    }

    private void RemoveFilter(FilterRow row)
    {
        FilterRows.Remove(row);
    }

    private void OnFilterPropertyChanged(FilterRow row, ChangeEventArgs e)
    {
        if (e.Value is not string propertyName)
            return;

        row.Filter.PropertyName = propertyName;
    }

    private void OnFilterTypeChanged(FilterRow row, ChangeEventArgs e)
    {
        var val = e.Value!.ToString();
        if (val is null)
            return;

        row.Filter.FilterType = Enum.Parse<PropertyFilter>(val);
        
    }

    private List<PropertyFilter> GetFilterOptionsForProperty(SchemaPropertyDto property)
    {
        switch (property.Type)
        {
            case SchemaPropertyType.Text:
                return [PropertyFilter.Equals, PropertyFilter.NotEquals, PropertyFilter.StartsWith, PropertyFilter.EndsWith, PropertyFilter.Contains];
            case SchemaPropertyType.Number:
                return [PropertyFilter.Equals, PropertyFilter.NotEquals, PropertyFilter.GreaterThan, PropertyFilter.LessThan];
            case SchemaPropertyType.Boolean:
                return [PropertyFilter.Equals, PropertyFilter.NotEquals];
            case SchemaPropertyType.DateTime:
                return [PropertyFilter.Equals, PropertyFilter.NotEquals, PropertyFilter.GreaterThan, PropertyFilter.LessThan];
            case SchemaPropertyType.Enum:
                return [PropertyFilter.Equals, PropertyFilter.NotEquals];
            default:
                return [];
        }
    }

    private record FilterRow
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public EntryFilter Filter { get; set; } = new EntryFilter();
        public DynamicEntryField? Field { get; set; }
    }

    #endregion

    private void ApplySortAndFilterOptions()
    {
        var args = new SortAndFilterOptionsChangedEventArgs(SortByProperty, Descending, Filters);

        var allValid = true;
        foreach (var row in FilterRows)
        {
            if (row.Field is null || !row.Field.IsValid())
            {
                allValid = false;
                break;
            }

            var (_, val) = row.Field.GetPropertyAndValue();

            row.Filter.ReferenceValue = val;
        }

        if (allValid && OnOptionsChanged.HasDelegate)
        {
            OnOptionsChanged.InvokeAsync(args);
        }
    }
}