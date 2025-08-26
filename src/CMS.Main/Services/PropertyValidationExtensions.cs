using System.Globalization;
using System.Runtime.InteropServices;
using Ardalis.Result;
using CMS.Main.DTOs.SchemaProperty;

namespace CMS.Main.Services;

public static class PropertyValidationExtensions
{
    /// <summary>
    /// Provides extension methods for validating and normalizing property values based on their schema type.
    /// Only utc date-times allowed
    /// <param name="value">Is an object which usually holds a string value from user inputs and is then parsed
    /// to the required type</param>
    /// </summary>
    public static Result ValidateProperty(SchemaPropertyDto property, ref object? value)
    {
        switch (property.Type)
        {
            case SchemaPropertyType.Text:
                value = value as string;
                break;
            case SchemaPropertyType.Integer:
                if (value is int intValue)
                {
                    value = intValue;
                }
                else if (value is string intString)
                {
                    intString = intString.Trim();
                    if (string.IsNullOrEmpty(intString))
                    {
                        value = null;
                    }
                    else if (int.TryParse(intString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt))
                    {
                        value = parsedInt;
                    }
                    else
                    {
                        return Result.Invalid(new ValidationError($"Invalid integer value for property '{property.Name}'."));
                    }
                }
                else if (value is not null)
                {
                    return Result.Invalid(new ValidationError($"Invalid integer value for property '{property.Name}'."));
                }
                else
                {
                    value = null;
                }
                break;
            case SchemaPropertyType.Boolean:
                switch (value)
                {
                    case bool boolValue:
                        value = boolValue;
                        break;
                    case string boolString:
                    {
                        boolString = boolString.Trim().ToLowerInvariant();
                        if (boolString == "true") value = true;
                        else if (boolString == "false") value = false;
                        else if (string.IsNullOrEmpty(boolString)) value = null;
                        else return Result.Invalid(new ValidationError($"Invalid boolean value for property '{property.Name}'."));
                        break;
                    }
                    default:
                        value = null;
                        break;
                }
                break;
            case SchemaPropertyType.DateTime:
                if (value is DateTime dateTimeValue)
                {
                    if (dateTimeValue.Kind != DateTimeKind.Utc)
                    {
                        return Result.Invalid(new ValidationError($"Date/time value for property '{property.Name}' must be in UTC."));
                    }
                    value = dateTimeValue.ToUniversalTime().ToString("o");
                }
                else if (value is string dateTimeString)
                {
                    dateTimeString = dateTimeString.Trim();
                    if (string.IsNullOrEmpty(dateTimeString))
                    {
                        value = null;
                    }
                    else if (DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedDt))
                    {
                        if (parsedDt.Kind != DateTimeKind.Utc)
                        {
                            return Result.Invalid(new ValidationError($"Date/time value for property '{property.Name}' must be in UTC."));
                        }
                        value = parsedDt.ToUniversalTime().ToString("o");
                    }
                    else
                    {
                        return Result.Invalid(new ValidationError($"Invalid date/time value for property '{property.Name}'."));
                    }
                }
                else
                {
                    value = null;
                }
                break;
            case SchemaPropertyType.Decimal:
                if (value is decimal decimalValue)
                {
                    value = decimalValue;
                }
                else if (value is string decimalString)
                {
                    decimalString = decimalString.Trim();
                    if (string.IsNullOrEmpty(decimalString))
                    {
                        value = null;
                    }
                    else if (decimal.TryParse(decimalString, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDec))
                    {
                        value = parsedDec;
                    }
                    else
                    {
                        return Result.Invalid(new ValidationError($"Invalid decimal value for property '{property.Name}'."));
                    }
                }
                else if (value is not null)
                {
                    return Result.Invalid(new ValidationError($"Invalid decimal value for property '{property.Name}'."));
                }
                else
                {
                    value = null;
                }
                break;
            case SchemaPropertyType.Enum:
                if (property.Options != null && value is string enumString)
                {
                    enumString = enumString.Trim();
                    var match = property.Options.FirstOrDefault(opt => string.Equals(opt, enumString, StringComparison.OrdinalIgnoreCase));
                    if (match == null)
                    {
                        return Result.Invalid(new ValidationError(
                            $"Invalid enum value '{enumString}' for property '{property.Name}'. Allowed: [{string.Join(", ", property.Options)}]"));
                    }
                    value = match;
                }
                else
                {
                    value = null;
                }
                break;
        }
        
        if (property.IsRequired && value is null or "")
        {
            return Result.Invalid(new ValidationError($"Property '{property.Name}' is required and cannot be null or empty."));
        }
        
        return Result.Success();
    }
}