using Ardalis.Result;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.DTOs;
using CMS.Main.Models;

namespace CMS.Main.Abstractions.Properties.PropertyTypes;

public class TextPropertyType : IPropertyTypeHandler
{
    public PropertyType TypeName => PropertyType.Text;

    public Result<object?> CastAndValidate(Property property, object? value)
    {
        // Handle null/empty values first
        if (value is null or "")
        {
            if (property.IsRequired)
            {
                return Result.Invalid(new ValidationError($"Property '{property.Name}' cannot be null or empty."));
            }
            return Result.Success(value);
        }

        // Cast to string
        if (value is string strValue)
        {
            return Result.Success<object?>(strValue);
        }
        else
        {
            return Result.Invalid(new ValidationError($"Invalid text value for property '{property.Name}'. Expected string value."));
        }
    }
}
