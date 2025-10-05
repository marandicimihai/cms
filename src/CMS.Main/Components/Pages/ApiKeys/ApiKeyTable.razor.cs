using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.DTOs;
using CMS.Main.Services;
using CMS.Main.Services.State;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.ApiKeys;

public partial class ApiKeyTable : ComponentBase, IDisposable
{
    [Parameter]
    public List<ApiKeyDto> ApiKeys { get; set; } = [];

    [Parameter]
    public EventCallback<ApiKeyDto> OnEdit { get; set; }
    
    [Inject]
    private ApiKeyStateService ApiKeyStateService { get; set; } = default!;
    
    [Inject]
    private IApiKeyService ApiKeyService { get; set; } = default!;
    
    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    protected override void OnInitialized()
    {
        ApiKeyStateService.ApiKeyCreated += OnApiKeyCreated;
        ApiKeys = ApiKeys.OrderByDescending(k => k.CreatedAt).ToList();
    }

    private void OnApiKeyCreated(ApiKeyDto newKey)
    {
        ApiKeys.Insert(0, newKey);
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ApiKeyStateService.ApiKeyCreated -= OnApiKeyCreated;
    }

    private async Task OnDeleteKey(ApiKeyDto key)
    {
        // TODO: Add auth here

        var confirmed = await ConfirmationService.ShowAsync(
            "Delete API Key",
            "Are you sure you want to delete this API key? This action cannot be undone.",
            "Delete");

        if (!confirmed)
            return;
        
        var result = await ApiKeyService.DeleteApiKeyAsync(key.Id);
        
        if (result.IsSuccess)
        {
            ApiKeys.Remove(key);
            StateHasChanged();
        }
        else
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    $"There was an error when deleting API key named {key.Name}.",
                Type = NotificationType.Error
            });
        }
    }

    private async Task OnToggleKeyState(ApiKeyDto key, object? newValue)
    {
        if (newValue is bool isActive)
        {
            var toUpdate = key.Adapt<ApiKeyDto>();
            toUpdate.IsActive = isActive;
            
            var result = await ApiKeyService.UpdateApiKeyAsync(toUpdate);
            if (result.IsSuccess)
            {
                key.IsActive = isActive;
            }
            else
            {
                await Notifications.NotifyAsync(new()
                {
                    Message = result.Errors.FirstOrDefault() ??
                        $"There was an error when updating the state of API key named {key.Name}.",
                    Type = NotificationType.Error
                });
            }
        }

        await Task.CompletedTask;
    }
}