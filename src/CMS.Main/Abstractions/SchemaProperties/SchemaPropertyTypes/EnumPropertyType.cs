using Ardalis.Result;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;

namespace CMS.Main.Abstractions.SchemaProperties.SchemaPropertyTypes;

public class EnumPropertyType : ISchemaPropertyTypeHandler
{
    public SchemaPropertyType TypeName => SchemaPropertyType.Enum;

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

        // Validate that enum options are configured
        if (property.Options is null || property.Options.Length == 0)
        {
            return Result.Invalid(new ValidationError(
                $"No enum options available for property '{property.Name}'. Please edit the property to add some options."));
        }

        // Cast and validate enum value
        if (value is string enumString)
        {
            enumString = enumString.Trim();
            var match = property.Options.FirstOrDefault(opt => 
                string.Equals(opt, enumString, StringComparison.OrdinalIgnoreCase));
            
            if (match != null)
            {
                return Result.Success<object?>(match); // Return the canonical option string
            }
            else
            {
                return Result.Invalid(new ValidationError(
                    $"Invalid enum value '{enumString}' for property '{property.Name}'. Allowed: [{string.Join(", ", property.Options)}]"));
            }
        }
        else
        {
            return Result.Invalid(new ValidationError(
                $"Invalid enum value for property '{property.Name}'. Expected string value from allowed options: [{string.Join(", ", property.Options ?? Array.Empty<string>())}]"));
        }
    }
}