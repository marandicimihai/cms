using System.Text.Json;
using CMS.Main.Abstractions;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs.Schema;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Schemas;

public partial class ViewSchemaPropertiesTable : ComponentBase
{
    [Parameter, EditorRequired]
    public SchemaDto Schema { get; set; } = default!;
    
    [Parameter]
    public EventCallback<SchemaPropertyDto> OnEditProperty { get; set; }

    [Inject]
    private ISchemaPropertyService PropertyService { get; set; } = default!;
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    private StatusIndicator? statusIndicator;
    
    private async Task OnDeletePropertyClicked(SchemaPropertyDto property)
    {
        if (!await AuthHelper.CanEditSchema(Schema.Id))
        {
            statusIndicator?.Show("You do not have access to this schema or it does not exist.",
                StatusIndicator.StatusSeverity.Error);
            return;
        }
        
        var isConfirmed = await ConfirmationService.ShowAsync(
            title: "Delete Schema Property",
            message: $"Are you sure you want to delete the property '{property.Name}'? This action cannot be undone and may result in the loss of data.",
            confirmText: "Delete",
            cancelText: "Cancel"
            );

        if (!isConfirmed)
            return;
        
        var result = await PropertyService.DeleteSchemaPropertyAsync(property.Id);

        if (result.IsSuccess)
        {
            statusIndicator?.Show("Successfully deleted schema property.",
                StatusIndicator.StatusSeverity.Success);

            Schema.Properties.Remove(property);
        }
        else
        {
            statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error",
                StatusIndicator.StatusSeverity.Error);
        }
    }

    private string ExampleJson
    {
        get
        {
            if (Schema?.Properties == null || Schema.Properties.Count == 0)
                return "{\n}";
            var dict = new Dictionary<string, object?>();
            foreach (var p in Schema.Properties)
            {
                dict[p.Name] = p.Type switch
                {
                    SchemaPropertyType.Text => $"Sample {p.Name}",
                    SchemaPropertyType.Boolean => true,
                    SchemaPropertyType.DateTime => DateTime.UtcNow.ToString("o"),
                    SchemaPropertyType.Number => 123.45,
                    SchemaPropertyType.Enum => p.Options is { Length: > 0 } ? p.Options[0] : "ExampleOption",
                    _ => null
                };
            }
            return JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}