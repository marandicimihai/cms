using CMS.Shared.DTOs.Project;

namespace CMS.Main.Client.Services.State;

public class ProjectStateService
{
    public event Action<List<ProjectDto>>? ProjectsCreated;
    public event Action<List<ProjectDto>>? ProjectsUpdated;
    public event Action<List<string>>? ProjectsDeleted;

    public void NotifyCreated(List<ProjectDto> projects)
    {
        ProjectsCreated?.Invoke(projects);
    }

    public void NotifyUpdated(List<ProjectDto> projects)
    {
        ProjectsUpdated?.Invoke(projects);
    }

    public void NotifyDeleted(List<string> projectIds)
    {
        ProjectsDeleted?.Invoke(projectIds);
    }
}