
using CMS.Main.Abstractions.Entries;
using Microsoft.AspNetCore.Components;
using CMS.Main.Components.Shared;
using Mapster;
using CMS.Main.DTOs;
using CMS.Main.Abstractions.Properties.PropertyTypes;

namespace CMS.Main.Components.Pages.Entries;

public partial class SortAndFilterOptions : ComponentBase
{
    // Event args for sort/filter changes
    public record SortAndFilterOptionsChangedEventArgs(string SortByProperty, bool Descending, List<EntryFilter> Filters);

    [Parameter]
    public EventCallback<SortAndFilterOptionsChangedEventArgs> OnOptionsChanged { get; set; }

    // Sorting
    [Parameter]
    public List<string> SortableProperties { get; set; } = [];
    [Parameter]
    public string InitialSortByProperty { get; set; } = "CreatedAt";
    [Parameter]
    public bool InitialDescending { get; set; } = false;

    private string SortByProperty { get; set; } = "CreatedAt";
    private bool Descending { get; set; }

    // Filtering
    [Parameter]
    public List<PropertyDto> FilterableProperties { get; set; } = [];
    private List<PropertyDto> FilterablePropertiesCopy { get; set; } = [];
    private List<FilterRow> FilterRows { get; set; } = [];
    private List<EntryFilter> Filters => FilterRows.Select(r => r.Filter).ToList();

    protected override void OnParametersSet()
    {
        // Defensive copy for UI state
        FilterablePropertiesCopy = FilterableProperties.Adapt<List<PropertyDto>>();
        FilterablePropertiesCopy.ForEach(p => p.IsRequired = false);
        SortByProperty = InitialSortByProperty;
        Descending = InitialDescending;
    }

    private void AddFilter()
    {
        var firstProp = FilterableProperties.FirstOrDefault()?.Name ?? string.Empty;
        FilterRows.Add(new FilterRow
        {
            Filter = new EntryFilter { PropertyName = firstProp, FilterType = PropertyFilter.Equals }
        });
    }

    private void RemoveFilter(FilterRow row)
    {
        FilterRows.Remove(row);
    }

    private void OnFilterPropertyChanged(FilterRow row, ChangeEventArgs e)
    {
        if (e.Value is string propertyName)
        {
            row.Filter.PropertyName = propertyName;
            row.Filter.FilterType = PropertyFilter.Equals;
            
            var property = FilterablePropertiesCopy.FirstOrDefault(p => p.Name == propertyName);
            if (property != null)
            {
                property.IsRequired = !(row.Filter.FilterType is PropertyFilter.Equals or PropertyFilter.NotEquals);
                StateHasChanged();
            }
        }
    }

    private void OnFilterTypeChanged(FilterRow row, ChangeEventArgs e)
    {
        if (e.Value is not string val)
            return;
        row.Filter.FilterType = Enum.Parse<PropertyFilter>(val);
        var property = FilterablePropertiesCopy.FirstOrDefault(p => p.Name == row.Filter.PropertyName);
        if (property != null)
        {
            property.IsRequired = !(row.Filter.FilterType is PropertyFilter.Equals or PropertyFilter.NotEquals);
            StateHasChanged();
        }
    }

    private static List<PropertyFilter> GetFilterOptionsForProperty(PropertyDto property)
    {
        return property.Type switch
        {
            PropertyType.Text => [PropertyFilter.Equals, PropertyFilter.NotEquals, PropertyFilter.StartsWith, PropertyFilter.EndsWith, PropertyFilter.Contains],
            PropertyType.Number => [PropertyFilter.Equals, PropertyFilter.NotEquals, PropertyFilter.GreaterThan, PropertyFilter.LessThan],
            PropertyType.Boolean => [PropertyFilter.Equals, PropertyFilter.NotEquals],
            PropertyType.DateTime => [PropertyFilter.Equals, PropertyFilter.NotEquals, PropertyFilter.GreaterThan, PropertyFilter.LessThan],
            PropertyType.Enum => [PropertyFilter.Equals, PropertyFilter.NotEquals],
            _ => []
        };
    }

    private record FilterRow
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public EntryFilter Filter { get; set; } = new();
        public DynamicEntryField? Field { get; set; }
    }

    private void ApplySortAndFilterOptions()
    {
        var args = new SortAndFilterOptionsChangedEventArgs(SortByProperty, Descending, Filters);
        var allValid = FilterRows.All(row => row.Field is not null && row.Field.IsValid());
        if (allValid)
        {
            foreach (var row in FilterRows)
            {
                var (_, val) = row.Field!.GetPropertyAndValue();
                row.Filter.ReferenceValue = val;
            }
            if (OnOptionsChanged.HasDelegate)
                OnOptionsChanged.InvokeAsync(args);
        }
    }
}