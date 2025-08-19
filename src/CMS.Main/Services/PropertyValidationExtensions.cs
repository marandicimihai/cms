using Ardalis.Result;
using CMS.Shared.DTOs.SchemaProperty;

namespace CMS.Main.Services;

public static class PropertyValidationExtensions
{
    /// <summary>
    /// Provides extension methods for validating and normalizing property values based on their schema type.
    /// </summary>
    public static Result ValidateProperty(SchemaPropertyWithIdDto property, ref object? value)
    {
        switch (property.Type)
        {
            case SchemaPropertyType.Text:
                value = value is string s && !string.IsNullOrWhiteSpace(s) ? s : null;
                break;
            case SchemaPropertyType.Integer:
                value = value is int i ? i : null;
                break;
            case SchemaPropertyType.Boolean:
                value = value is bool b ? b : null;
                break;
            case SchemaPropertyType.DateTime:
                if (value is DateTime dt)
                    value = dt.ToUniversalTime().ToString("o");
                else
                    value = null;
                break;
            case SchemaPropertyType.Decimal:
                value = value is decimal d ? d : null;
                break;
            case SchemaPropertyType.Enum:
                if (property.Options != null && value is string enumVal)
                {
                    if (!property.Options.Contains(enumVal))
                    {
                        return Result.Invalid(new ValidationError(
                            $"Invalid enum value '{enumVal}' for property '{property.Name}'. Allowed: [{string.Join(", ", property.Options)}]"));
                    }
                    value = enumVal;
                }
                else
                {
                    value = null;
                }
                break;
        }
        
        if (property.IsRequired && value == null)
        {
            return Result.Invalid(new ValidationError($"Property '{property.Name}' is required."));
        }
        
        return Result.Success();
    }
}