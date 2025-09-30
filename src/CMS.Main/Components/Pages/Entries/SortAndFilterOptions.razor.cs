
using System.Globalization;
using CMS.Main.Abstractions.Entries;
using Microsoft.AspNetCore.Components;
using CMS.Main.DTOs.SchemaProperty;

namespace CMS.Main.Components.Pages.Entries;

public partial class SortAndFilterOptions : ComponentBase
{
    public record struct SortAndFilterOptionsChangedEventArgs(string SortByProperty, bool Descending, List<EntryFilter> Filters);

    [Parameter]
    public EventCallback<SortAndFilterOptionsChangedEventArgs> OnOptionsChanged { get; set; }

    protected override void OnInitialized()
    {
        sortByPropertyBacking = InitialSortByProperty;
        descendingBacking = InitialDescending;
    }

    protected override void OnParametersSet()
    {
        EffectiveFilterableProperties = FilterableProperties
            .Where(p => GetFilterOptions(p.Type).Any())
            .OrderBy(p => p.Name)
            .ToList();

        if (!filtersInitialized)
        {
            InitializeFilterRows();
            filtersInitialized = true;
        }
    }

    #region Sorting

    [Parameter]
    public List<string> SortableProperties { get; set; } = [];

    [Parameter]
    public string InitialSortByProperty { get; set; } = "CreatedAt";

    [Parameter]
    public bool InitialDescending { get; set; } = false;

    private string sortByPropertyBacking = string.Empty;
    private string SortByProperty
    {
        get => sortByPropertyBacking;
        set
        {
            if (value == sortByPropertyBacking) return;
            sortByPropertyBacking = value;
            if (OnOptionsChanged.HasDelegate)
            {
                OnOptionsChanged.InvokeAsync(new SortAndFilterOptionsChangedEventArgs(SortByProperty, Descending, BuildFiltersForCallback()));
            }
        }
    }

    private bool descendingBacking = false;
    private bool Descending
    {
        get => descendingBacking;
        set
        {
            if (value == descendingBacking) return;
            descendingBacking = value;
            if (OnOptionsChanged.HasDelegate)
            {
                OnOptionsChanged.InvokeAsync(new SortAndFilterOptionsChangedEventArgs(SortByProperty, Descending, BuildFiltersForCallback()));
            }
        }
    }

    #endregion

    #region Filtering

    [Parameter]
    public List<SchemaPropertyDto> FilterableProperties { get; set; } = [];

    [Parameter]
    public IEnumerable<EntryFilter>? InitialFilters { get; set; }

    private readonly List<FilterRowState> filterRows = [];
    private bool filtersInitialized;

    private List<SchemaPropertyDto> EffectiveFilterableProperties { get; set; } = [];

    private bool CanAddFilters => EffectiveFilterableProperties.Count > 0;

    private void InitializeFilterRows()
    {
        filterRows.Clear();

        if (InitialFilters is null) return;

        foreach (var filter in InitialFilters)
        {
            if (string.IsNullOrWhiteSpace(filter.PropertyName))
                continue;

            var property = GetProperty(filter.PropertyName);
            if (property is null) continue;

            var allowed = GetFilterOptions(property.Type);
            if (!allowed.Any()) continue;

            var type = filter.FilterType.HasValue && allowed.Contains(filter.FilterType.Value)
                ? filter.FilterType.Value
                : allowed[0];

            var value = ConvertToDisplayValue(filter.ReferenceValue, property.Type);

            filterRows.Add(new FilterRowState
            {
                PropertyName = property.Name,
                FilterType = type,
                ReferenceValue = value
            });
        }
    }

    private void AddFilter()
    {
        var firstProp = EffectiveFilterableProperties.FirstOrDefault();
        if (firstProp is null) return;
        var allowed = GetFilterOptions(firstProp.Type);
        if (!allowed.Any()) return;

        filterRows.Add(new FilterRowState
        {
            PropertyName = firstProp.Name,
            FilterType = allowed[0],
            ReferenceValue = string.Empty
        });
        NotifyFiltersChanged();
    }

    private void RemoveFilter(FilterRowState row)
    {
        if (filterRows.Remove(row))
        {
            NotifyFiltersChanged();
        }
    }

    private void OnPropertyChanged(FilterRowState row, ChangeEventArgs args)
    {
        var selected = args.Value?.ToString();
        if (string.IsNullOrWhiteSpace(selected)) return;
        if (row.PropertyName == selected) return;

        row.PropertyName = selected;
        row.ReferenceValue = string.Empty;
        row.ErrorMessage = null;

        var property = GetProperty(selected);
        if (property is null)
        {
            row.FilterType = null;
        }
        else
        {
            var allowed = GetFilterOptions(property.Type);
            row.FilterType = allowed.FirstOrDefault();
        }

        NotifyFiltersChanged();
    }

    private void OnFilterTypeChanged(FilterRowState row, ChangeEventArgs args)
    {
        var selected = args.Value?.ToString();
        if (string.IsNullOrWhiteSpace(selected)) return;
        if (Enum.TryParse<PropertyFilter>(selected, out var parsed))
        {
            if (row.FilterType != parsed)
            {
                row.FilterType = parsed;
                NotifyFiltersChanged();
            }
        }
    }

    private void OnValueChanged(FilterRowState row, ChangeEventArgs args)
    {
        row.ReferenceValue = args.Value?.ToString() ?? string.Empty;
        row.ErrorMessage = null;
        NotifyFiltersChanged();
    }

    private SchemaPropertyDto? GetProperty(string? name)
        => name is null ? null : EffectiveFilterableProperties.FirstOrDefault(p => p.Name == name);

    private static List<PropertyFilter> GetFilterOptions(SchemaPropertyType type) => type switch
    {
        SchemaPropertyType.Text => new() { PropertyFilter.Equals, PropertyFilter.NotEquals, PropertyFilter.Contains, PropertyFilter.StartsWith, PropertyFilter.EndsWith },
        SchemaPropertyType.Number => new() { PropertyFilter.GreaterThan, PropertyFilter.LessThan },
        SchemaPropertyType.DateTime => new() { PropertyFilter.GreaterThan, PropertyFilter.LessThan },
        _ => new()
    };

    private static string FormatFilterLabel(PropertyFilter filter) => filter switch
    {
        PropertyFilter.Equals => "Equals",
        PropertyFilter.NotEquals => "Not equals",
        PropertyFilter.GreaterThan => "Greater than",
        PropertyFilter.LessThan => "Less than",
        PropertyFilter.Contains => "Contains",
        PropertyFilter.StartsWith => "Starts with",
        PropertyFilter.EndsWith => "Ends with",
        _ => filter.ToString()
    };

    private static string GetInputType(SchemaPropertyType? type) => type switch
    {
        SchemaPropertyType.Number => "number",
        SchemaPropertyType.DateTime => "datetime-local",
        _ => "text"
    };

    private static string? GetValuePlaceholder(SchemaPropertyType? type) => type switch
    {
        SchemaPropertyType.Number => "e.g. 42",
        SchemaPropertyType.DateTime => "Select date/time",
        _ => "Enter value"
    };

    private static string ConvertToDisplayValue(object? value, SchemaPropertyType type)
    {
        if (value is null) return string.Empty;
        switch (type)
        {
            case SchemaPropertyType.Number:
                if (value is IFormattable formattable)
                    return formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;
                return value.ToString() ?? string.Empty;
            case SchemaPropertyType.DateTime:
                if (value is DateTime dt)
                {
                    return (dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt)
                        .ToLocalTime().ToString("yyyy-MM-ddTHH:mm");
                }
                if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
                {
                    return parsed.ToLocalTime().ToString("yyyy-MM-ddTHH:mm");
                }
                return string.Empty;
            default:
                return value.ToString() ?? string.Empty;
        }
    }

    private bool TryFormatReferenceValue(FilterRowState row, SchemaPropertyDto property, out object? formatted)
    {
        formatted = null;
        var raw = row.ReferenceValue?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            row.ErrorMessage = "Value is required";
            return false;
        }

        switch (property.Type)
        {
            case SchemaPropertyType.Text:
                formatted = raw;
                return true;
            case SchemaPropertyType.Number:
                if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                {
                    formatted = dec.ToString(CultureInfo.InvariantCulture);
                    return true;
                }
                row.ErrorMessage = "Enter a valid number";
                return false;
            case SchemaPropertyType.DateTime:
                if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
                {
                    var specified = dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Local) : dt;
                    formatted = specified.ToUniversalTime().ToString("o");
                    return true;
                }
                row.ErrorMessage = "Enter a valid date/time";
                return false;
        }

        row.ErrorMessage = "Unsupported property type";
        return false;
    }

    private void NotifyFiltersChanged()
    {
        foreach (var r in filterRows) r.ErrorMessage = null;

        var filters = BuildFiltersForCallback();
        if (OnOptionsChanged.HasDelegate)
        {
            OnOptionsChanged.InvokeAsync(new SortAndFilterOptionsChangedEventArgs(SortByProperty, Descending, filters));
        }
    }

    private List<EntryFilter> BuildFiltersForCallback()
    {
        var list = new List<EntryFilter>();
        foreach (var row in filterRows)
        {
            if (string.IsNullOrWhiteSpace(row.PropertyName) || row.FilterType is null)
                continue;

            var prop = GetProperty(row.PropertyName);
            if (prop is null) continue;

            var allowed = GetFilterOptions(prop.Type);
            if (!allowed.Contains(row.FilterType.Value)) continue;

            if (!TryFormatReferenceValue(row, prop, out var formatted)) continue;

            list.Add(new EntryFilter
            {
                PropertyName = row.PropertyName,
                FilterType = row.FilterType,
                ReferenceValue = formatted
            });
        }
        return list;
    }

    private sealed class FilterRowState
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? PropertyName { get; set; }
        public PropertyFilter? FilterType { get; set; }
        public string ReferenceValue { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    #endregion
}