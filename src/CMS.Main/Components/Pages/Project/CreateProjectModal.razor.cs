using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;

namespace CMS.Main.Components.Pages.Project;

public partial class CreateProjectModal : ComponentBase, IDisposable
{
    private string? projectUrl;
    private bool _isOpen;

    [SupplyParameterFromForm]
    private ProjectCreateDto CreateDto { get; set; } = new();

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen != value)
            {
                _isOpen = value;
                if (_isOpen)
                {
                    projectUrl = null;
                    CreateDto = new ProjectCreateDto();
                }
                StateHasChanged();
            }
        }
    }

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += OnLocationChanged;
        CheckQueryParameters();
    }

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        CheckQueryParameters();
    }

    private void CheckQueryParameters()
    {
        var uri = new Uri(Navigation.Uri);
        var queryParams = QueryHelpers.ParseQuery(uri.Query);
        
        IsOpen = queryParams.TryGetValue("new", out var value) && value == "true";
        
        // Reset state when modal is opened
        if (IsOpen && projectUrl != null)
        {
            projectUrl = null;
            CreateDto = new ProjectCreateDto();
        }
    }

    private void CloseModal()
    {
        // Remove the query parameter from URL
        var uri = new Uri(Navigation.Uri);
        var pathWithoutQuery = uri.GetLeftPart(UriPartial.Path);
        Navigation.NavigateTo(pathWithoutQuery, false);
    }

    private void NavigateToProject()
    {
        if (projectUrl != null)
        {
            Navigation.NavigateTo(projectUrl);
        }
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (!IsOpen) return;
        if (e.Key == "Escape")
        {
            CloseModal();
        }
    }

    private async Task HandleValidSubmit()
    {
        // Get the current user's ID and set it in the DTO
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var projectDto = new ProjectDto
        {
            Name = CreateDto.Name
        };

        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId)) projectDto.OwnerId = userId;
        }
        else
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "You must be logged in to create a project.",
                Type = NotificationType.Error
            });
            return;
        }

        var result = await ProjectService.CreateProjectAsync(projectDto);

        if (!result.IsSuccess)
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    $"There was an error when creating project named {projectDto.Name}.",
                Type = NotificationType.Error
            });
            return;
        }

        await Notifications.NotifyAsync(new()
        {
            Message = $"Created project named {result.Value.Name}.",
            Type = NotificationType.Success
        });

        projectUrl = $"/project/{result.Value.Id}";
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
    }

    private class ProjectCreateDto()
    {
        [Required(ErrorMessage = "Project name is required.")]
        [Length(3, 50, ErrorMessage = "Project name must be between 3 and 50 characters long.")]
        public string Name { get; set; } = default!;
    }
}