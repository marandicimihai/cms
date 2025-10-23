using System.ComponentModel.DataAnnotations;
using CMS.Main.Abstractions;
using CMS.Main.Abstractions.Notifications;
using CMS.Main.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CMS.Main.Components.Pages.Project;

public partial class CreateSchemaModal : ComponentBase
{
    private bool _isOpen;

    [Parameter]
    public string? ProjectId { get; set; }

    [Parameter]
    public EventCallback OnSchemaCreated { get; set; }

    [SupplyParameterFromForm]
    private SchemaCreateDto CreateDto { get; set; } = new();

    [Inject]
    private ISchemaService SchemaService { get; set; } = default!;

    [Inject]
    private INotificationService Notifications { get; set; } = default!;

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen != value)
            {
                _isOpen = value;
                if (_isOpen)
                {
                    CreateDto = new SchemaCreateDto();
                }
                StateHasChanged();
            }
        }
    }

    public void Open()
    {
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

    private async Task HandleValidSubmit()
    {
        if (string.IsNullOrEmpty(ProjectId))
        {
            await Notifications.NotifyAsync(new()
            {
                Message = "Project ID is required to create a schema.",
                Type = NotificationType.Error
            });
            return;
        }

        var schemaDto = new SchemaDto
        {
            Name = CreateDto.Name,
            ProjectId = ProjectId
        };

        var result = await SchemaService.CreateSchemaAsync(schemaDto);

        if (!result.IsSuccess)
        {
            await Notifications.NotifyAsync(new()
            {
                Message = result.Errors.FirstOrDefault() ??
                    $"There was an error when creating schema named {schemaDto.Name}.",
                Type = NotificationType.Error
            });
            return;
        }

        await Notifications.NotifyAsync(new()
        {
            Message = $"Created schema named {result.Value.Name}.",
            Type = NotificationType.Success
        });

        CloseModal();
        await OnSchemaCreated.InvokeAsync();
    }

    private class SchemaCreateDto
    {
        [Required(ErrorMessage = "Schema name is required.")]
        [Length(3, 50, ErrorMessage = "Schema name must be between 3 and 50 characters long.")]
        public string Name { get; set; } = default!;
    }
}
