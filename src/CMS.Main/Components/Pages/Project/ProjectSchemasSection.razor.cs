using CMS.Main.Client.Components;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Schema;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Project;

public partial class ProjectSchemasSection : ComponentBase
{
    [Parameter]
    public List<SchemaWithIdDto> Schemas { get; set; } = [];

    [Parameter]
    public string ProjectId { get; set; } = string.Empty;
    
    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;

    public SchemaCreationDto NewSchema { get; set; } = new();
    private bool IsAddFormVisible { get; set; }

    private StatusIndicator? statusIndicator;
    private string? statusText;

    public bool IsCreatingSchema { get; set; } = false;

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
        
        IsAddFormVisible = false;
        NewSchema = new SchemaCreationDto { ProjectId = ProjectId };
        
        IsCreatingSchema = false;
        StateHasChanged();
    }
}
