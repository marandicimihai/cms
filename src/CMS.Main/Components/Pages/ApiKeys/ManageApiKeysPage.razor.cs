using CMS.Main.Abstractions;
using CMS.Main.DTOs.ApiKey;
using CMS.Main.DTOs.Project;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;
using Ardalis.Result;
using CMS.Main.Components.Shared;

namespace CMS.Main.Components.Pages.ApiKeys;

public partial class ManageApiKeysPage : ComponentBase
{
    [Parameter]
    public Guid ProjectId { get; set; }

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    [Inject]
    private IApiKeyService ApiKeyService { get; set; } = default!;
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    private ProjectDto ProjectDto { get; set; } = new();
    
    private StatusIndicator? statusIndicator;
    private ApiKeyCreateForm? createForm;
    
    // queued status for showing after render when initialized
    private string? queuedStatusMessage;
    private StatusIndicator.StatusSeverity? queuedStatusSeverity;
    private bool createFormVisible;
    private bool hasAccess;

    protected override async Task OnInitializedAsync()
    {
        if (!await AuthHelper.CanEditProject(ProjectId.ToString()))
        {
            queuedStatusMessage = "You do not have access to this resource or it does not exist.";
            queuedStatusSeverity = StatusIndicator.StatusSeverity.Error;
            return;
        }

        hasAccess = true;

        var result =
            await ProjectService.GetProjectByIdAsync(ProjectId.ToString(), opt => { opt.IncludeApiKeys = true; });

        if (result.IsSuccess)
        {
            ProjectDto = result.Value;
        }
        else
        {
            queuedStatusMessage = result.Errors.FirstOrDefault() ?? "There was an error";
            queuedStatusSeverity = StatusIndicator.StatusSeverity.Error;
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (queuedStatusMessage != null && queuedStatusSeverity != null)
        {
            statusIndicator?.Show(queuedStatusMessage, queuedStatusSeverity.Value);
            queuedStatusMessage = null;
            queuedStatusSeverity = null;
        }
    }

    private void ShowCreateForm()
    {
        createForm?.ResetForm();
        createFormVisible = true;
    }
    
    private void HideCreateForm() => createFormVisible = false;
}