using Ardalis.Result;
using CMS.Main.Abstractions.Entries;
using CMS.Main.Abstractions.Properties.PropertyTypes;
using CMS.Main.Models;
using CMS.Main.Services.SchemaProperties;

namespace CMS.Main.Services.Entries;

public static class EntryQueryExtensions
{
    public static IQueryable<Entry> ApplySorting(this IQueryable<Entry> query, EntryGetOptions options, Schema schema)
    {
        // Sort the entries
        switch (options.SortByPropertyName)
        {
            case "CreatedAt":
                query = options.Descending
                    ? query.OrderByDescending(e => e.CreatedAt)
                    : query.OrderBy(e => e.CreatedAt);
                break;
            case "UpdatedAt":
                query = options.Descending
                    ? query.OrderByDescending(e => e.UpdatedAt)
                    : query.OrderBy(e => e.UpdatedAt);
                break;
            default:
                // Sort by custom property
                var property = schema.Properties.FirstOrDefault(p => p.Name == options.SortByPropertyName);
                if (property is not null &&
                        (property.Type == PropertyType.Text ||
                         property.Type == PropertyType.Number ||
                         property.Type == PropertyType.DateTime))
                {
                    if (property.Type == PropertyType.Number)
                    {
                        query = options.Descending
                            ? query.OrderByDescending(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDecimal())
                            : query.OrderBy(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDecimal());
                        break;
                    }
                    else if (property.Type == PropertyType.DateTime)
                    {
                        query = options.Descending
                            ? query.OrderByDescending(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDateTime())
                            : query.OrderBy(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDateTime());
                    }
                    else if (property.Type == PropertyType.Text)
                    {
                        query = options.Descending
                            ? query.OrderByDescending(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetString())
                            : query.OrderBy(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetString());
                    }
                }
                else
                {
                    throw new ArgumentException($"Cannot sort by property '{options.SortByPropertyName}'. It does not exist or is of an unsupported type.");
                }
                break;
        }

        return query;
    }

    public static IQueryable<Entry> ApplyFilters(this IQueryable<Entry> query, EntryGetOptions options, Schema schema, IPropertyValidator validator)
    {
        foreach (var filter in options.Filters)
        {
            var property = schema.Properties.FirstOrDefault(p => p.Name == filter.PropertyName);
            if (property is null) continue;

            var castResult = validator.ValidateAndCast(property, filter.ReferenceValue);
            if (castResult.IsInvalid())
            {
                throw new ArgumentException($"Invalid reference value for filter on property '{filter.PropertyName}': {string.Join(", ", castResult.ValidationErrors.Select(e => e.ErrorMessage))}");
            }

            var value = castResult.Value;

            if (value is null)
            {
                // For equals and not equals, compare property value against null
                query = filter.FilterType switch
                {
                    PropertyFilter.Equals => query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString() == null),
                    PropertyFilter.NotEquals => query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString() != null),
                    _ => throw new ArgumentException($@"Cannot apply filter '{filter.FilterType}' to 
                            property '{filter.PropertyName}' when the reference value is null.
                            Only 'Equals' and 'NotEquals' are supported for null comparisons."),
                };
                continue;
            }

            query = (property.Type, filter.FilterType) switch
            {
                (PropertyType.Number, PropertyFilter.Equals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDecimal() == (decimal)value!),
                (PropertyType.Number, PropertyFilter.NotEquals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDecimal() != (decimal)value!),
                (PropertyType.Number, PropertyFilter.GreaterThan) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDecimal() > (decimal)value!),
                (PropertyType.Number, PropertyFilter.LessThan) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDecimal() < (decimal)value!),

                (PropertyType.DateTime, PropertyFilter.Equals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDateTime() == (DateTime)value!),
                (PropertyType.DateTime, PropertyFilter.NotEquals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDateTime() != (DateTime)value!),
                (PropertyType.DateTime, PropertyFilter.GreaterThan) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDateTime() > (DateTime)value!),
                (PropertyType.DateTime, PropertyFilter.LessThan) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDateTime() < (DateTime)value!),

                (PropertyType.Text, PropertyFilter.Equals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString() == (string)value!),
                (PropertyType.Text, PropertyFilter.NotEquals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString() != (string)value!),
                (PropertyType.Text, PropertyFilter.Contains) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString()!.Contains((string)value!)),
                (PropertyType.Text, PropertyFilter.StartsWith) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString()!.StartsWith((string)value!)),
                (PropertyType.Text, PropertyFilter.EndsWith) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString()!.EndsWith((string)value!)),

                (PropertyType.Boolean, PropertyFilter.Equals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetBoolean() == (bool)value!),
                (PropertyType.Boolean, PropertyFilter.NotEquals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetBoolean() != (bool)value!),

                (PropertyType.Enum, PropertyFilter.Equals) when value is string enumString =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString() == enumString),
                (PropertyType.Enum, PropertyFilter.NotEquals) when value is string enumString =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString() != enumString),

                _ => query
            };
        }

        return query;
    }
}
