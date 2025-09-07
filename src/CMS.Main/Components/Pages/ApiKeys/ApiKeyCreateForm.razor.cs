using CMS.Main.Abstractions;
using CMS.Main.DTOs.ApiKey;
using CMS.Main.Components.Shared;
using Microsoft.AspNetCore.Components;
using CMS.Main.Services.State;

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
    public StatusIndicator? StatusIndicator { get; set; }
    
    [Parameter]
    public bool Visible { get; set; } = true;

    [Inject]
    private IApiKeyService ApiKeyService { get; set; } = default!;

    [Inject]
    private ApiKeyStateService ApiKeyStateService { get; set; } = default!;

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
        ApiKeyDto.ProjectId = ProjectId;
        var result = await ApiKeyService.CreateApiKeyAsync(ApiKeyDto);
        if (result.IsSuccess)
        {
            rawKey = result.Value.Item1;
            StatusIndicator?.Show("Successfully created API key.", StatusIndicator.StatusSeverity.Success);
            ApiKeyStateService.NotifyCreated(result.Value.Item2);
            await OnSuccess.InvokeAsync();
        }
        else
        {
            StatusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error", StatusIndicator.StatusSeverity.Error);
        }
        StateHasChanged();
    }
}
