using Ardalis.Result;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.DTOs.SchemaProperty;
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
    Result<object?> ValidateAndCast(SchemaProperty property, object? value, bool validateIsRequired = true);

    /// <inheritdoc cref="ValidateAndCast(SchemaProperty, object?)"/>
    Result<object?> ValidateAndCast(SchemaPropertyDto property, object? value, bool validateIsRequired = true);
}

public class SchemaPropertyValidator(ISchemaPropertyTypeHandlerFactory handlerFactory) : ISchemaPropertyValidator
{
    private readonly ISchemaPropertyTypeHandlerFactory handlerFactory = handlerFactory;

    public Result<object?> ValidateAndCast(SchemaProperty property, object? value, bool validateIsRequired = true)
    {
        var handler = handlerFactory.GetHandler(property.Type);
        // If handler.CastAndValidate supports validateIsRequired, pass it; otherwise, ignore
        return handler.CastAndValidate(property, value, validateIsRequired);
    }

    public Result<object?> ValidateAndCast(SchemaPropertyDto property, object? value, bool validateIsRequired = true)
        => ValidateAndCast(property.Adapt<SchemaProperty>(), value, validateIsRequired);
}