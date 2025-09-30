using System.Globalization;
using Ardalis.Result;
using CMS.Main.DTOs.SchemaProperty;
using Mapster;

namespace CMS.Main.Models;

public class PropertyValidator
{
    /// <summary>
    /// Attempts to convert a provided value to the canonical representation for the given schema property.
    /// </summary>
    /// <param name="property">The schema property definition describing the expected type and options.</param>
    /// <param name="value">The input value to convert. This value is not mutated by the method (a converted value is returned inside the result on success).</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the converted/normalized value on success, or an invalid result with a <see cref="ValidationError"/>
    /// describing the reason for failure.
    /// </returns>
    /// <remarks>
    /// Conversion behavior by property type:
    /// <para>
    /// - Text: the value is cast to <see cref="string"/> (null if not a string).<br/>
    /// - Boolean: accepts <see cref="bool"/> or string values "true"/"false" (case-insensitive). An empty string becomes null. Any other input yields an invalid result.<br/>
    /// - DateTime: accepts <see cref="DateTime"/> or a date/time string. The value must represent UTC; otherwise an invalid result is returned. On success the value is normalized to an ISO 8601 UTC string (format "o").
    ///   Parsing uses <see cref="CultureInfo.InvariantCulture"/> with <see cref="System.Globalization.DateTimeStyles.AssumeUniversal"/> | <see cref="System.Globalization.DateTimeStyles.AdjustToUniversal"/>.<br/>
    /// - Number: accepts <see cref="decimal"/> or numeric strings parsed with <see cref="NumberStyles.Any"/> and <see cref="CultureInfo.InvariantCulture"/>. An empty string becomes null. Non-numeric inputs yield an invalid result.<br/>
    /// - Enum: accepts string values and matches them case-insensitively against <see cref="SchemaPropertyDto.Options"/>; on match the canonical option string is returned. Unrecognized values yield an invalid result.<br/>
    /// </para>
    /// Note: this method focuses on conversion/normalization and does not enforce <see cref="SchemaPropertyDto.IsRequired"/> constraints.
    /// Errors are returned as invalid <see cref="Result{T}"/> containing a <see cref="ValidationError"/> with a descriptive message.
    /// </remarks>
    public static Result<object?> CastToPropertyType(SchemaProperty property, object? value)
    {
        switch (property.Type)
        {
            // ! Value accepted if it is a string
            case SchemaPropertyType.Text:
                if (value is string strValue)
                {
                    value = strValue;
                }
                else
                {
                    return Result.Invalid(new ValidationError($"Invalid string value for property '{property.Name}'."));
                }
                break;
            // ! Value accepted if it is a bool or a string "true"/"false" (case-insensitive)
            case SchemaPropertyType.Boolean:
                if (value is bool boolValue)
                {
                    value = boolValue;
                }
                else if (value is string boolString)
                {
                    boolString = boolString.Trim().ToLowerInvariant();
                    if (boolString == "true") value = true;
                    else if (boolString == "false") value = false;
                    else return Result.Invalid(new ValidationError($"Invalid boolean value for property '{property.Name}'."));
                }
                else
                {
                    return Result.Invalid(new ValidationError($"Invalid boolean value for property '{property.Name}'."));
                }
                break;
            // ! Value accepted if it is a DateTime (must be UTC) or a date/time string (must parse to UTC)
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
                    if (DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedDt))
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
                    return Result.Invalid(new ValidationError($"Invalid date/time value for property '{property.Name}'."));
                }
                break;
            // ! Value accepted if it is a decimal or a numeric string
            case SchemaPropertyType.Number:
                if (value is decimal decimalValue)
                {
                    value = decimalValue;
                }
                else if (value is string decimalString)
                {
                    decimalString = decimalString.Trim();
                    if (decimal.TryParse(decimalString, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDec))
                    {
                        value = parsedDec;
                    }
                    else
                    {
                        return Result.Invalid(new ValidationError($"Invalid decimal value for property '{property.Name}'."));
                    }
                }
                else
                {
                    return Result.Invalid(new ValidationError($"Invalid decimal value for property '{property.Name}'."));
                }
                break;
            // ! Value accepted if it is a string matching one of the enum options (case-insensitive)
            case SchemaPropertyType.Enum:
                if (property.Options is not null)
                {
                    if (value is string enumString)
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
                        return Result.Invalid(new ValidationError(
                            $"Invalid enum value for property '{property.Name}'. Allowed: [{string.Join(", ", property.Options)}]"));
                    }
                }
                else
                {
                    return Result.Invalid(new ValidationError(
                        $"No enum options available for property '{property.Name}'. Please edit the property to add some options."));
                }
                break;
        }

        return value;
    }
    

    /// <summary>
    /// Validates and converts a value for the given schema property. Updates <paramref name="value"/> to the normalized result.
    /// </summary>
    /// <param name="property">The schema property definition.</param>
    /// <param name="value">The value to validate and convert. Updated to the normalized value if valid.</param>
    /// <returns>Success if valid; otherwise, an invalid result with validation errors.</returns>
    public static Result ValidateProperty(SchemaProperty property, ref object? value)
    {
        // Don't validate null values for non-required fields
        if (property.IsRequired && value is not null)
        {
            var castResult = CastToPropertyType(property, value);
            if (castResult.IsInvalid())
            {
                return Result.Invalid(castResult.ValidationErrors);
            }
            value = castResult.Value;
        }

        // Enforce field-specific constraints
        if (property.IsRequired && value is null or "")
        {
            return Result.Invalid(new ValidationError($"Property '{property.Name}' is required and cannot be null or empty."));
        }

        return Result.Success();
    }

    /// <inheritdoc cref="ValidateProperty(SchemaProperty, ref object?)"/>
    public static Result<object?> ValidateProperty(SchemaPropertyDto property, ref object? value)
        => ValidateProperty(property.Adapt<SchemaProperty>(), ref value);
}
