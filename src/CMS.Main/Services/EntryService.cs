using System.Text.Json;
using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.Models;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Entry;
using CMS.Shared.DTOs.SchemaProperty;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Services;

public class EntryService(
    DbContextConcurrencyHelper dbHelper,
    ILogger<EntryService> logger
) : IEntryService
{
    public async Task<Result<EntryWithIdDto>> AddEntryAsync(EntryCreationDto creationDto)
    {
        try
        {
            var schema = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Schemas
                    .AsNoTracking()
                    .Include(s => s.Properties)
                    .FirstOrDefaultAsync(s => s.Id == creationDto.SchemaId));
            
            if (schema is null) 
                return Result.NotFound();
            
            var entry = creationDto.Adapt<Entry>();

            var dictStringObject = new Dictionary<string, object?>();
            var dictSchemaPropertyObject = new Dictionary<SchemaPropertyWithIdDto, object?>();
            
            var validationErrors = new List<ValidationError>();

            foreach (var property in creationDto.Properties)
            {
                var name = property.Key.Name.Trim();

                // If property does not exist in schema, skip it
                var schemaProp = schema.Properties.FirstOrDefault(p => p.Name == name);
                if (schemaProp == null)
                {
                    validationErrors.Add(new ValidationError($"Property {name} does not exist in schema."));
                    continue;
                }

                var finalValue = property.Value;
                var validationResult = PropertyValidationExtensions.ValidateProperty(property.Key, ref finalValue);
                if (validationResult.IsInvalid())
                {
                    validationErrors.AddRange(validationResult.ValidationErrors);
                    continue;
                }

                dictStringObject.Add(name, finalValue);
                dictSchemaPropertyObject.Add(property.Key, finalValue);
            }

            if (validationErrors.Count > 0)
            {
                return Result.Invalid(validationErrors);
            }

            entry.Data = JsonDocument.Parse(JsonSerializer.Serialize(dictStringObject));

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                await dbContext.Entries.AddAsync(entry);
                await dbContext.SaveChangesAsync();
            });

            var adaptedEntry = entry.Adapt<EntryWithIdDto>();
            adaptedEntry.Properties = dictSchemaPropertyObject;

            return Result.Success(adaptedEntry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating entry for schema {schemaId}", creationDto.SchemaId);
            return Result.Error($"Error creating entry for schema {creationDto.SchemaId}");
        }
    }
}