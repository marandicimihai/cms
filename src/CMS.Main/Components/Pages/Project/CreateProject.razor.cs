using System.Security.Claims;
using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.DTOs;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Project;

public partial class CreateProject : ComponentBase
{
    private bool isLoading;

    private string? projectUrl;

    [SupplyParameterFromForm]
    private ProjectDto ProjectDto { get; set; } = new();

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        // Get the current user's ID and set it in the DTO
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId)) ProjectDto.OwnerId = userId;
        }
    }

    private async Task HandleValidSubmit()
    {
        isLoading = true;
        StateHasChanged();
        await Task.Yield();

        var result = await ProjectService.CreateProjectAsync(ProjectDto);

        if (!result.IsSuccess)
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    $"There was an error when creating project named {ProjectDto.Name}.",
                Type = NotificationType.Error
            });
            return;
        }

        await Notifications.NotifyAsync(new()
        {
            Message = $"Created project named {result.Value.Name}.",
            Type = NotificationType.Info
        });

        projectUrl = $"/project/{result.Value.Id}";
        isLoading = false;
    }
}