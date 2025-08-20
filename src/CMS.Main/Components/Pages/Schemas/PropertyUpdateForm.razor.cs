using CMS.Main.Client.Components;
using CMS.Main.Client.Services;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Schema;
using CMS.Shared.DTOs.SchemaProperty;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Schemas;

public partial class PropertyUpdateForm : ComponentBase
{
    [Parameter, EditorRequired]
    public SchemaWithIdDto Schema { get; set; } = default!;

    [Parameter]
    public EventCallback OnHide { get; set; }
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ISchemaPropertyService PropertyService { get; set; } = default!;

    public bool FormVisible { get; private set; }

    private SchemaPropertyDto PropertyDto { get; set; } = new();
    private string EnumOptions { get; set; } = string.Empty;
    private SchemaPropertyType[] PropertyTypes { get; } = Enum.GetValues<SchemaPropertyType>();
    
    private StatusIndicator? statusIndicator;

    public void ShowForm(SchemaPropertyDto propertyDto)
    {
        PropertyDto = propertyDto;
        FormVisible = true;
    }

    private async Task HideForm()
    {
        FormVisible = false;
        statusIndicator?.Hide();
        await OnHide.InvokeAsync();
    }

    private void OnTypeChanged(ChangeEventArgs _)
    {
        if (PropertyDto.Type != SchemaPropertyType.Enum)
        {
            EnumOptions = string.Empty;
            PropertyDto.Options = null;
        }
    }
    
    private bool IsEnumOptionsValid =>
        PropertyDto.Type != SchemaPropertyType.Enum ||
        (!string.IsNullOrWhiteSpace(EnumOptions) &&
         EnumOptions.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length > 0);

    private async Task HandleUpdatePropertySubmit()
    {
        if (!await AuthHelper.CanEditSchema(Schema.Id))
        {
            statusIndicator?.Show("You do not have access to this schema or it does not exist.",
                StatusIndicator.StatusSeverity.Error);
            return;
        }
        
        if (PropertyDto.Type == SchemaPropertyType.Enum)
        {
            var options = string.IsNullOrWhiteSpace(EnumOptions)
                ? []
                : EnumOptions.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (options.Length == 0)
            {
                return;
            }
            PropertyDto.Options = options;
        }
        
        PropertyDto.SchemaId = Schema.Id;

        var result = await PropertyService.UpdateSchemaPropertyAsync(PropertyDto);

        if (result.IsSuccess)
        {
            Schema.Properties.Add(result.Value);
            statusIndicator?.Show("Successfully created schema property.",
                StatusIndicator.StatusSeverity.Success);
            await HideForm();
        }
        else
        {
            statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error",
                StatusIndicator.StatusSeverity.Error);
        }
        
        StateHasChanged();
    }
}