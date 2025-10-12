using CMS.Main.Abstractions.Notifications;
using CMS.Main.Abstractions.Properties.PropertyTypes;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Schemas;

public partial class PropertyCreateForm : ComponentBase
{
    [Parameter, EditorRequired]
    public SchemaDto Schema { get; set; } = default!;

    [Parameter]
    public EventCallback OnSuccess { get; set; }
    
    [Parameter]
    public EventCallback OnCancel { get; set; }
    
    [Parameter]
    public bool Visible { get; set; } = true;
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private IPropertyService PropertyService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    private PropertyDto PropertyDto { get; set; } = new();
    private string EnumOptions { get; set; } = string.Empty;
    private PropertyType[] PropertyTypes { get; } = Enum.GetValues<PropertyType>();

    public void ResetForm()
    {
        PropertyDto = new PropertyDto
        {
            SchemaId = Schema.Id,
            Name = string.Empty,
            Type = PropertyType.Text,
            Options = null
        };
        EnumOptions = string.Empty;
    }

    private void OnTypeChanged(ChangeEventArgs _)
    {
        if (PropertyDto.Type != PropertyType.Enum)
        {
            EnumOptions = string.Empty;
            PropertyDto.Options = null;
        }
    }
    
    private bool IsEnumOptionsValid =>
        PropertyDto.Type != PropertyType.Enum ||
        (!string.IsNullOrWhiteSpace(EnumOptions) &&
         EnumOptions.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length > 0);

    private async Task HandleCreatePropertySubmit()
    {
        if (!await AuthHelper.OwnsSchema(Schema.Id))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
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

        var result = await PropertyService.CreatePropertyAsync(PropertyDto);

        if (result.IsSuccess)
        {
            Schema.Properties.Add(result.Value);
            await OnSuccess.InvokeAsync();
        }
        else
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    $"There was an error when creating property named {PropertyDto.Name}.",
                Type = NotificationType.Error
            });
        }
        
        StateHasChanged();
    }
}