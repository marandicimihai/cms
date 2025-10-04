using Ardalis.Result;
using CMS.Main.Abstractions.Entries;
using CMS.Main.DTOs.SchemaProperty;
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
                        (property.Type == SchemaPropertyType.Text ||
                         property.Type == SchemaPropertyType.Number ||
                         property.Type == SchemaPropertyType.DateTime))
                {
                    if (property.Type == SchemaPropertyType.Number)
                    {
                        query = options.Descending
                            ? query.OrderByDescending(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDecimal())
                            : query.OrderBy(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDecimal());
                        break;
                    }
                    else if (property.Type == SchemaPropertyType.DateTime)
                    {
                        query = options.Descending
                            ? query.OrderByDescending(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDateTime())
                            : query.OrderBy(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDateTime());
                    }
                    else if (property.Type == SchemaPropertyType.Text)
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

    public static IQueryable<Entry> ApplyFilters(this IQueryable<Entry> query, EntryGetOptions options, Schema schema, ISchemaPropertyValidator validator)
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
                (SchemaPropertyType.Number, PropertyFilter.Equals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDecimal() == (decimal)value!),
                (SchemaPropertyType.Number, PropertyFilter.NotEquals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDecimal() != (decimal)value!),
                (SchemaPropertyType.Number, PropertyFilter.GreaterThan) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDecimal() > (decimal)value!),
                (SchemaPropertyType.Number, PropertyFilter.LessThan) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDecimal() < (decimal)value!),

                (SchemaPropertyType.DateTime, PropertyFilter.Equals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDateTime() == (DateTime)value!),
                (SchemaPropertyType.DateTime, PropertyFilter.NotEquals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDateTime() != (DateTime)value!),
                (SchemaPropertyType.DateTime, PropertyFilter.GreaterThan) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDateTime() > (DateTime)value!),
                (SchemaPropertyType.DateTime, PropertyFilter.LessThan) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDateTime() < (DateTime)value!),

                (SchemaPropertyType.Text, PropertyFilter.Equals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString() == (string)value!),
                (SchemaPropertyType.Text, PropertyFilter.NotEquals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString() != (string)value!),
                (SchemaPropertyType.Text, PropertyFilter.Contains) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString()!.Contains((string)value!)),
                (SchemaPropertyType.Text, PropertyFilter.StartsWith) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString()!.StartsWith((string)value!)),
                (SchemaPropertyType.Text, PropertyFilter.EndsWith) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString()!.EndsWith((string)value!)),

                (SchemaPropertyType.Boolean, PropertyFilter.Equals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetBoolean() == (bool)value!),
                (SchemaPropertyType.Boolean, PropertyFilter.NotEquals) =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetBoolean() != (bool)value!),

                (SchemaPropertyType.Enum, PropertyFilter.Equals) when value is string enumString =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString() == enumString),
                (SchemaPropertyType.Enum, PropertyFilter.NotEquals) when value is string enumString =>
                    query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString() != enumString),

                _ => query
            };
        }

        return query;
    }
}
