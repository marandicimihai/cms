using System.Text.Json;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.Abstractions.Properties.PropertyTypes;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Schemas;

public partial class ViewSchemaPropertiesTable : ComponentBase
{
    [Parameter, EditorRequired]
    public SchemaDto Schema { get; set; } = default!;
    
    [Parameter]
    public EventCallback<PropertyDto> OnEditProperty { get; set; }

    [Inject]
    private ISchemaPropertyService PropertyService { get; set; } = default!;
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    private async Task OnDeletePropertyClicked(PropertyDto property)
    {
        if (!await AuthHelper.CanEditSchema(Schema.Id))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        var isConfirmed = await ConfirmationService.ShowAsync(
            title: "Delete Property",
            message: $"Are you sure you want to delete the property '{property.Name}'? This action cannot be undone and may result in the loss of data.",
            confirmText: "Delete",
            cancelText: "Cancel"
            );

        if (!isConfirmed)
            return;

        var result = await PropertyService.DeleteSchemaPropertyAsync(property.Id);

        if (result.IsSuccess)
        {
            await Notifications.NotifyAsync(new()
            {
                Message = $"Deleted property named {property.Name}.",
                Type = NotificationType.Info
            });

            Schema.Properties.Remove(property);
        }
        else
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ?? "There was an error",
                Type = NotificationType.Error
            });
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
                    PropertyType.Text => $"Sample {p.Name}",
                    PropertyType.Boolean => true,
                    PropertyType.DateTime => DateTime.UtcNow.ToString("o"),
                    PropertyType.Number => 123.45,
                    PropertyType.Enum => p.Options is { Length: > 0 } ? p.Options[0] : "ExampleOption",
                    _ => null
                };
            }
            return JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}