using Ardalis.Result;
using CMS.Main.Abstractions.Properties.PropertyTypes;
using CMS.Main.DTOs;
using CMS.Main.Models;
using Mapster;

namespace CMS.Main.Abstractions.SchemaProperties;

public interface IPropertyTypeHandler
{
    PropertyType TypeName { get; }

    /// <summary>
    /// Casts and validates a value for the given schema property.
    /// This method combines type casting with property-specific validation.
    /// </summary>
    /// <param name="property">The schema property definition with type, options, and constraints</param>
    /// <param name="value">The value to cast and validate</param>
    /// <returns>Success with the cast and validated value, or Invalid with validation errors</returns>
    Result<object?> CastAndValidate(Property property, object? value);

    Result<object?> CastAndValidate(PropertyDto property, object? value)
        => CastAndValidate(property.Adapt<Property>(), value);
}

/// <summary>
/// Factory for resolving schema property type handlers by type.
/// </summary>
public interface IPropertyTypeHandlerFactory
{
    /// <summary>
    /// Gets the appropriate handler for the specified schema property type.
    /// </summary>
    /// <param name="propertyType">The schema property type</param>
    /// <returns>The handler for the specified type</returns>
    IPropertyTypeHandler GetHandler(PropertyType propertyType);
}