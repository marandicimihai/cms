using CMS.Main.Abstractions;
using Microsoft.AspNetCore.Components;
using CMS.Main.Services.State;
using Mapster;
using CMS.Main.DTOs;
using CMS.Main.Abstractions.Notifications;

namespace CMS.Main.Components.Pages.ApiKeys;

public partial class ApiKeyCreateForm : ComponentBase
{
    [Parameter, EditorRequired]
    public string ProjectId { get; set; } = default!;

    [Parameter]
    public EventCallback OnSuccess { get; set; }
    
    [Parameter]
    public EventCallback OnCancel { get; set; }
    
    [Parameter]
    public bool Visible { get; set; } = true;

    [Inject]
    private IApiKeyService ApiKeyService { get; set; } = default!;

    [Inject]
    private ApiKeyStateService ApiKeyStateService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    [SupplyParameterFromForm]
    private ApiKeyDto ApiKeyDto { get; set; } = new();
    private string? rawKey;

    protected override void OnInitialized()
    {
        ResetForm();
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
        // TODO: Add auth here

        var toCreate = ApiKeyDto.Adapt<ApiKeyDto>();
        var result = await ApiKeyService.CreateApiKeyAsync(toCreate);
        if (result.IsSuccess)
        {
            rawKey = result.Value.Item1;
            
            ApiKeyStateService.NotifyCreated(result.Value.Item2);
            await Notifications.NotifyAsync(new()
            {
                Message = $"Created API key named {result.Value.Item2.Name}.",
                Type = NotificationType.Info
            });
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
