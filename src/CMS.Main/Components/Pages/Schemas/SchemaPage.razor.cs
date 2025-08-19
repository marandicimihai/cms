using CMS.Main.Client.Components;
using CMS.Main.Services;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Schema;
using CMS.Shared.DTOs.SchemaProperty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace CMS.Main.Components.Pages.Schemas;

public partial class SchemaPage : ComponentBase
{
    [Parameter]
    public Guid ProjectId { get; set; }
    
    [Parameter]
    public Guid SchemaId { get; set; }

    private SchemaWithIdDto Schema { get; set; } = new();
    
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    
    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;
    
    [Inject]
    private ISchemaPropertyService PropertyService { get; set; } = default!;
    
    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    private StatusIndicator? statusIndicator;
    private string? statusText;
    private bool pendingStatusError;

    private bool isCreatePropertyFormVisible;
    private SchemaPropertyCreationDto NewProperty { get; set; } = new();
    private string OptionsCsv { get; set; } = string.Empty;
    private SchemaPropertyType[] PropertyTypes { get; } = Enum.GetValues<SchemaPropertyType>();

    protected override async Task OnInitializedAsync()
    {
        if (!await HasAccess())
            return;

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
            statusText = result.Errors.FirstOrDefault() ?? "There was an error";
            pendingStatusError = true;
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (pendingStatusError)
        {
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
            pendingStatusError = false;
            StateHasChanged();
        }
    }

    private async Task<bool> HasAccess()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var authorizationResult =
            await AuthorizationService.AuthorizeAsync(user, ProjectId.ToString(), "ProjectPolicies.CanEditProject");

        if (authorizationResult.Succeeded) return true;

        statusText = "Could not load project. You do not have permission to access this project.";
        statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
        pendingStatusError = true;

        return false;
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
        OptionsCsv = string.Empty;
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
            OptionsCsv = string.Empty;
            NewProperty.Options = null;
        }
    }

    private bool IsEnumOptionsValid =>
        NewProperty.Type != SchemaPropertyType.Enum ||
        (!string.IsNullOrWhiteSpace(OptionsCsv) &&
         OptionsCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length > 0);

    private async Task HandleCreatePropertySubmit()
    {
        if (!await HasAccess())
            return;
        
        if (NewProperty.Type == SchemaPropertyType.Enum)
        {
            var options = string.IsNullOrWhiteSpace(OptionsCsv)
                ? []
                : OptionsCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
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
            statusText = "Successfully created schema property.";
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Success);
        }
        else
        {
            statusText = result.Errors.FirstOrDefault() ?? "There was an error";
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
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

    private async Task OnDeletePropertyClicked(SchemaPropertyWithIdDto property)
    {
        if (!await HasAccess())
            return;
        
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
            statusText = "Successfully deleted schema property.";
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Success);
            
            Schema.Properties.Remove(property);
        }
        else
        {
            statusText = result.Errors.FirstOrDefault() ?? "There was an error";
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
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
                    SchemaPropertyType.Integer => 123,
                    SchemaPropertyType.Boolean => true,
                    SchemaPropertyType.DateTime => DateTime.UtcNow.ToString("o"),
                    SchemaPropertyType.Decimal => 123.45m,
                    SchemaPropertyType.Enum => (p.Options != null && p.Options.Length > 0) ? p.Options[0] : "ExampleOption",
                    _ => null
                };
            }
            return JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}