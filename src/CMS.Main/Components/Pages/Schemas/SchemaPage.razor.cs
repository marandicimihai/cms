using CMS.Main.Abstractions;
using CMS.Main.Components.Shared;
using CMS.Main.DTOs.Schema;
using CMS.Main.DTOs.SchemaProperty;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Schemas;

public partial class SchemaPage : ComponentBase
{
    [Parameter]
    public Guid SchemaId { get; set; }

    private SchemaDto Schema { get; set; } = new();
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;
    
    [Inject]
    private ISchemaPropertyService PropertyService { get; set; } = default!;

    private PropertyCreateForm? createForm;
    private PropertyUpdateForm? updateForm;
    private StatusIndicator? statusIndicator;
    
    private bool createFormVisible;
    private bool updateFormVisible;

    private string? queuedStatusMessage;
    private StatusIndicator.StatusSeverity? queuedStatusSeverity;

    protected override async Task OnInitializedAsync()
    {
        if (!await AuthHelper.CanEditSchema(SchemaId.ToString()))
        {
            queuedStatusMessage = "You do not have access to this schema or it does not exist.";
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

    private void ShowCreateForm()
    {
        createForm?.ResetForm();
        createFormVisible = true;
        updateFormVisible = false;
    }
    
    private void HideCreateForm()
    {
        createFormVisible = false;
    }
    
    private void ShowUpdateForm(SchemaPropertyDto property)
    {
        updateForm?.SetModel(property);
        updateFormVisible = true;
        createFormVisible = false;
    }
    
    private void HideUpdateForm()
    {
        updateFormVisible = false;
    }
}