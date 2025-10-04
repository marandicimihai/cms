using CMS.Main.Abstractions;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Project;

public partial class ProjectSchemasSection : ComponentBase
{
    [Parameter]
    public List<SchemaDto> Schemas { get; set; } = [];

    [Parameter]
    public string ProjectId { get; set; } = string.Empty;

    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;

    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;
    
    private StatusIndicator? statusIndicator;

    [SupplyParameterFromForm]
    public SchemaDto NewSchema { get; set; } = new();
    private bool IsAddFormVisible { get; set; }

    private bool IsCreatingSchema { get; set; }

    protected override void OnParametersSet()
    {
        NewSchema = new SchemaDto { ProjectId = ProjectId };
    }

    private void ShowAddForm()
    {
        IsAddFormVisible = true;
        NewSchema = new SchemaDto { ProjectId = ProjectId };
    }

    private void HideAddForm()
    {
        statusIndicator?.Hide();
        IsAddFormVisible = false;
        NewSchema = new SchemaDto { ProjectId = ProjectId };
    }

    public async Task HandleAddSchema()
    {
        if (!await AuthHelper.CanEditProject(ProjectId))
        {
            statusIndicator?.Show("You do not have access to this project or it does not exist.",
                StatusIndicator.StatusSeverity.Error);
            return;
        }

        IsCreatingSchema = true;
        StateHasChanged();
        await Task.Yield();

        var result = await SchemaService.CreateSchemaAsync(NewSchema);

        if (!result.IsSuccess)
        {
            statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error",
                StatusIndicator.StatusSeverity.Error);
        }
        else
        {
            statusIndicator?.Show("Successfully created schema.",
                StatusIndicator.StatusSeverity.Success);

            IsAddFormVisible = false;
            NewSchema = new SchemaDto { ProjectId = ProjectId };
            Schemas.Add(result.Value);
        }

        IsCreatingSchema = false;
        StateHasChanged();
    }

    private async Task OnDeleteSchemaAsync(SchemaDto schema)
    {
        if (!await AuthHelper.CanEditProject(ProjectId))
        {
            statusIndicator?.Show("You do not have access to this project or it does not exist.",
                StatusIndicator.StatusSeverity.Error);
            return;
        }

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
                statusIndicator?.Show("Schema deleted successfully.",
                    StatusIndicator.StatusSeverity.Success);
                Schemas.Remove(schema);
            }
            else
            {
                statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error",
                    StatusIndicator.StatusSeverity.Error);
            }
        }
    }
}