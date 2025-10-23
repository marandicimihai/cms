using CMS.Main.Abstractions;
using Microsoft.AspNetCore.Components;
using CMS.Main.Services.State;
using Mapster;
using CMS.Main.DTOs;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components.Web;

namespace CMS.Main.Components.Pages.ApiKeys;

public partial class ApiKeyCreateForm : ComponentBase
{
    private bool _isOpen;

    [Parameter, EditorRequired]
    public string ProjectId { get; set; } = default!;

    [Parameter]
    public EventCallback OnSuccess { get; set; }

    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;

    [Inject]
    private IApiKeyService ApiKeyService { get; set; } = default!;

    [Inject]
    private ApiKeyStateService ApiKeyStateService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    [SupplyParameterFromForm]
    private ApiKeyDto ApiKeyDto { get; set; } = new();
    private string? rawKey;

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
                    ResetForm();
                }
                StateHasChanged();
            }
        }
    }

    public void Open()
    {
        IsOpen = true;
    }

    public void CloseModal()
    {
        IsOpen = false;
        rawKey = null;
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (!IsOpen) return;
        if (e.Key == "Escape")
        {
            CloseModal();
        }
    }

    public void ResetForm()
    {
        ApiKeyDto = new ApiKeyDto
        {
            ProjectId = ProjectId,
            Name = string.Empty,
            IsActive = true
        };
        rawKey = null;
    }

    private async Task HandleCreateApiKeySubmit()
    {
        if (!await AuthHelper.OwnsProject(ProjectId))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        var toCreate = ApiKeyDto.Adapt<ApiKeyDto>();
        var result = await ApiKeyService.CreateApiKeyAsync(toCreate);
        if (result.IsSuccess)
        {
            rawKey = result.Value.Item1;
            
            ApiKeyStateService.NotifyCreated(result.Value.Item2);
            await OnSuccess.InvokeAsync();
        }
        else
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    $"There was an error when creating API key named {result.Value.Item2.Name}.",
                Type = NotificationType.Error
            });
        }
        StateHasChanged();
    }
}
