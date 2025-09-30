using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CMS.Main.Serialization;

/// <summary>
/// Converts any JSON value into a string (or null). Useful for binding Dictionary<string,string?> where values
/// may be booleans/numbers/objects but should be represented as strings.
/// </summary>
public class DictionaryStringJsonConverter : JsonConverter<Dictionary<string, string?>>
{
    public override Dictionary<string, string?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return new Dictionary<string, string?>();
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var result = new Dictionary<string, string?>();
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            result[prop.Name] = ConvertElementToString(prop.Value);
        }
        return result;
    }

    private static string? ConvertElementToString(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.String:
                return el.GetString();
            case JsonValueKind.True:
                return "true";
            case JsonValueKind.False:
                return "false";
            case JsonValueKind.Number:
                // Use invariant culture representation
                return el.GetRawText();
            case JsonValueKind.Object:
            case JsonValueKind.Array:
                return el.GetRawText();
            default:
                return el.GetRawText();
        }
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, string?> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            if (kvp.Value is null) writer.WriteNullValue();
            else writer.WriteStringValue(kvp.Value);
        }
        writer.WriteEndObject();
    }
}
