using System.Text.Json;
using Ardalis.Result;
using CMS.Main.Abstractions;
using CMS.Main.Data;
using CMS.Main.DTOs.Entry;
using CMS.Main.DTOs.Pagination;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CMS.Main.Services;

public class EntryService(
    IDbContextConcurrencyHelper dbHelper,
    ILogger<EntryService> logger
) : IEntryService
{
    public async Task<Result<(List<EntryDto>, PaginationMetadata)>> GetEntriesForSchema(
        string schemaId, 
        PaginationParams? paginationParams = null,
        Action<EntryGetOptions>? configureOptions = null)
    {
        var options = new EntryGetOptions();
        configureOptions?.Invoke(options);
        
        paginationParams ??= new PaginationParams(1, 10);
        var cappedPageSize = Math.Clamp(paginationParams.PageSize, 1, IEntryService.MaxPageSize);
        var cappedPageNumber = Math.Max(paginationParams.PageNumber, 1);
        
        try
        {
            var schema = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Schemas
                    .AsNoTracking()
                    .Include(s => s.Properties)
                    .FirstOrDefaultAsync(s => s.Id == schemaId));
            
            if (schema is null) 
                return Result.NotFound();

            var (entries, paginationMetadata) = await dbHelper.ExecuteAsync(async dbContext =>
            {
                var query = dbContext.Entries.AsQueryable();

                if (options.IncludeSchema)
                {
                    if (options.SchemaGetOptions.IncludeProperties)
                        query = query.Include(e => e.Schema).ThenInclude(s => s.Properties);

                    if (options.SchemaGetOptions.IncludeProject)
                        query = query.Include(e => e.Schema).ThenInclude(s => s.Project);
                }

                query = query
                    .AsNoTracking()
                    .Where(e => e.SchemaId == schemaId);
                
                switch (options.SortingOption)
                {
                    case EntrySortingOption.CreatedAt:
                        query = options.Descending
                            ? query.OrderByDescending(e => e.CreatedAt)
                            : query.OrderBy(e => e.CreatedAt);
                        break;
                    case EntrySortingOption.UpdatedAt:
                        query = options.Descending
                            ? query.OrderByDescending(e => e.UpdatedAt)
                            : query.OrderBy(e => e.UpdatedAt);
                        break;
                }
                    
                var entries = await query
                    .Skip((cappedPageNumber - 1) * cappedPageSize)
                    .Take(cappedPageSize)
                    .ToListAsync();

                var paginationMetadata = new PaginationMetadata(
                    await dbContext.Entries.CountAsync(e => e.SchemaId == schemaId),
                    cappedPageNumber,
                    cappedPageSize,
                    IProjectService.MaxPageSize
                );
                
                return (entries, paginationMetadata);
            });

            var dtos = entries.Select(e =>
            {
                var dto = e.Adapt<EntryDto>();
                dto.Properties = JsonToDictionary(e.Data, schema);
                return dto;
            }).ToList();
            
            return Result.Success((dtos, paginationMetadata));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting entries for schema {schemaId}.", schemaId);
            return Result.Error($"Error getting entries for schema {schemaId}.");
        }
    }

    public async Task<Result<EntryDto>> GetEntryByIdAsync(
        string entryId,
        Action<EntryGetOptions>? configureOptions = null)
    {
        var options = new EntryGetOptions();
        configureOptions?.Invoke(options);
        
        try
        {
            var entry = await dbHelper.ExecuteAsync(async dbContext =>
            {
                var query = dbContext.Entries.AsQueryable();

                if (options.IncludeSchema)
                {
                    if (options.SchemaGetOptions.IncludeProperties)
                        query = query.Include(e => e.Schema).ThenInclude(s => s.Properties);

                    if (options.SchemaGetOptions.IncludeProject)
                        query = query.Include(e => e.Schema).ThenInclude(s => s.Project);
                }
                
                return await query.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == entryId);
            });

            if (entry is null)
                return Result.NotFound();
            
            var dto = entry.Adapt<EntryDto>();
            dto.Properties = JsonToDictionary(entry.Data, entry.Schema);
            
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting entry {entryId}.", entryId);
            return Result.Error($"Error getting entry {entryId}.");
        }
    }

    private Dictionary<SchemaPropertyDto, object?> JsonToDictionary(JsonDocument data, Schema schema)
    {
        var adaptedProperties = schema.Properties.Adapt<List<SchemaPropertyDto>>();
        var dictionary = new Dictionary<SchemaPropertyDto, object?>();
        
        foreach (var property in adaptedProperties)
        {
            if (data.RootElement.TryGetProperty(property.Name, out var value))
            {
                dictionary[property] = value.ValueKind switch
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
                dictionary[property] = null;
            }
        }

        return dictionary;
    }

    public async Task<Result<EntryDto>> AddEntryAsync(
        EntryDto dto)
    {
        try
        {
            var schema = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Schemas
                    .AsNoTracking()
                    .Include(s => s.Properties)
                    .FirstOrDefaultAsync(s => s.Id == dto.SchemaId));
            
            if (schema is null) 
                return Result.NotFound();
            
            var entry = dto.Adapt<Entry>();

            var dictStringObject = new Dictionary<string, object?>();
            var dictSchemaPropertyObject = new Dictionary<SchemaPropertyDto, object?>();
            
            var validationErrors = new List<ValidationError>();

            foreach (var property in dto.Properties)
            {
                var name = property.Key.Name;

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

            var adaptedEntry = entry.Adapt<EntryDto>();
            adaptedEntry.Properties = dictSchemaPropertyObject;

            return Result.Success(adaptedEntry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating entry for schema {schemaId}.", dto.SchemaId);
            return Result.Error($"Error creating entry for schema {dto.SchemaId}.");
        }
    }

    public Task<Result> UpdateEntryAsync(EntryDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> DeleteEntryAsync(string entryId)
    {
        try
        {
            var entry = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Entries.FindAsync(entryId));

            if (entry is null)
                return Result.NotFound();

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                dbContext.Entries.Remove(entry);
                await dbContext.SaveChangesAsync();
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting entry {entryId}.", entryId);
            return Result.Error($"Error deleting entry {entryId}.");
        }
    }
}