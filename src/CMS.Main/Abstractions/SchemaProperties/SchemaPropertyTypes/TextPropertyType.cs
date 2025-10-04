using Ardalis.Result;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;

namespace CMS.Main.Abstractions.SchemaProperties.SchemaPropertyTypes;

public class TextPropertyType : ISchemaPropertyTypeHandler
{
    public SchemaPropertyType TypeName => SchemaPropertyType.Text;

    public Result<object?> CastAndValidate(SchemaProperty property, object? value)
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
