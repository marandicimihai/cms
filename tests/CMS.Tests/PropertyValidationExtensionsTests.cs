using System;
using System.Linq;
using Ardalis.Result;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;
using CMS.Main.Services.SchemaProperties;
using Xunit;

namespace CMS.Tests;

public class SchemaPropertyValidatorTests
{
    private readonly ISchemaPropertyValidator _validator;

    public SchemaPropertyValidatorTests()
    {
        var factory = new SchemaPropertyTypeHandlerFactory();
        _validator = new SchemaPropertyValidator(factory);
    }

    private static SchemaPropertyDto MakeProp(string name, SchemaPropertyType type, bool required = false, string[]? options = null)
        => new SchemaPropertyDto
        {
            Id = Guid.NewGuid().ToString(),
            SchemaId = Guid.NewGuid().ToString(),
            Name = name,
            Type = type,
            IsRequired = required,
            Options = options
        };

    [Fact]
    public void Text_Allows_String_Value()
    {
        var prop = MakeProp("Title", SchemaPropertyType.Text);
        object? val = "Hello";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello", val);
    }

    [Fact]
    public void Text_NonString_Fails_When_NotString()
    {
        var prop = MakeProp("Title", SchemaPropertyType.Text);
        object? val = 123;
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsInvalid());
        Assert.Contains("Invalid text value", result.ValidationErrors.First().ErrorMessage);
    }

    [Fact]
    public void Text_Required_Null_Fails()
    {
        var prop = MakeProp("Title", SchemaPropertyType.Text, required: true);
        object? val = null;
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsInvalid());
        Assert.Contains("required", result.ValidationErrors.First().ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Boolean_Accepts_True_String()
    {
        var prop = MakeProp("Active", SchemaPropertyType.Boolean, required: true);
        object? val = "TrUe";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        Assert.Equal(true, val);
    }

    [Theory]
    [InlineData(" TRUE ", true)]
    [InlineData(" false", false)]
    [InlineData("FaLsE", false)]
    public void Boolean_Varied_Casing_Parses(string input, bool expected)
    {
        var prop = MakeProp("Active", SchemaPropertyType.Boolean, required: true);
        object? val = input;
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, val);
    }

    [Fact]
    public void Boolean_Invalid_String_Fails()
    {
        var prop = MakeProp("Active", SchemaPropertyType.Boolean, required: true);
        object? val = "yes";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsInvalid());
        Assert.Contains("Invalid boolean", result.ValidationErrors.First().ErrorMessage);
    }

    [Fact]
    public void Boolean_Required_Blank_Fails()
    {
        var prop = MakeProp("Active", SchemaPropertyType.Boolean, required: true);
        object? val = "   ";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void DateTime_Utc_DateTime_Serializes()
    {
        var prop = MakeProp("PublishedAt", SchemaPropertyType.DateTime, required: true);
        var original = new DateTime(2025, 8, 20, 12, 30, 0, DateTimeKind.Utc);
        object? val = original;
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        var dt = Assert.IsType<DateTime>(val);
        Assert.Equal(DateTimeKind.Utc, dt.Kind);
        Assert.Equal(original, dt);
    }

    [Fact]
    public void DateTime_Local_DateTime_ConvertsToUtc()
    {
        var prop = MakeProp("PublishedAt", SchemaPropertyType.DateTime, required: true);
        var original = new DateTime(2025, 8, 20, 12, 30, 0, DateTimeKind.Local);
        object? val = original;
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        var dt = Assert.IsType<DateTime>(val);
        Assert.Equal(DateTimeKind.Utc, dt.Kind);
        Assert.Equal(original.ToUniversalTime(), dt);
    }

    [Fact]
    public void DateTime_Utc_String_Parses()
    {
        var prop = MakeProp("PublishedAt", SchemaPropertyType.DateTime, required: true);
        object? val = "2025-08-20T12:30:00Z";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        var dt = Assert.IsType<DateTime>(val);
        Assert.Equal(DateTimeKind.Utc, dt.Kind);
        Assert.Equal(new DateTime(2025, 8, 20, 12, 30, 0, DateTimeKind.Utc), dt);
    }

    [Fact]
    public void Decimal_String_Parses()
    {
        var prop = MakeProp("Price", SchemaPropertyType.Number, required: true);
        object? val = "1.23";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        Assert.Equal(1.23m, val);
    }

    [Theory]
    [InlineData("1.0", 1.0)]
    [InlineData("3.", 3.0)]
    [InlineData(".5", 0.5)]
    [InlineData("  .5  ", 0.5)]
    public void Decimal_Fraction_Forms_Parse(string input, decimal expected)
    {
        var prop = MakeProp("Price", SchemaPropertyType.Number, required: true);
        object? val = input;
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, val);
    }

    [Fact]
    public void Decimal_Invalid_String_Fails()
    {
        var prop = MakeProp("Price", SchemaPropertyType.Number, required: true);
        object? val = "1.2.3";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsInvalid());
        Assert.Contains("Invalid decimal", result.ValidationErrors.First().ErrorMessage);
    }

    [Fact]
    public void Enum_Valid_Case_Insensitive()
    {
        var prop = MakeProp("Color", SchemaPropertyType.Enum, true, new[] { "Red", "Green", "Blue" });
        object? val = "green";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        Assert.Equal("Green", val); // normalized to actual option casing
    }

    [Fact]
    public void Enum_Whitespace_Case_Insensitive_Match()
    {
        var prop = MakeProp("Color", SchemaPropertyType.Enum, true, new[] { "Red", "Green", "Blue" });
        object? val = "  RED  ";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        Assert.Equal("Red", val);
    }

    [Fact]
    public void Enum_Invalid_Value_Fails()
    {
        var prop = MakeProp("Color", SchemaPropertyType.Enum, true, new[] { "Red", "Green" });
        object? val = "Blue";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsInvalid());
        Assert.Contains("Invalid enum value", result.ValidationErrors.First().ErrorMessage);
    }

    [Fact]
    public void DateTime_Optional_String_WithOffset_ConvertsToUtc()
    {
        var prop = MakeProp("PublishedAt", SchemaPropertyType.DateTime);
        object? val = "2025-08-20T12:30:00+02:00";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        // Optional field still gets converted to UTC DateTime
        var dt = Assert.IsType<DateTime>(val);
        Assert.Equal(DateTimeKind.Utc, dt.Kind);
    }

    [Fact]
    public void DateTime_String_Unspecified_With_AssumeUniversal_Accepts()
    {
        var prop = MakeProp("PublishedAt", SchemaPropertyType.DateTime, required: true);
        object? val = "2025-08-20T12:30:00"; // unspecified kind, assumed universal
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsSuccess);
        var dt = Assert.IsType<DateTime>(val);
        Assert.Equal(DateTimeKind.Utc, dt.Kind);
    }

    [Fact]
    public void DateTime_Required_Blank_Fails()
    {
        var prop = MakeProp("PublishedAt", SchemaPropertyType.DateTime, required: true);
        object? val = "   ";
        var result = _validator.ValidateAndCast(prop, val);
        val = result.Value;
        Assert.True(result.IsInvalid());
    }

    [Fact]
    public void Text_Required_Whitespace_Passes()
    {
        var prop = MakeProp("Title", SchemaPropertyType.Text, required: true);
        object? val = "   ";
        var result = _validator.ValidateAndCast(prop, val);
        Assert.True(result.IsSuccess);
        Assert.Equal("   ", val);
    }
}
