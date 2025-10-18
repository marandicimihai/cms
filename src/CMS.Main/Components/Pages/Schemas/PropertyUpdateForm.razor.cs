using CMS.Main.Abstractions.Notifications;
using CMS.Main.Abstractions.Properties.PropertyTypes;
using CMS.Main.Abstractions.SchemaProperties;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CMS.Main.Components.Pages.Schemas;

public partial class PropertyUpdateForm : ComponentBase
{
    private bool _isOpen;

    [Parameter, EditorRequired]
    public SchemaDto Schema { get; set; } = default!;

    [Parameter]
    public EventCallback OnPropertyUpdated { get; set; }
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private IPropertyService PropertyService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    [SupplyParameterFromForm]
    private PropertyDto PropertyDto { get; set; } = new();
    private string EnumOptions { get; set; } = string.Empty;

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen != value)
            {
                _isOpen = value;
                StateHasChanged();
            }
        }
    }

    public void Open(PropertyDto propertyDto)
    {
        PropertyDto = propertyDto.Adapt<PropertyDto>();
        EnumOptions = PropertyDto.Options is { Length: > 0 } ? string.Join(", ", PropertyDto.Options) : string.Empty;
        IsOpen = true;
    }

    public void CloseModal()
    {
        IsOpen = false;
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (!IsOpen) return;
        if (e.Key == "Escape")
        {
            CloseModal();
        }
    }

    private bool IsEnumOptionsValid =>
        PropertyDto.Type != PropertyType.Enum ||
        (!string.IsNullOrWhiteSpace(EnumOptions) &&
         EnumOptions.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length > 0);

    private async Task HandleUpdatePropertySubmit()
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

        var result = await PropertyService.UpdatePropertyAsync(PropertyDto);

        if (!result.IsSuccess)
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    $"There was an error when updating property named {PropertyDto.Name}.",
                Type = NotificationType.Error
            });
            return;
        }

        await Notifications.NotifyAsync(new()
        {
            Message = $"Updated property named {result.Value.Name}.",
            Type = NotificationType.Success
        });

        CloseModal();
        await OnPropertyUpdated.InvokeAsync();
    }
}