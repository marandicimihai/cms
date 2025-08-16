using CMS.Main.Client.Components;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Schema;
using CMS.Shared.DTOs.SchemaProperty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

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
            statusText = result.Errors.First();
            pendingStatusError = true;
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (pendingStatusError)
        {
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
            pendingStatusError = false;
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
            statusText = result.Errors.First();
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
}