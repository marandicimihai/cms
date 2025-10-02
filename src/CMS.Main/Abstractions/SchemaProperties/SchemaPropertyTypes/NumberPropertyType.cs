using System.Globalization;
using Ardalis.Result;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;

namespace CMS.Main.Abstractions.SchemaProperties.SchemaPropertyTypes;

public class NumberPropertyType : ISchemaPropertyTypeHandler
{
    public SchemaPropertyType TypeName => SchemaPropertyType.Number;

    public Result<object?> CastAndValidate(SchemaProperty property, object? value, bool validateIsRequired = true)
    {
        // Handle null values first
        if (value is null)
        {
            if (validateIsRequired && property.IsRequired)
            {
                return Result.Invalid(new ValidationError($"Property '{property.Name}' is required and cannot be null."));
            }
            return Result.Success(value);
        }

        // Cast to decimal
        if (value is decimal decimalValue)
        {
            return Result.Success<object?>(decimalValue);
        }
        else if (value is string decimalString)
        {
            decimalString = decimalString.Trim();
            if (decimal.TryParse(decimalString, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDec))
            {
                return Result.Success<object?>(parsedDec);
            }
            else
            {
                return Result.Invalid(new ValidationError($"Invalid decimal value for property '{property.Name}'. Expected valid number format."));
            }
        }
        else
        {
            return Result.Invalid(new ValidationError($"Invalid decimal value for property '{property.Name}'. Expected decimal or string value."));
        }
    }
}