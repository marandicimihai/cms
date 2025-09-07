using CMS.Main.Components.Shared;
using CMS.Main.DTOs.ApiKey;
using CMS.Main.Services.State;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.ApiKeys;

public partial class ApiKeyViewingTable : ComponentBase, IDisposable
{
    [Parameter]
    public List<ApiKeyDto> ApiKeys { get; set; } = [];

    [Parameter]
    public EventCallback<ApiKeyDto> OnEdit { get; set; }
    
    [Parameter]
    public EventCallback<ApiKeyDto> OnDelete { get; set; }

    [Inject]
    private ApiKeyStateService ApiKeyStateService { get; set; } = default!;

    protected override void OnInitialized()
    {
        ApiKeyStateService.ApiKeyCreated += OnApiKeyCreated;
    }

    private void OnApiKeyCreated(ApiKeyDto newKey)
    {
        ApiKeys.Add(newKey);
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ApiKeyStateService.ApiKeyCreated -= OnApiKeyCreated;
    }

    private async Task OnEditKey(ApiKeyDto key)
    {
        if (OnEdit.HasDelegate)
            await OnEdit.InvokeAsync(key);
    }

    private async Task OnDeleteKey(ApiKeyDto key)
    {
        if (OnDelete.HasDelegate)
            await OnDelete.InvokeAsync(key);
    }
}