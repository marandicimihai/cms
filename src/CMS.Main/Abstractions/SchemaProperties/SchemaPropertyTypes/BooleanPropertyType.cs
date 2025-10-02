using Ardalis.Result;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;

namespace CMS.Main.Abstractions.SchemaProperties.SchemaPropertyTypes;

public class BooleanPropertyType : ISchemaPropertyTypeHandler
{
    public SchemaPropertyType TypeName => SchemaPropertyType.Boolean;

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
