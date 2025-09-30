using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.DTOs.Entry;
using CMS.Main.DTOs.Schema;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;
using CMS.Main.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CMS.Main.DTOs.Pagination;
using CMS.Main.Abstractions.Entries;

namespace CMS.Tests;

public class EntryServiceTests
{
    private readonly ApplicationDbContext context;
    private readonly EntryService entryService;

    public EntryServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new ApplicationDbContext(options);
        var mockLogger = new Mock<ILogger<EntryService>>();
        var dbHelper = new DbContextConcurrencyHelper(context);
        entryService = new EntryService(dbHelper, mockLogger.Object);
    }

    [Fact]
    public async Task AddEntryAsync_NotFound_WhenSchemaMissing()
    {
        var dto = new EntryDto { SchemaId = Guid.NewGuid().ToString(), Fields = new Dictionary<string, object?>() };

        var result = await entryService.AddEntryAsync(dto);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task AddEntryAsync_CreatesEntry_WhenSchemaExists_AndValidFields()
    {
        // Arrange schema + properties
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = SchemaPropertyType.Text, IsRequired = true };
        await context.SchemaProperties.AddAsync(prop);
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
    public async Task GetEntryByIdAsync_NotFound_WhenMissing()
    {
        var result = await entryService.GetEntryByIdAsync(Guid.NewGuid().ToString());
        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task GetEntryByIdAsync_ReturnsEntry_WithFields()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Published", SchemaId = schema.Id, Type = SchemaPropertyType.Boolean };
        await context.SchemaProperties.AddAsync(prop);
        await context.SaveChangesAsync();

        // create entry directly
        var entry = new Entry { SchemaId = schema.Id };
        entry.SetFields(new List<SchemaProperty> { prop }, new Dictionary<string, object?> { ["Published"] = true });
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
        var prop = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = SchemaPropertyType.Text };
        await context.SchemaProperties.AddAsync(prop);
        await context.SaveChangesAsync();

        // add multiple entries
        for (var i = 0; i < 5; i++)
        {
            var e = new Entry { SchemaId = schema.Id };
            e.SetFields(new List<SchemaProperty> { prop }, new Dictionary<string, object?> { ["Title"] = $"T{i}" });
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
    public async Task GetEntriesForSchema_ClampsPageSize_ToMax()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = SchemaPropertyType.Text };
        await context.SchemaProperties.AddAsync(prop);
        await context.SaveChangesAsync();

        // Add some entries fewer than the requested oversized page size
        for (var i = 0; i < 10; i++)
        {
            var e = new Entry { SchemaId = schema.Id };
            e.SetFields(new List<SchemaProperty> { prop }, new Dictionary<string, object?> { ["Title"] = $"T{i}" });
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
    public async Task AddEntryAsync_Ignores_Unknown_Properties()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = SchemaPropertyType.Text };
        await context.SchemaProperties.AddAsync(prop);
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
        var savedFields = saved.GetFields(new List<SchemaProperty> { prop });
        Assert.False(savedFields.ContainsKey("NotAProperty"));
    }

    [Fact]
    public async Task UpdateEntryAsync_Ignores_Unknown_Properties()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = SchemaPropertyType.Text };
        await context.SchemaProperties.AddAsync(prop);
        await context.SaveChangesAsync();

        var entry = new Entry { SchemaId = schema.Id };
        entry.SetFields(new List<SchemaProperty> { prop }, new Dictionary<string, object?> { ["Title"] = "Initial" });
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
        var fields = updated!.GetFields(new List<SchemaProperty> { prop });
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
        var prop = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = SchemaPropertyType.Text };
        await context.SchemaProperties.AddAsync(prop);
        await context.SaveChangesAsync();

        var entry = new Entry { SchemaId = schema.Id };
        entry.SetFields(new List<SchemaProperty> { prop }, new Dictionary<string, object?> { ["Title"] = "Old" });
        await context.Entries.AddAsync(entry);
        await context.SaveChangesAsync();

        var dto = entry.Adapt<EntryDto>();
        dto.Fields["Title"] = "New";

        var result = await entryService.UpdateEntryAsync(dto);

        Assert.True(result.IsSuccess);

    var updated = await context.Entries.FindAsync(entry.Id);
    Assert.NotNull(updated);
    var fields = updated!.GetFields(new List<SchemaProperty> { prop });
    Assert.Equal("New", fields["Title"]);
    }

    [Fact]
    public async Task DeleteEntryAsync_DeletesEntry_WhenExists()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "P", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = SchemaPropertyType.Text };
        await context.SchemaProperties.AddAsync(prop);
        await context.SaveChangesAsync();

        var entry = new Entry { SchemaId = schema.Id };
        entry.SetFields(new List<SchemaProperty> { prop }, new Dictionary<string, object?> { ["Title"] = "X" });
        await context.Entries.AddAsync(entry);
        await context.SaveChangesAsync();

        var result = await entryService.DeleteEntryAsync(entry.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(await context.Entries.FindAsync(entry.Id));
    }
}
