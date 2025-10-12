using Ardalis.Result;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.DTOs;
using CMS.Main.Models;

namespace CMS.Main.Abstractions.Properties.PropertyTypes;

public class BooleanPropertyType : IPropertyTypeHandler
{
    public PropertyType TypeName => PropertyType.Boolean;

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

        // Cast to boolean
        if (value is bool boolValue)
        {
            return Result.Success<object?>(boolValue);
        }
        else if (value is string boolString)
        {
            boolString = boolString.Trim().ToLowerInvariant();
            if (boolString == "true") 
                return Result.Success<object?>(true);
            else if (boolString == "false") 
                return Result.Success<object?>(false);
            else 
                return Result.Invalid(new ValidationError($"Invalid boolean value for property '{property.Name}'. Expected 'true' or 'false'."));
        }
        else
        {
            return Result.Invalid(new ValidationError($"Invalid boolean value for property '{property.Name}'. Expected boolean or string value."));
        }
    }
}
