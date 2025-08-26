using System;
using System.Threading.Tasks;
using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.DTOs.Schema;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Models;
using CMS.Main.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CMS.Tests;

public class SchemaServiceTests
{
    private readonly ApplicationDbContext context;
    private readonly SchemaService schemaService;

    public SchemaServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        context = new ApplicationDbContext(options);
        var mockLogger = new Mock<ILogger<SchemaService>>();
        var dbHelper = new DbContextConcurrencyHelper(context);
        schemaService = new SchemaService(dbHelper, mockLogger.Object);
    }

    [Fact]
    public async Task GetSchemaByIdAsync_NotFound_WhenMissing()
    {
        var result = await schemaService.GetSchemaByIdAsync(Guid.NewGuid().ToString());
        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task GetSchemaByIdAsync_ReturnsSchema_WithoutProperties_WhenIncludeFalse()
    {
        // Arrange
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "Proj", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Content", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = SchemaPropertyType.Text };
        await context.SchemaProperties.AddAsync(prop);
        await context.SaveChangesAsync();

        // Act - default include false
        var result = await schemaService.GetSchemaByIdAsync(schema.Id);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Value;
        Assert.Equal(schema.Id, dto.Id);
        Assert.Equal(schema.Name, dto.Name);
        Assert.Equal(schema.ProjectId, dto.ProjectId);
        Assert.NotNull(dto.Project);
        Assert.Empty(dto.Properties); // Should be empty because IncludeProperties default is false
    }

    [Fact]
    public async Task GetSchemaByIdAsync_ReturnsSchema_WithProperties_WhenIncludeTrue()
    {
        // Arrange
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "Proj", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Content", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var prop1 = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = SchemaPropertyType.Text };
        var prop2 = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "PublishedOn", SchemaId = schema.Id, Type = SchemaPropertyType.DateTime };
        await context.SchemaProperties.AddRangeAsync(prop1, prop2);
        await context.SaveChangesAsync();

        // Act
        var result = await schemaService.GetSchemaByIdAsync(schema.Id, opt => opt.IncludeProperties = true);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Value;
        Assert.Equal(schema.Id, dto.Id);
        Assert.Equal(schema.Name, dto.Name);
        Assert.Equal(schema.ProjectId, dto.ProjectId);
        Assert.NotNull(dto.Project);
        Assert.Equal(2, dto.Properties.Count);
        Assert.Contains(dto.Properties, p => p.Name == "Title");
        Assert.Contains(dto.Properties, p => p.Name == "PublishedOn");
    }

    [Fact]
    public async Task CreateSchemaAsync_NotFound_WhenProjectMissing()
    {
        var dto = new SchemaDto { Name = "Article", ProjectId = Guid.NewGuid().ToString() };
        var result = await schemaService.CreateSchemaAsync(dto);
        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task CreateSchemaAsync_Success_WhenProjectExists()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "Proj", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        var dto = new SchemaDto { Name = "Article", ProjectId = project.Id };
        var result = await schemaService.CreateSchemaAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("Article", result.Value.Name);
        Assert.NotNull(await context.Schemas.FindAsync(result.Value.Id));
    }

    [Fact]
    public async Task DeleteSchemaAsync_NotFound_WhenMissing()
    {
        var result = await schemaService.DeleteSchemaAsync(Guid.NewGuid().ToString());
        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task DeleteSchemaAsync_Success_WhenExists()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "Proj", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Content", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        await context.SaveChangesAsync();

        var result = await schemaService.DeleteSchemaAsync(schema.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(await context.Schemas.FindAsync(schema.Id));
    }
}
