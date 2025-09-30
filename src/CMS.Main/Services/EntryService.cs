using System.Text.Json;
using Ardalis.Result;
using CMS.Main.Abstractions.Entries;
using CMS.Main.Data;
using CMS.Main.DTOs.Entry;
using CMS.Main.DTOs.Pagination;
using CMS.Main.DTOs.Schema;
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
                    .Include(s => s.Project)
                    .FirstOrDefaultAsync(s => s.Id == schemaId));
            
            if (schema is null) 
                return Result.NotFound();

            var (entries, paginationMetadata) = await dbHelper.ExecuteAsync(async dbContext =>
            {
                var query = dbContext.Entries
                    .AsNoTracking()
                    .Where(e => e.SchemaId == schemaId)
                    .AsQueryable();

                // Sort the entries
                switch (options.SortByPropertyName)
                {
                    case "CreatedAt":
                        query = options.Descending
                            ? query.OrderByDescending(e => e.CreatedAt)
                            : query.OrderBy(e => e.CreatedAt);
                        break;
                    case "UpdatedAt":
                        query = options.Descending
                            ? query.OrderByDescending(e => e.UpdatedAt)
                            : query.OrderBy(e => e.UpdatedAt);
                        break;
                    default:
                        // Sort by custom property
                        var property = schema.Properties.FirstOrDefault(p => p.Name == options.SortByPropertyName);
                        if (property is not null && (property.Type == SchemaPropertyType.Text || property.Type == SchemaPropertyType.Number || property.Type == SchemaPropertyType.DateTime))
                        {
                            if (property.Type == SchemaPropertyType.Number)
                            {
                                query = options.Descending
                                    ? query.OrderByDescending(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDecimal())
                                    : query.OrderBy(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDecimal());
                                break;
                            }
                            else if (property.Type == SchemaPropertyType.DateTime)
                            {
                                query = options.Descending
                                    ? query.OrderByDescending(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDateTime())
                                    : query.OrderBy(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetDateTime());
                            }
                            else if (property.Type == SchemaPropertyType.Text)
                            {
                                query = options.Descending
                                    ? query.OrderByDescending(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetString())
                                    : query.OrderBy(e => e.Data.RootElement.GetProperty(options.SortByPropertyName).GetString());
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"Cannot sort by property '{options.SortByPropertyName}'. It does not exist or is of an unsupported type.");
                        }
                        break;
                }
                    
                // Take relevant entries
                var entries = await query
                    .Skip((cappedPageNumber - 1) * cappedPageSize)
                    .Take(cappedPageSize)
                    .ToListAsync();

                // Construct pagination metadata
                var paginationMetadata = new PaginationMetadata(
                    await dbContext.Entries.CountAsync(e => e.SchemaId == schemaId),
                    cappedPageNumber,
                    cappedPageSize,
                    IEntryService.MaxPageSize
                );
                
                return (entries, paginationMetadata);
            });

            // Map entries to DTOs
            var dtos = entries.Select(e =>
            {
                var dto = e.Adapt<EntryDto>();
                dto.Fields = e.GetFields(schema.Properties);
                dto.Schema = schema.Adapt<SchemaDto>();
                return dto;
            }).ToList();
            
            return Result.Success((dtos, paginationMetadata));
        }
        catch (ArgumentException argEx)
        {
            return Result.Invalid(new ValidationError(argEx.Message));
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
                await dbContext.Entries
                    .AsNoTracking()
                    .Include(e => e.Schema)
                        .ThenInclude(s => s.Properties)
                    .Include(e => e.Schema)
                        .ThenInclude(s => s.Project)
                    .FirstOrDefaultAsync(e => e.Id == entryId));

            if (entry is null)
                return Result.NotFound();
            
            var dto = entry.Adapt<EntryDto>();
            dto.Fields = entry.GetFields(entry.Schema.Properties);
            
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting entry {entryId}.", entryId);
            return Result.Error($"Error getting entry {entryId}.");
        }
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
            var setResult = entry.SetFields(schema.Properties, dto.Fields);

            if (setResult.IsInvalid())
            {
                return Result.Invalid(setResult.ValidationErrors);
            }

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                await dbContext.Entries.AddAsync(entry);
                await dbContext.SaveChangesAsync();
            });

            var adaptedEntry = entry.Adapt<EntryDto>();
            adaptedEntry.Fields = setResult.Value;

            return Result.Success(adaptedEntry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating entry for schema {schemaId}.", dto.SchemaId);
            return Result.Error($"Error creating entry for schema {dto.SchemaId}.");
        }
    }

    public async Task<Result> UpdateEntryAsync(EntryDto dto)
    {
        try
        {
            var entry = await dbHelper.ExecuteAsync(async dbContext =>
                await dbContext.Entries
                    .Include(e => e.Schema)
                    .ThenInclude(s => s.Properties)
                    .FirstOrDefaultAsync(e => e.Id == dto.Id));

            if (entry is null)
                return Result.NotFound();
            
            // Get current fields and update with new values
            var fields = entry.GetFields(entry.Schema.Properties);
            foreach (var field in fields)
            {
                if (dto.Fields.TryGetValue(field.Key, out object? value))
                {
                    fields[field.Key] = value;
                }
            }

            var setResult = entry.SetFields(entry.Schema.Properties, fields);

            if (setResult.IsInvalid())
            {
                return Result.Invalid(setResult.ValidationErrors);
            }

            entry.UpdatedAt = DateTime.UtcNow;

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                await dbContext.SaveChangesAsync();
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating entry {entryId}.", dto.Id);
            return Result.Error($"Error updating entry {dto.Id}.");
        }
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