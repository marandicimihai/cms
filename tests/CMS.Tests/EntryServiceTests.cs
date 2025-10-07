using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.Result;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.Data;
using CMS.Main.Models;
using CMS.Main.Services.SchemaProperties;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CMS.Main.DTOs.Pagination;
using CMS.Main.Abstractions.Entries;
using CMS.Main.Services.Entries;
using CMS.Main.DTOs;
using CMS.Main.Abstractions.Properties.PropertyTypes;

namespace CMS.Tests;

public class EntryServiceTests
{
    private readonly ApplicationDbContext context;
    private readonly EntryService entryService;
    private readonly IPropertyValidator validator;

    public EntryServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new ApplicationDbContext(options);
        var mockLogger = new Mock<ILogger<EntryService>>();
        var dbHelper = new DbContextConcurrencyHelper(context);
        
        // Setup validator for Entry.SetFields calls
        var factory = new PropertyTypeHandlerFactory();
        validator = new PropertyValidator(factory);
        
        entryService = new EntryService(dbHelper, validator, mockLogger.Object);
    }


    [Fact]
    public async Task AddEntryAsync_NotFound_WhenSchemaMissing()
    {
        var dto = new EntryDto { SchemaId = Guid.NewGuid().ToString(), Fields = new Dictionary<string, object?>() };

        var result = await entryService.AddEntryAsync(dto);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task AddEntry_IgnoresFields_NotInSchema()
    {
        // Arrange: create schema and one property
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        // Add entry with one valid and one invalid field
        var dto = new EntryDto
        {
            SchemaId = schema.Id,
            Fields = new Dictionary<string, object?>
            {
                ["Title"] = "Valid",
                ["NotInSchema"] = "ShouldBeIgnored"
            }
        };

        var result = await entryService.AddEntryAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("Valid", result.Value.Fields["Title"]);
        Assert.False(result.Value.Fields.ContainsKey("NotInSchema"));

        // Verify stored entry also does not include the unknown property
        var saved = await context.Entries.FindAsync(result.Value.Id);
        Assert.NotNull(saved);
        var savedFields = saved.GetFields(new List<Property> { prop });
        Assert.False(savedFields.ContainsKey("NotInSchema"));
    }

    [Fact]
    public async Task AddEntryAsync_CreatesEntry_WhenSchemaExists_AndValidFields()
    {
        // Arrange schema + properties
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text, IsRequired = true };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        var dto = new EntryDto { SchemaId = schema.Id, Fields = new Dictionary<string, object?> { ["Title"] = "Hello" } };

        // Act
        var result = await entryService.AddEntryAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello", result.Value.Fields["Title"]);

        var saved = await context.Entries.FindAsync(result.Value.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task AddEntryAsync_Ignores_Unknown_Properties()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        var dto = new EntryDto
        {
            SchemaId = schema.Id,
            Fields = new Dictionary<string, object?>
            {
                ["Title"] = "Allowed",
                ["NotAProperty"] = "ShouldBeIgnored"
            }
        };

        var result = await entryService.AddEntryAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("Allowed", result.Value.Fields["Title"]);
        Assert.False(result.Value.Fields.ContainsKey("NotAProperty"));

        // Verify stored entry also does not include the unknown property
        var saved = await context.Entries.FindAsync(result.Value.Id);
        Assert.NotNull(saved);
        var savedFields = saved.GetFields(new List<Property> { prop });
        Assert.False(savedFields.ContainsKey("NotAProperty"));
    }

    [Fact]
    public async Task GetEntryByIdAsync_NotFound_WhenMissing()
    {
        var result = await entryService.GetEntryByIdAsync(Guid.NewGuid().ToString());
        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task GetEntryByIdAsync_Includes_PropertiesAndProject()
    {
        // Arrange: create project, schema, and properties
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop1 = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        var prop2 = new Property { Id = Guid.NewGuid().ToString(), Name = "Published", SchemaId = schema.Id, Type = PropertyType.Boolean };
        await context.Properties.AddAsync(prop1);
        await context.Properties.AddAsync(prop2);
        await context.SaveChangesAsync();

        // create entry directly
        var entry = new Entry { SchemaId = schema.Id };
        entry.SetFields(new List<Property> { prop1, prop2 }, new Dictionary<string, object?> { ["Title"] = "Hello", ["Published"] = true }, validator);
        await context.Entries.AddAsync(entry);
        await context.SaveChangesAsync();

        // Act
        var result = await entryService.GetEntryByIdAsync(entry.Id);

    // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(entry.Id, result.Value.Id);
        Assert.Equal("Hello", result.Value.Fields["Title"]);
        Assert.True((bool)result.Value.Fields["Published"]!);
        Assert.Equal(schema.Id, result.Value.SchemaId);
        // Check that the schema includes the correct properties
        Assert.NotNull(result.Value.Schema);
        Assert.Equal(schema.Id, result.Value.Schema.Id);
        Assert.Equal(2, result.Value.Schema.Properties.Count);
        Assert.Contains(result.Value.Schema.Properties, p => p.Name == "Title");
        Assert.Contains(result.Value.Schema.Properties, p => p.Name == "Published");
    }

    [Fact]
    public async Task GetEntryByIdAsync_ReturnsEntry_WithFields()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Published", SchemaId = schema.Id, Type = PropertyType.Boolean };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        // create entry directly
        var entry = new Entry { SchemaId = schema.Id };
        entry.SetFields(new List<Property> { prop }, new Dictionary<string, object?> { ["Published"] = true }, validator);
        await context.Entries.AddAsync(entry);
        await context.SaveChangesAsync();

        var result = await entryService.GetEntryByIdAsync(entry.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(entry.Id, result.Value.Id);
        Assert.True((bool)result.Value.Fields["Published"]!);
    }

    [Fact]
    public async Task GetEntriesForSchema_PaginationAndSorting_Works()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        // add multiple entries
        for (var i = 0; i < 5; i++)
        {
            var e = new Entry { SchemaId = schema.Id };
            e.SetFields(new List<Property> { prop }, new Dictionary<string, object?> { ["Title"] = $"T{i}" }, validator);
            // stagger CreatedAt
            e.CreatedAt = DateTime.UtcNow.AddMinutes(i);
            await context.Entries.AddAsync(e);
        }
        await context.SaveChangesAsync();

        var result = await entryService.GetEntriesForSchema(schema.Id, new PaginationParams(1, 2), opt =>
        {
            opt.SortByPropertyName = "CreatedAt";
            opt.Descending = true;
        });

        Assert.True(result.IsSuccess);
        var list = result.Value.Item1;
        var meta = result.Value.Item2;
        Assert.Equal(2, list.Count);
        Assert.Equal(5, meta.TotalCount);
        // first item should have the latest CreatedAt
        Assert.True(list[0].CreatedAt >= list[1].CreatedAt);
    }

    [Fact]
    public async Task GetEntriesForSchema_Includes_PropertiesAndProject()
    {
        // Arrange: create project, schema, and properties
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop1 = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        var prop2 = new Property { Id = Guid.NewGuid().ToString(), Name = "Published", SchemaId = schema.Id, Type = PropertyType.Boolean };
        await context.Properties.AddAsync(prop1);
        await context.Properties.AddAsync(prop2);
        await context.SaveChangesAsync();

        // Add entry
        var entry = new Entry { SchemaId = schema.Id };
        entry.SetFields(new List<Property> { prop1, prop2 }, new Dictionary<string, object?> { ["Title"] = "Hello", ["Published"] = true }, validator);
        await context.Entries.AddAsync(entry);
        await context.SaveChangesAsync();

        // Act
        var result = await entryService.GetEntriesForSchema(schema.Id, new PaginationParams(1, 10));

        // Assert
        Assert.True(result.IsSuccess);
        var entries = result.Value.Item1;
        Assert.Single(entries);
        var dto = entries[0];
        Assert.Equal(entry.Id, dto.Id);
        Assert.Equal("Hello", dto.Fields["Title"]);
        Assert.True((bool)dto.Fields["Published"]!);
        Assert.NotNull(dto.Schema);
        Assert.Equal(schema.Id, dto.Schema.Id);
        Assert.Equal(2, dto.Schema.Properties.Count);
        Assert.Contains(dto.Schema.Properties, p => p.Name == "Title");
        Assert.Contains(dto.Schema.Properties, p => p.Name == "Published");
    }

    [Fact]
    public async Task GetEntriesForSchema_ClampsPageSize_ToMax()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        // Add some entries fewer than the requested oversized page size
        for (var i = 0; i < 10; i++)
        {
            var e = new Entry { SchemaId = schema.Id };
            e.SetFields(new List<Property> { prop }, new Dictionary<string, object?> { ["Title"] = $"T{i}" }, validator);
            await context.Entries.AddAsync(e);
        }
        await context.SaveChangesAsync();

        var oversized = IEntryService.MaxPageSize + 500;
        var result = await entryService.GetEntriesForSchema(schema.Id, new PaginationParams(1, oversized));

        Assert.True(result.IsSuccess);
        var list = result.Value.Item1;
        var meta = result.Value.Item2;
        Assert.True(meta.PageSize <= IEntryService.MaxPageSize);
        Assert.Equal(IEntryService.MaxPageSize, meta.MaxPageSize);
        Assert.True(list.Count <= IEntryService.MaxPageSize);
    }

    [Fact]
    public async Task GetEntriesForSchema_SortsByProperty()
    {
        // Arrange
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        // Add entries with different titles
        var titles = new[] { "C", "A", "B" };
        foreach (var t in titles)
        {
            var e = new Entry { SchemaId = schema.Id };
            e.SetFields(new List<Property> { prop }, new Dictionary<string, object?> { ["Title"] = t }, validator);
            await context.Entries.AddAsync(e);
        }
        await context.SaveChangesAsync();

        // Act: sort ascending
        var resultAsc = await entryService.GetEntriesForSchema(schema.Id, new PaginationParams(1, 10), opt =>
        {
            opt.SortByPropertyName = "Title";
            opt.Descending = false;
        });
    var ascTitles = resultAsc.Value.Item1.Select(e => e.Fields["Title"]?.ToString()).ToList();
    Assert.Equal(new[] { "A", "B", "C" }, ascTitles);

        // Act: sort descending
        var resultDesc = await entryService.GetEntriesForSchema(schema.Id, new PaginationParams(1, 10), opt =>
        {
            opt.SortByPropertyName = "Title";
            opt.Descending = true;
        });
    var descTitles = resultDesc.Value.Item1.Select(e => e.Fields["Title"]?.ToString()).ToList();
    Assert.Equal(new[] { "C", "B", "A" }, descTitles);
    }

    [Fact]
    public async Task GetEntriesForSchema_FiltersByProperty()
    {
        // Arrange
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Published", SchemaId = schema.Id, Type = PropertyType.Boolean };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        // Add entries with different Published values
        var e1 = new Entry { SchemaId = schema.Id };
        e1.SetFields(new List<Property> { prop }, new Dictionary<string, object?> { ["Published"] = true }, validator);
        var e2 = new Entry { SchemaId = schema.Id };
        e2.SetFields(new List<Property> { prop }, new Dictionary<string, object?> { ["Published"] = false }, validator);
        await context.Entries.AddAsync(e1);
        await context.Entries.AddAsync(e2);
        await context.SaveChangesAsync();

        // Act: filter for Published = true
        var result = await entryService.GetEntriesForSchema(schema.Id, new PaginationParams(1, 10), opt =>
        {
            opt.Filters = [
                new() { PropertyName = "Published", FilterType = PropertyFilter.Equals, ReferenceValue = true }
            ];
        });
        var filtered = result.Value.Item1;
        Assert.Single(filtered);
        Assert.True((bool)filtered[0].Fields["Published"]!);
    }

    [Fact]
    public async Task UpdateEntryAsync_Ignores_Unknown_Properties()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        var entry = new Entry { SchemaId = schema.Id };
        entry.SetFields(new List<Property> { prop }, new Dictionary<string, object?> { ["Title"] = "Initial" }, validator);
        await context.Entries.AddAsync(entry);
        await context.SaveChangesAsync();

        var dto = entry.Adapt<EntryDto>();
        // include unknown property on update
        dto.Fields["Title"] = "Updated";
        dto.Fields["Unknown"] = "ShouldBeIgnored";

        var result = await entryService.UpdateEntryAsync(dto);

        Assert.True(result.IsSuccess);

        var updated = await context.Entries.FindAsync(entry.Id);
        Assert.NotNull(updated);
        var fields = updated!.GetFields(new List<Property> { prop });
        Assert.Equal("Updated", fields["Title"]);
        Assert.False(fields.ContainsKey("Unknown"));
    }

    [Fact]
    public async Task UpdateEntryAsync_UpdatesFields_WhenValid()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        var entry = new Entry { SchemaId = schema.Id };
        entry.SetFields(new List<Property> { prop }, new Dictionary<string, object?> { ["Title"] = "Old" }, validator);
        await context.Entries.AddAsync(entry);
        await context.SaveChangesAsync();

        var dto = entry.Adapt<EntryDto>();
        dto.Fields["Title"] = "New";

        var result = await entryService.UpdateEntryAsync(dto);

        Assert.True(result.IsSuccess);

    var updated = await context.Entries.FindAsync(entry.Id);
    Assert.NotNull(updated);
    var fields = updated!.GetFields(new List<Property> { prop });
    Assert.Equal("New", fields["Title"]);
    }

    [Fact]
    public async Task DeleteEntryAsync_DeletesEntry_WhenExists()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        await context.Properties.AddAsync(prop);
        await context.SaveChangesAsync();

        var entry = new Entry { SchemaId = schema.Id };
        entry.SetFields(new List<Property> { prop }, new Dictionary<string, object?> { ["Title"] = "X" }, validator);
        await context.Entries.AddAsync(entry);
        await context.SaveChangesAsync();

        var result = await entryService.DeleteEntryAsync(entry.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(await context.Entries.FindAsync(entry.Id));
    }
}
