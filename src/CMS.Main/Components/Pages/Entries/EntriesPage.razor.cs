using CMS.Main.Client.Components;
using CMS.Main.Client.Services;
using CMS.Main.Client.Services.State;
using CMS.Main.Components.Shared;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Entry;
using CMS.Shared.DTOs.Schema;
using CMS.Shared.DTOs.SchemaProperty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CMS.Main.Components.Pages.Entries;

public partial class EntriesPage : ComponentBase
{
    [Parameter]
    public Guid SchemaId { get; set; }
    
    private SchemaDto Schema { get; set; } = new();
    
    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;
    
    [Inject]
    private IEntryService EntryService { get; set; } = default!;

    [Inject]
    private EntryStateService EntryStateService { get; set; } = default!;

    private DynamicEntryForm? entryForm;
    
    private StatusIndicator? statusIndicator;

    private bool showForm;

    private string? queuedStatusMessage;
    private StatusIndicator.StatusSeverity? queuedStatusSeverity;

    protected override async Task OnInitializedAsync()
    {
        if (!await AuthHelper.CanEditSchema(SchemaId.ToString()))
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

    private async Task OnEntrySubmit(Dictionary<SchemaPropertyDto, object?> entry)
    {
        if (!await AuthHelper.CanEditSchema(SchemaId.ToString()))
        {
            statusIndicator?.Show("You do not have access to this project or it does not exist.",
                StatusIndicator.StatusSeverity.Error);
            return;
        }

        var dto = new EntryDto
        {
            SchemaId = SchemaId.ToString(),
            Properties = entry
        };

        var result = await EntryService.AddEntryAsync(dto);

        if (result.IsSuccess)
        {
            EntryStateService.NotifyCreated([result.Value]);
            
            statusIndicator?.Show("Entry created successfully.",
                StatusIndicator.StatusSeverity.Success);
            showForm = false;
            
            entryForm?.Reset();
            StateHasChanged();
        }
        else
        {
            statusIndicator?.Show(result.Errors.FirstOrDefault() ?? "There was an error",
                StatusIndicator.StatusSeverity.Error);
        }
    }

    private void ToggleAddForm()
    {
        showForm = !showForm;
    }
}