using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Ardalis.Result;

namespace CMS.Main.Models;

public class Entry : IDisposable
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = default!;

    [Required]
    [StringLength(36)]
    public string SchemaId { get; set; } = default!;
    public Schema Schema { get; set; } = default!;

    [Required]
    public JsonDocument Data { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Data?.Dispose();
    }

    public Dictionary<string, object?> GetFields(List<SchemaProperty> properties)
    {
        var dictionary = new Dictionary<string, object?>();
        
        foreach (var property in properties)
        {
            if (Data.RootElement.TryGetProperty(property.Name, out var value))
            {
                dictionary[property.Name] = value.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.False => false,
                    JsonValueKind.True => true,
                    JsonValueKind.Number => value.TryGetInt32(out var intValue) ? intValue : value.GetDecimal(),
                    _ => null
                };
            }
            else
            {
                dictionary[property.Name] = null;
            }
        }

        return dictionary;
    }

    
    /// <summary>
    /// Validates and sets entry fields from the provided dictionary using the schema properties.
    /// Updates the entry's <see cref="Data"/> and returns the normalized fields if all are valid; otherwise returns validation errors.
    /// </summary>
    /// <param name="properties">The schema property definitions.</param>
    /// <param name="fields">Field values to validate and set.</param>
    /// <returns>
    /// Success with the normalized fields if all are valid; otherwise, an invalid result with validation errors.
    /// </returns>
    public Result<Dictionary<string, object?>> SetFields(
        List<SchemaProperty> properties,
        Dictionary<string, object?> fields)
    {
        Dictionary<string, object?> validFields = [];
        List<ValidationError> validationErrors = [];
        List<string> seenFieldNames = [];

        foreach (var field in fields)
        {
            // If we have already seen this field name (duplicate), move on
            var fieldName = field.Key;
            if (seenFieldNames.Contains(fieldName))
            {
                continue;
            }
            seenFieldNames.Add(fieldName);

            // If the property does not exist in the schema, move on
            var property = properties.FirstOrDefault(p => p.Name == fieldName);
            if (property == null)
            {
                continue;
            }

            // Validate the field value
            var fieldValue = field.Value;
            var validationResult = PropertyValidator.ValidateProperty(property, ref fieldValue);
            if (validationResult.IsInvalid())
            {
                validationErrors.AddRange(validationResult.ValidationErrors);
                continue;
            }

            // If valid, add to the valid fields dictionary
            validFields.Add(fieldName, fieldValue);
        }

        foreach (var property in properties)
        {
            if (property.IsRequired && !seenFieldNames.Contains(property.Name))
            {
                validationErrors.Add(new ValidationError($"Property '{property.Name}' is required and must be provided."));
            }
        }

        if (validationErrors.Count == 0)
        {
            var json = JsonSerializer.Serialize(validFields);
            Data?.Dispose();
            Data = JsonDocument.Parse(json);
            return Result.Success(validFields);
        }
        else
        {
            return Result.Invalid(validationErrors);
        }
    }
}