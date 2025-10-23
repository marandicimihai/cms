using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Project;

public partial class ProjectSchemasSection : ComponentBase
{
    private CreateSchemaModal createSchemaModal = default!;

    [Parameter]
    public List<SchemaDto> Schemas { get; set; } = [];

    [Parameter]
    public string ProjectId { get; set; } = string.Empty;

    [Parameter]
    public EventCallback OnSchemasChanged { get; set; }

    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;

    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    protected override void OnInitialized()
    {
        
    }

    private void OpenAddSchemaModal()
    {
        createSchemaModal.Open();
    }

    private async Task HandleSchemaCreated()
    {
        // Notify parent component to refresh
        await OnSchemasChanged.InvokeAsync();
    }

    private async Task OnDeleteSchemaAsync(SchemaDto schema)
    {
        if (!await AuthHelper.OwnsProject(ProjectId))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        var confirmed = await ConfirmationService.ShowAsync(
            "Delete Schema",
            "Are you sure you want to delete this schema? This action cannot be undone.",
            "Delete"
        );

        if (confirmed)
        {
            var result = await SchemaService.DeleteSchemaAsync(schema.Id);

            if (result.IsSuccess)
            {
                Schemas.Remove(schema);
                await OnSchemasChanged.InvokeAsync();
            }
            else
            {
                await Notifications.NotifyAsync(new()
                {
                    Message = result.Errors.FirstOrDefault() ??
                        $"There was an error when deleting schema named {schema.Name}.",
                    Type = NotificationType.Error
                });
            }
        }
    }
}