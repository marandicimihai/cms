using CMS.Main.Client.Components;
using CMS.Main.Components.Shared;
using CMS.Main.Services.State;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Entry;
using CMS.Shared.DTOs.Schema;
using CMS.Shared.DTOs.SchemaProperty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CMS.Main.Components.Pages.Entries;

public partial class EntriesPage : ComponentBase
{
    [Parameter]
    public Guid ProjectId { get; set; }
    
    [Parameter]
    public Guid SchemaId { get; set; }
    
    private SchemaWithIdDto Schema { get; set; } = new();
    
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    
    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;
    
    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;
    
    [Inject]
    private IEntryService EntryService { get; set; } = default!;

    [Inject]
    private EntryStateService EntryStateService { get; set; } = default!;

    private DynamicEntryForm? entryForm;
    
    private StatusIndicator? statusIndicator;
    private string? statusText;
    private bool pendingStatusError;

    private bool showForm;

    protected override async Task OnInitializedAsync()
    {
        if (!await HasAccess())
            return;

        var result = await SchemaService.GetSchemaByIdAsync(SchemaId.ToString(), opt =>
        {
            opt.IncludeProperties = true;
        });

        if (result.IsSuccess)
        {
            Schema = result.Value;
        }
        else
        {
            statusText = result.Errors.FirstOrDefault() ?? "There was an error";
            pendingStatusError = true;
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (pendingStatusError)
        {
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
            pendingStatusError = false;
            StateHasChanged();
        }
    }
    
    private async Task<bool> HasAccess()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var authorizationResult =
            await AuthorizationService.AuthorizeAsync(user, ProjectId.ToString(), "ProjectPolicies.CanEditProject");

        if (authorizationResult.Succeeded) return true;

        statusText = "Could not load project. You do not have permission to access this project.";
        statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
        pendingStatusError = true;

        return false;
    }

    private async Task OnEntrySubmit(Dictionary<SchemaPropertyWithIdDto, object?> entry)
    {
        if (!await HasAccess())
            return;

        var creationDto = new EntryCreationDto
        {
            SchemaId = SchemaId.ToString(),
            Properties = entry
        };

        var result = await EntryService.AddEntryAsync(creationDto);

        if (result.IsSuccess)
        {
            EntryStateService.NotifyCreated([result.Value]);
            
            statusText = "Entry created successfully.";
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Success);
            showForm = false;
            
            entryForm?.Reset();
            StateHasChanged();
        }
        else
        {
            statusText = result.Errors.FirstOrDefault() ?? "There was an error";
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
        }
    }

    private void ToggleAddForm()
    {
        showForm = !showForm;
    }
}