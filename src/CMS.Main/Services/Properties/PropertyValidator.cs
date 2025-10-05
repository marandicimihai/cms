using Ardalis.Result;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.DTOs;
using CMS.Main.Models;
using Mapster;

namespace CMS.Main.Services.SchemaProperties;

public interface ISchemaPropertyValidator
{
    /// <summary>
    /// Validates and converts a value for the given schema property.
    /// </summary>
    /// <param name="property">The schema property definition</param>
    /// <param name="value">The value to validate and convert</param>
    /// <returns>Success containing a normalized object if valid; otherwise, an invalid result with validation errors</returns>
    Result<object?> ValidateAndCast(Property property, object? value);

    /// <inheritdoc cref="ValidateAndCast(Property, object?)"/>
    Result<object?> ValidateAndCast(PropertyDto property, object? value);
}

public class SchemaPropertyValidator(IPropertyTypeHandlerFactory handlerFactory) : ISchemaPropertyValidator
{
    private readonly IPropertyTypeHandlerFactory handlerFactory = handlerFactory;

    public Result<object?> ValidateAndCast(Property property, object? value)
    {
        var handler = handlerFactory.GetHandler(property.Type);
        // If handler.CastAndValidate supports validateIsRequired, pass it; otherwise, ignore
        return handler.CastAndValidate(property, value);
    }

    public Result<object?> ValidateAndCast(PropertyDto property, object? value)
        => ValidateAndCast(property.Adapt<Property>(), value);
}