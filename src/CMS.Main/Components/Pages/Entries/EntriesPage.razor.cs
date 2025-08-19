using CMS.Main.Client.Components;
using CMS.Main.Components.Shared;
using CMS.Shared.Abstractions;
using CMS.Shared.DTOs.Entry;
using CMS.Shared.DTOs.Pagination;
using CMS.Shared.DTOs.Schema;
using CMS.Shared.DTOs.SchemaProperty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;

namespace CMS.Main.Components.Pages.Entries;

public partial class EntriesPage : ComponentBase
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
    private IEntryService EntryService { get; set; } = default!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    
    private List<EntryWithIdDto> Entries { get; set; } = [];

    private DynamicEntryForm? entryForm;

    // Used later for loading more entries
    private int totalCount;
    
    private StatusIndicator? statusIndicator;
    private string? statusText;
    private bool pendingStatusError;

    private bool showForm;

    private string navigationSource = "schema";
    
    protected override async Task OnInitializedAsync()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("from", out var fromVal))
        {
            navigationSource = fromVal.ToString();
        }

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
            return;
        }
        
        var entriesResult = await EntryService.GetEntriesForSchema(
            SchemaId.ToString(),
            new PaginationParams(1, 100));

        if (entriesResult.IsSuccess)
        {
            (Entries, var pagination) = entriesResult.Value;
            totalCount = pagination.TotalCount;
        }
        else
        {
            statusText = entriesResult.Errors.FirstOrDefault() ?? "There was an error";
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

    private async Task OnEntrySubmit(Dictionary<SchemaPropertyWithIdDto, object?> entry)
    {
        if (!await HasAccess())
            return;

        var creationDto = new EntryCreationDto
        {
            SchemaId = SchemaId.ToString(),
            Properties = entry
        };

        var result = await EntryService.AddEntryAsync(creationDto);

        if (result.IsSuccess)
        {
            if (result.Value is not null)
            {
                Entries.Insert(0, result.Value);
            }
            statusText = "Entry created successfully.";
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Success);
            showForm = false;
            
            entryForm?.Reset();
            StateHasChanged();
        }
        else
        {
            statusText = result.Errors.FirstOrDefault() ?? "There was an error";
            statusIndicator?.Show(StatusIndicator.StatusSeverity.Error);
        }
    }

    private void ToggleAddForm()
    {
        showForm = !showForm;
    }

    private object? GetEntryPropertyValue(EntryWithIdDto entry, string propertyName)
    {
        foreach (var kv in entry.Properties)
        {
            if (kv.Key.Name == propertyName)
                return kv.Value;
        }
        return null;
    }

    private RenderFragment FormatCell(object? value) => builder =>
    {
        switch (value)
        {
            case null:
                builder.AddContent(0, "null");
                return;
            case string s:
            {
                builder.AddContent(0, DateTime.TryParse(s, out var dt) ? dt.ToLocalTime().ToString("g") : s);
                return;
            }
            case bool b:
                builder.AddContent(0, b.ToString().ToLowerInvariant());
                return;
            default:
                builder.AddContent(0, value.ToString());
                break;
        }
    };
}