using CMS.Main.Abstractions.Properties.PropertyTypes;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Schemas;

public partial class PropertyUpdateForm : ComponentBase
{
    [Parameter, EditorRequired]
    public SchemaDto Schema { get; set; } = default!;

    [Parameter]
    public EventCallback OnSuccess { get; set; }
    
    [Parameter]
    public EventCallback OnCancel { get; set; }
    
    [Parameter]
    public StatusIndicator? StatusIndicator { get; set; }
    
    [Parameter]
    public bool Visible { get; set; } = true;
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ISchemaPropertyService PropertyService { get; set; } = default!;

    [SupplyParameterFromForm]
    private PropertyDto PropertyDto { get; set; } = new();
    private string EnumOptions { get; set; } = string.Empty;

    public void SetModel(PropertyDto propertyDto)
    {
        PropertyDto = propertyDto;
        EnumOptions = PropertyDto.Options is { Length: > 0 } ? string.Join(", ", PropertyDto.Options) : string.Empty;
    }

    private bool IsEnumOptionsValid =>
        PropertyDto.Type != PropertyType.Enum ||
        (!string.IsNullOrWhiteSpace(EnumOptions) &&
         EnumOptions.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length > 0);

    private async Task HandleUpdatePropertySubmit()
    {
        if (!await AuthHelper.CanEditSchema(Schema.Id))
        {
            StatusIndicator?.Show("You do not have access to this schema or it does not exist.",
                StatusIndicator.StatusSeverity.Error);
            return;
        }

        if (PropertyDto.Type == PropertyType.Enum)
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
            StatusIndicator?.Show("Successfully updated schema property.",
                StatusIndicator.StatusSeverity.Success);
            await OnSuccess.InvokeAsync();
        }
        else
        {
            StatusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error",
                StatusIndicator.StatusSeverity.Error);
        }
        
        StateHasChanged();
    }
}