using System.Globalization;
using Ardalis.Result;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;

namespace CMS.Main.Abstractions.SchemaProperties.SchemaPropertyTypes;

public class DateTimePropertyType : ISchemaPropertyTypeHandler
{
    public SchemaPropertyType TypeName => SchemaPropertyType.DateTime;

    public Result<object?> CastAndValidate(SchemaProperty property, object? value)
    {
        // Handle null values first
        if (value is null)
        {
            if (property.IsRequired)
            {
                return Result.Invalid(new ValidationError($"Property '{property.Name}' cannot be null."));
            }
            return Result.Success(value);
        }

        // Cast to DateTime (always convert to UTC)
        if (value is DateTime dateTimeValue)
        {
            if (dateTimeValue.Kind == DateTimeKind.Unspecified)
            {
                dateTimeValue = DateTime.SpecifyKind(dateTimeValue, DateTimeKind.Utc);
            }
            return Result.Success<object?>(dateTimeValue.ToUniversalTime());
        }
        else if (value is DateTimeOffset dto)
        {
            return Result.Success<object?>(dto.ToUniversalTime().UtcDateTime);
        }
        else if (value is string dateTimeString)
        {
            dateTimeString = dateTimeString.Trim();
            if (DateTimeOffset.TryParse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedDt))
            {
                return Result.Success<object?>(parsedDt.ToUniversalTime().UtcDateTime);
            }
            else
            {
                return Result.Invalid(new ValidationError($"Invalid date/time value for property '{property.Name}'. Expected valid date/time format."));
            }
        }
        else
        {
            return Result.Invalid(new ValidationError($"Invalid date/time value for property '{property.Name}'. Expected DateTime, DateTimeOffset, or string value."));
        }
    }
}