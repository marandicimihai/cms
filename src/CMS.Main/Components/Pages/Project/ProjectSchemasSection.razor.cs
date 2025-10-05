using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.DTOs;
using CMS.Main.Services;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Project;

public partial class ProjectSchemasSection : ComponentBase
{
    [Parameter]
    public List<SchemaDto> Schemas { get; set; } = [];

    [Parameter]
    public string ProjectId { get; set; } = string.Empty;

    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;

    [Inject]
    private AuthorizationHelperService AuthHelper { get; set; } = default!;
    
    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    [SupplyParameterFromForm]
    public SchemaDto NewSchema { get; set; } = new();
    private bool IsAddFormVisible { get; set; }

    private bool IsCreatingSchema { get; set; }

    protected override void OnParametersSet()
    {
        NewSchema = new SchemaDto { ProjectId = ProjectId };
    }

    private void ShowAddForm()
    {
        IsAddFormVisible = true;
        NewSchema = new SchemaDto { ProjectId = ProjectId };
    }

    private void HideAddForm()
    {
        IsAddFormVisible = false;
        NewSchema = new SchemaDto { ProjectId = ProjectId };
    }

    public async Task HandleAddSchema()
    {
        if (!await AuthHelper.CanEditProject(ProjectId))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Could not retrieve resource.",
                Type = NotificationType.Error
            });
            return;
        }

        IsCreatingSchema = true;
        StateHasChanged();
        await Task.Yield();

        var result = await SchemaService.CreateSchemaAsync(NewSchema);

        if (result.IsSuccess)
        {
            IsAddFormVisible = false;
            NewSchema = new SchemaDto { ProjectId = ProjectId };
            Schemas.Add(result.Value);
        }
        else
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ?? "There was an error",
                Type = NotificationType.Error
            });
        }

        IsCreatingSchema = false;
        StateHasChanged();
    }

    private async Task OnDeleteSchemaAsync(SchemaDto schema)
    {
        if (!await AuthHelper.CanEditProject(ProjectId))
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