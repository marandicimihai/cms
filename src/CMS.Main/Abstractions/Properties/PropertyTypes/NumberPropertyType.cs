using System.Globalization;
using Ardalis.Result;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.Models;

namespace CMS.Main.Abstractions.Properties.PropertyTypes;

public class NumberPropertyType : IPropertyTypeHandler
{
    public PropertyType TypeName => PropertyType.Number;

    public Result<object?> CastAndValidate(Property property, object? value)
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