using System.Text.Json;
using System.Text.Json.Serialization;
using CMS.Main.DTOs.SchemaProperty;

namespace CMS.Main.DTOs.Entry
{
    public class SchemaPropertyDtoDictionaryConverter : JsonConverter<Dictionary<SchemaPropertyDto, object?>>
    {
        public override Dictionary<SchemaPropertyDto, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<SchemaPropertyDto, object?> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var kvp in value)
            {
                // Use Id as the key for serialization
                writer.WritePropertyName(kvp.Key.Name);
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}

