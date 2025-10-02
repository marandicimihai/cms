using Ardalis.Result;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;
using Mapster;

namespace CMS.Main.Abstractions.SchemaProperties;

public interface ISchemaPropertyTypeHandler
{
    SchemaPropertyType TypeName { get; }

    /// <summary>
    /// Casts and validates a value for the given schema property.
    /// This method combines type casting with property-specific validation.
    /// </summary>
    /// <param name="property">The schema property definition with type, options, and constraints</param>
    /// <param name="value">The value to cast and validate</param>
    /// <returns>Success with the cast and validated value, or Invalid with validation errors</returns>
    Result<object?> CastAndValidate(SchemaProperty property, object? value, bool validateIsRequired = true);

    Result<object?> CastAndValidate(SchemaPropertyDto property, object? value, bool validateIsRequired = true)
        => CastAndValidate(property.Adapt<SchemaProperty>(), value, validateIsRequired);
}

/// <summary>
/// Factory for resolving schema property type handlers by type.
/// </summary>
public interface ISchemaPropertyTypeHandlerFactory
{
    /// <summary>
    /// Gets the appropriate handler for the specified schema property type.
    /// </summary>
    /// <param name="propertyType">The schema property type</param>
    /// <returns>The handler for the specified type</returns>
    ISchemaPropertyTypeHandler GetHandler(SchemaPropertyType propertyType);
}