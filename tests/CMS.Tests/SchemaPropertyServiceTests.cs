using System;
using System.Threading.Tasks;
using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.Models;
using CMS.Main.Services;
using CMS.Shared.DTOs.SchemaProperty;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CMS.Tests;

public class SchemaPropertyServiceTests
{
    private readonly ApplicationDbContext context;
    private readonly SchemaPropertyService propertyService;

    public SchemaPropertyServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        context = new ApplicationDbContext(options);
        var dbHelper = new DbContextConcurrencyHelper(context);
        var mockLogger = new Mock<ILogger<SchemaPropertyService>>();
        propertyService = new SchemaPropertyService(dbHelper, mockLogger.Object);
    }

    [Fact]
    public async Task CreateSchemaPropertyAsync_NotFound_WhenSchemaMissing()
    {
        var dto = new SchemaPropertyCreationDto
        {
            SchemaId = Guid.NewGuid().ToString(),
            Name = "Title",
            Type = SchemaPropertyType.Text
        };

        var result = await propertyService.CreateSchemaPropertyAsync(dto);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task CreateSchemaPropertyAsync_Success_WhenSchemaExists()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "Proj", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        await context.SaveChangesAsync();

        var dto = new SchemaPropertyCreationDto
        {
            SchemaId = schema.Id,
            Name = "Title",
            Type = SchemaPropertyType.Text
        };

        var result = await propertyService.CreateSchemaPropertyAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("Title", result.Value.Name);
        Assert.NotNull(await context.SchemaProperties.FindAsync(result.Value.Id));
    }

    [Fact]
    public async Task DeleteSchemaPropertyAsync_NotFound_WhenMissing()
    {
        var result = await propertyService.DeleteSchemaPropertyAsync(Guid.NewGuid().ToString());
        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task DeleteSchemaPropertyAsync_Success_WhenExists()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "Proj", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var property = new SchemaProperty { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = SchemaPropertyType.Text };
        await context.SchemaProperties.AddAsync(property);
        await context.SaveChangesAsync();

        var result = await propertyService.DeleteSchemaPropertyAsync(property.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(await context.SchemaProperties.FindAsync(property.Id));
    }
}

