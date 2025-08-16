using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.Result;
using CMS.Main.Data;
using CMS.Main.Models;
using CMS.Main.Services;
using CMS.Shared.DTOs.Pagination;
using CMS.Shared.DTOs.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CMS.Tests;

public class ProjectServiceTests
{
    private readonly ApplicationDbContext context;
    private readonly ProjectService projectService;

    public ProjectServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        context = new ApplicationDbContext(options);
        var mockLogger = new Mock<ILogger<ProjectService>>();
        var dbHelper = new DbContextConcurrencyHelper(context);
        projectService = new ProjectService(dbHelper, mockLogger.Object);
    }

    [Fact]
    public async Task GetProjectsForUserAsync_WithValidUserId_ReturnsProjectsWithPagination()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var projects = new List<Project>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "Project 1", OwnerId = userId },
            new() { Id = Guid.NewGuid().ToString(), Name = "Project 2", OwnerId = userId },
            new() { Id = Guid.NewGuid().ToString(), Name = "Project 3", OwnerId = userId }
        };

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        var paginationParams = new PaginationParams(1, 2);

        // Act
        var result = await projectService.GetProjectsForUserAsync(userId, paginationParams);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Item1.Count);
        Assert.Equal(3, result.Value.Item2.TotalCount);
        Assert.Equal(1, result.Value.Item2.CurrentPage);
        Assert.Equal(2, result.Value.Item2.PageSize);
    }

    [Fact]
    public async Task GetProjectsForUserAsync_WithNoProjects_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = await projectService.GetProjectsForUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Item1);
        Assert.Equal(0, result.Value.Item2.TotalCount);
    }

    [Fact]
    public async Task GetProjectsForUserAsync_WithNullPaginationParams_UsesDefaults()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var projects = new List<Project>();
        for (var i = 0; i < 15; i++)
        {
            projects.Add(new Project { Id = Guid.NewGuid().ToString(), Name = $"Project {i + 1}", OwnerId = userId });
        }

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        // Act
        var result = await projectService.GetProjectsForUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Item1.Count); // Default page size
        Assert.Equal(15, result.Value.Item2.TotalCount);
        Assert.Equal(1, result.Value.Item2.CurrentPage);
    }

    [Fact]
    public async Task GetProjectsForUserAsync_WithInvalidPageSize_ClampsToValidRange()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "Test Project", OwnerId = userId };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        var paginationParams = new PaginationParams(1, 200); // Exceeds max page size

        // Act
        var result = await projectService.GetProjectsForUserAsync(userId, paginationParams);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Item2.PageSize <= 50); // Should be clamped to max
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithValidId_ReturnsProject()
    {
        // Arrange
        var projectId = Guid.NewGuid().ToString();
        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await projectService.GetProjectByIdAsync(projectId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(projectId, result.Value.Id);
        Assert.Equal("Test Project", result.Value.Name);
        Assert.Equal(project.OwnerId, result.Value.OwnerId);
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await projectService.GetProjectByIdAsync(nonExistentId);

        // Assert
        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task CreateProjectAsync_WithValidDto_CreatesProject()
    {
        // Arrange
        var ownerId = Guid.NewGuid().ToString();
        var projectDto = new ProjectCreationDto
        {
            Name = "New Project",
            OwnerId = ownerId
        };

        // Act
        var result = await projectService.CreateProjectAsync(projectDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Project", result.Value.Name);
        Assert.Equal(ownerId, result.Value.OwnerId);
        Assert.NotNull(result.Value.Id);

        // Verify project was saved to database
        var savedProject = await context.Projects.FindAsync(result.Value.Id);
        Assert.NotNull(savedProject);
        Assert.Equal("New Project", savedProject.Name);
    }

    [Fact]
    public async Task CreateProjectAsync_WithInvalidOwnerId_ReturnsInvalid()
    {
        // Arrange
        var projectDto = new ProjectCreationDto
        {
            Name = "New Project",
            OwnerId = "invalid-guid"
        };

        // Act
        var result = await projectService.CreateProjectAsync(projectDto);

        // Assert
        Assert.True(result.IsInvalid());
        Assert.Contains("OwnerID must be a valid GUID.", result.ValidationErrors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public async Task UpdateProjectAsync_WithValidDto_UpdatesProject()
    {
        // Arrange
        var projectId = Guid.NewGuid().ToString();
        var ownerId = Guid.NewGuid().ToString();
        var project = new Project { Id = projectId, Name = "Original Name", OwnerId = ownerId };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        var updateDto = new ProjectUpdateDto
        {
            Id = projectId,
            Name = "Updated Name",
            OwnerId = ownerId
        };

        // Act
        var result = await projectService.UpdateProjectAsync(updateDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Name", result.Value.Name);

        // Verify project was updated in database
        var updatedProject = await context.Projects.FindAsync(projectId);
        Assert.NotNull(updatedProject);
        Assert.Equal("Updated Name", updatedProject.Name);
    }

    [Fact]
    public async Task UpdateProjectAsync_WithNonExistentProject_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var updateDto = new ProjectUpdateDto
        {
            Id = nonExistentId,
            Name = "Updated Name",
            OwnerId = Guid.NewGuid().ToString()
        };

        // Act
        var result = await projectService.UpdateProjectAsync(updateDto);

        // Assert
        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task DeleteProjectAsync_WithValidId_DeletesProject()
    {
        // Arrange
        var projectId = Guid.NewGuid().ToString();
        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = Guid.NewGuid().ToString() };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await projectService.DeleteProjectAsync(projectId);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify project was deleted from database
        var deletedProject = await context.Projects.FindAsync(projectId);
        Assert.Null(deletedProject);
    }

    [Fact]
    public async Task DeleteProjectAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await projectService.DeleteProjectAsync(nonExistentId);

        // Assert
        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task OwnsProject_WhenUserOwnsProject_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var projectId = Guid.NewGuid().ToString();
        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = userId };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await projectService.OwnsProject(userId, projectId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task OwnsProject_WhenUserDoesNotOwnProject_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var differentUserId = Guid.NewGuid().ToString();
        var projectId = Guid.NewGuid().ToString();
        var project = new Project { Id = projectId, Name = "Test Project", OwnerId = differentUserId };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        // Act
        var result = await projectService.OwnsProject(userId, projectId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task OwnsProject_WithNonExistentProject_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var nonExistentProjectId = Guid.NewGuid().ToString();

        // Act
        var result = await projectService.OwnsProject(userId, nonExistentProjectId);

        // Assert
        Assert.True(result.IsNotFound());
    }

    [Theory]
    [InlineData(0, 1)] // Page number too low
    [InlineData(-1, 1)] // Negative page number
    [InlineData(1, 0)] // Page size too low
    [InlineData(1, -1)] // Negative page size
    public async Task GetProjectsForUserAsync_WithInvalidPaginationParams_ClampsToValidValues(int pageNumber, int pageSize)
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var project = new Project { Id = Guid.NewGuid().ToString(), Name = "Test Project", OwnerId = userId };
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        var paginationParams = new PaginationParams(pageNumber, pageSize);

        // Act
        var result = await projectService.GetProjectsForUserAsync(userId, paginationParams);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Item2.CurrentPage >= 1);
        Assert.True(result.Value.Item2.PageSize >= 1);
    }

    [Fact]
    public async Task GetProjectsForUserAsync_OnlyReturnsProjectsForSpecificUser()
    {
        // Arrange
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        
        var projects = new List<Project>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "User1 Project 1", OwnerId = userId1 },
            new() { Id = Guid.NewGuid().ToString(), Name = "User1 Project 2", OwnerId = userId1 },
            new() { Id = Guid.NewGuid().ToString(), Name = "User2 Project", OwnerId = userId2 }
        };

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        // Act
        var result = await projectService.GetProjectsForUserAsync(userId1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Item1.Count);
        Assert.All(result.Value.Item1, p => Assert.Equal(userId1, p.OwnerId));
    }
}
