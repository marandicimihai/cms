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
                value = value is string s ? s.Trim() : null;
                break;
            case SchemaPropertyType.Integer:
                value = value is int i ? i : 0;
                break;
            case SchemaPropertyType.Boolean:
                value = value is true;
                break;
            case SchemaPropertyType.DateTime:
                if (value is DateTime dt)
                    value = dt.ToUniversalTime().ToString("o");
                else
                    value = DateTime.MinValue.ToUniversalTime().ToString("o");
                break;
            case SchemaPropertyType.Decimal:
                value = value is decimal d ? d : 0.0M;
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
                    return Result.Invalid(new ValidationError($"Enum value missing or options not defined for property '{property.Name}'."));
                }
                break;
        }
        
        return Result.Success();
    }
}