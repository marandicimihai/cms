using CMS.Main.Client.Components;
using CMS.Main.Client.Services;
using CMS.Main.Client.Services.State;
using CMS.Main.Services;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Schema;
using CMS.Shared.DTOs.SchemaProperty;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Schemas;

public partial class SchemaPage : ComponentBase
{
    [Parameter]
    public Guid ProjectId { get; set; }
    
    [Parameter]
    public Guid SchemaId { get; set; }

    private SchemaWithIdDto Schema { get; set; } = new();
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;
    
    [Inject]
    private ISchemaPropertyService PropertyService { get; set; } = default!;
    
    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    private StatusIndicator? statusIndicator;

    private bool isCreatePropertyFormVisible;
    private SchemaPropertyCreationDto NewProperty { get; set; } = new();
    private string EnumOptions { get; set; } = string.Empty;
    private SchemaPropertyType[] PropertyTypes { get; } = Enum.GetValues<SchemaPropertyType>();

    private string? queuedStatusMessage;
    private StatusIndicator.StatusSeverity? queuedStatusSeverity;

    protected override async Task OnInitializedAsync()
    {
        if (!await AuthHelper.CanAccessProject(ProjectId.ToString()))
        {
            queuedStatusMessage = "You do not have access to this project or it does not exist.";
            queuedStatusSeverity = StatusIndicator.StatusSeverity.Error;
            return;
        }

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

    private void ShowCreatePropertyForm()
    {
        NewProperty = new SchemaPropertyCreationDto
        {
            SchemaId = SchemaId.ToString(),
            Name = string.Empty,
            Type = SchemaPropertyType.Text,
            Options = null
        };
        EnumOptions = string.Empty;
        isCreatePropertyFormVisible = true;
    }

    private void HideCreatePropertyForm()
    {
        isCreatePropertyFormVisible = false;
    }

    private void OnTypeChanged(ChangeEventArgs _)
    {
        if (NewProperty.Type != SchemaPropertyType.Enum)
        {
            EnumOptions = string.Empty;
            NewProperty.Options = null;
        }
    }

    private bool IsEnumOptionsValid =>
        NewProperty.Type != SchemaPropertyType.Enum ||
        (!string.IsNullOrWhiteSpace(EnumOptions) &&
         EnumOptions.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length > 0);

    private async Task HandleCreatePropertySubmit()
    {
        if (!await AuthHelper.CanAccessProject(ProjectId.ToString()))
        {
            statusIndicator?.Show("You do not have access to this project or it does not exist.",
                StatusIndicator.StatusSeverity.Error);
            return;
        }
        
        if (NewProperty.Type == SchemaPropertyType.Enum)
        {
            var options = string.IsNullOrWhiteSpace(EnumOptions)
                ? []
                : EnumOptions.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (options.Length == 0)
            {
                return;
            }
            NewProperty.Options = options;
        }

        var result = await PropertyService.CreateSchemaPropertyAsync(NewProperty);

        if (result.IsSuccess)
        {
            Schema.Properties.Add(result.Value);
            isCreatePropertyFormVisible = false;
            statusIndicator?.Show("Successfully created schema property.",
                StatusIndicator.StatusSeverity.Success);
        }
        else
        {
            statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error",
                StatusIndicator.StatusSeverity.Error);
        }
        
        NewProperty = new SchemaPropertyCreationDto
        {
            SchemaId = SchemaId.ToString(),
            Name = string.Empty,
            Type = SchemaPropertyType.Text,
            Options = null
        };
        StateHasChanged();
    }
}