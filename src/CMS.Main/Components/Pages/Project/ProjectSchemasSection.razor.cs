using CMS.Main.Client.Components;
using CMS.Main.Services;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CMS.Main.Components.Pages.Project;

public partial class ProjectSchemasSection : ComponentBase
{
    private StatusIndicator? statusIndicator;
    private string? statusText;

    [Parameter]
    public List<SchemaWithIdDto> Schemas { get; set; } = [];

    [Parameter]
    public string ProjectId { get; set; } = string.Empty;

    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    [SupplyParameterFromForm]
    public SchemaCreationDto NewSchema { get; set; } = new();
    private bool IsAddFormVisible { get; set; }

    private bool IsCreatingSchema { get; set; }

    protected override void OnParametersSet()
    {
        NewSchema = new SchemaCreationDto { ProjectId = ProjectId };
    }

    private void ShowAddForm()
    {
        IsAddFormVisible = true;
        NewSchema = new SchemaCreationDto { ProjectId = ProjectId };
    }

    private void HideAddForm()
    {
        statusIndicator?.Hide();
        IsAddFormVisible = false;
        NewSchema = new SchemaCreationDto { ProjectId = ProjectId };
    }

    public async Task HandleAddSchema()
    {
        if (!await HasAccess())
            return;

        IsCreatingSchema = true;
        StateHasChanged();
        await Task.Yield();

        var result = await SchemaService.CreateSchemaAsync(NewSchema);

        if (!result.IsSuccess)
        {
            statusText = result.Errors.First();
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
            IsCreatingSchema = false;
            StateHasChanged();
            return;
        }

        statusText = "Successfully created schema.";
        statusIndicator?.Show(StatusIndicator.StatusSeverity.Success);
        IsAddFormVisible = false;
        NewSchema = new SchemaCreationDto { ProjectId = ProjectId };
        Schemas.Add(result.Value);

        IsCreatingSchema = false;
        StateHasChanged();
    }

    private async Task OnDeleteSchemaAsync(SchemaWithIdDto schema)
    {
        if (!await HasAccess())
            return;

        var confirmed = await ConfirmationService.ShowAsync(
            "Delete Schema",
            "Are you sure you want to delete this schema? This action cannot be undone.",
            "Delete"
        );

        if (confirmed)
        {
            var result = await SchemaService.DeleteSchemaAsync(schema.Id);

            if (result.IsSuccess)
            {
                statusText = "Schema deleted successfully.";
                statusIndicator?.Show(StatusIndicator.StatusSeverity.Success);
                Schemas.Remove(schema);
            }
            else
            {
                statusText = result.Errors.First();
                statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
            }
        }
    }

    private async Task<bool> HasAccess()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var authorizationResult = await AuthorizationService.AuthorizeAsync(user, ProjectId, "ProjectPolicies.CanEditProject");

        if (authorizationResult.Succeeded) return true;

        statusText = "Could not load project. You do not have permission to access this project.";
        statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);

        return false;
    }
}