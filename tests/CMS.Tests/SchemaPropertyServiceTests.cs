using System;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.Result;
using CMS.Main.Abstractions.Properties.PropertyTypes;
using CMS.Main.Data;
using CMS.Main.DTOs;
using CMS.Main.Models;
using CMS.Main.Services;
using Mapster;
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
        var dto = new PropertyDto
        {
            Id = Guid.NewGuid().ToString(),
            SchemaId = Guid.NewGuid().ToString(),
            Name = "Title",
            Type = PropertyType.Text
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

        var dto = new PropertyDto
        {
            Id = Guid.NewGuid().ToString(),
            SchemaId = schema.Id,
            Name = "Title",
            Type = PropertyType.Text
        };
        var result = await propertyService.CreateSchemaPropertyAsync(dto);
        Assert.True(result.IsSuccess);
        Assert.Equal("Title", result.Value.Name);
        Assert.NotNull(await context.Properties.FindAsync(result.Value.Id));
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
        var property = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        await context.Properties.AddAsync(property);
        await context.SaveChangesAsync();

        var result = await propertyService.DeleteSchemaPropertyAsync(property.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(await context.Properties.FindAsync(property.Id));
    }

    [Fact]
    public async Task UpdateSchemaPropertyAsync_NotFound_WhenMissing()
    {
        var updateDto = new PropertyDto
        {
            Id = Guid.NewGuid().ToString(),
            SchemaId = Guid.NewGuid().ToString(),
            Name = "Updated",
            Type = PropertyType.Text
        };
        var result = await propertyService.UpdateSchemaPropertyAsync(updateDto);
        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task UpdateSchemaPropertyAsync_Success_WhenNoChange()
    {
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "Proj", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        var schema = new Schema { Id = Guid.NewGuid().ToString(), Name = "Article", ProjectId = project.Id };
        await context.Schemas.AddAsync(schema);
        var property = new Property { Id = Guid.NewGuid().ToString(), Name = "Title", SchemaId = schema.Id, Type = PropertyType.Text };
        await context.Properties.AddAsync(property);
        await context.SaveChangesAsync();

        var updateDto = new PropertyDto
        {
            Id = property.Id,
            SchemaId = property.SchemaId,
            Name = property.Name,
            Type = property.Type
        };
        var result = await propertyService.UpdateSchemaPropertyAsync(updateDto);
        Assert.True(result.IsSuccess);
        Assert.Equal(property.Name, result.Value.Name);
    }

    [Fact]
    public async Task UpdateSchemaPropertyAsync_Error_WhenExceptionThrown()
    {
        var mockLogger = new Mock<ILogger<SchemaPropertyService>>();
        var brokenDbHelper = new Mock<IDbContextConcurrencyHelper>();
        brokenDbHelper.Setup(h => h.ExecuteAsync<object>(It.IsAny<Func<ApplicationDbContext, Task<object>>>()))
            .ThrowsAsync(new Exception("DB error"));
        brokenDbHelper.Setup(h => h.ExecuteAsync(It.IsAny<Func<ApplicationDbContext, Task<Property>>>() ))
            .ThrowsAsync(new Exception("DB error"));
        brokenDbHelper.Setup(h => h.ExecuteAsync(It.IsAny<Func<ApplicationDbContext, Task>>()))
            .ThrowsAsync(new Exception("DB error"));
        var brokenService = new SchemaPropertyService(brokenDbHelper.Object, mockLogger.Object);
        var updateDto = new PropertyDto
        {
            Id = Guid.NewGuid().ToString(),
            SchemaId = Guid.NewGuid().ToString(),
            Name = "Updated",
            Type = PropertyType.Text
        };
        var result = await brokenService.UpdateSchemaPropertyAsync(updateDto);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains("Error updating schema property", result.Errors.FirstOrDefault());
    }
}
