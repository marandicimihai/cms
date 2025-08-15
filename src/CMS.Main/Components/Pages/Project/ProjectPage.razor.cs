using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Pages.Project;

public partial class ProjectPage : ComponentBase
{
    [Parameter]
    public Guid ProjectId { get; set; }
}