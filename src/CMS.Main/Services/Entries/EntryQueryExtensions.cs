using Ardalis.Result;
using CMS.Main.Abstractions.Entries;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;

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

    public static IQueryable<Entry> ApplyFilters(this IQueryable<Entry> query, EntryGetOptions options, Schema schema)
    {
        foreach (var filter in options.Filters)
        {
            var property = schema.Properties.FirstOrDefault(p => p.Name == filter.PropertyName);
            
            if (property is null) continue;

            var castResult = PropertyValidator.CastToPropertyType(property, filter.ReferenceValue);
            if (castResult.IsInvalid())
            {
                throw new ArgumentException($"Invalid reference value for filter on property '{filter.PropertyName}': {string.Join(", ", castResult.ValidationErrors.Select(e => e.ErrorMessage))}");
            }

            var castedReferenceValue = castResult.Value;

            switch (filter.FilterType)
            {
                case PropertyFilter.Equals:
                    query = query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).ToString() == (string)castedReferenceValue!);
                    break;
                case PropertyFilter.NotEquals:
                    query = query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).ToString() != (string)castedReferenceValue!);
                    break;
                case PropertyFilter.GreaterThan:
                    if (property.Type == SchemaPropertyType.Number)
                    {
                        query = query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDecimal() > (decimal)castedReferenceValue!);
                    }
                    else if (property.Type == SchemaPropertyType.DateTime)
                    {
                        query = query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDateTime() > (DateTime)castedReferenceValue!);
                    }
                    break;
                case PropertyFilter.LessThan:
                    if (property.Type == SchemaPropertyType.Number)
                    {
                        query = query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDecimal() < (decimal)castedReferenceValue!);
                    }
                    else if (property.Type == SchemaPropertyType.DateTime)
                    {
                        query = query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetDateTime() < (DateTime)castedReferenceValue!);
                    }
                    break;
                case PropertyFilter.Contains:
                    if (property.Type == SchemaPropertyType.Text)
                    {
                        query = query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString().Contains((string)castedReferenceValue!));
                    }
                    break;
                case PropertyFilter.StartsWith:
                    if (property.Type == SchemaPropertyType.Text)
                    {
                        query = query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString().StartsWith((string)castedReferenceValue!));
                    }
                    break;
                case PropertyFilter.EndsWith:
                    if (property.Type == SchemaPropertyType.Text)
                    {
                        query = query.Where(e => e.Data.RootElement.GetProperty(filter.PropertyName).GetString().EndsWith((string)castedReferenceValue!));
                    }
                    break;
            }
        }

        return query;
    }
}
