using CMS.Shared.DTOs.Project;

namespace CMS.Main.Client.Services.State;

public class ProjectStateService
{
    public event Action<List<ProjectWithIdDto>>? ProjectsCreated;
    public event Action<List<ProjectWithIdDto>>? ProjectsUpdated;
    public event Action<List<string>>? ProjectsDeleted;

    public void NotifyCreated(List<ProjectWithIdDto> projects)
    {
        ProjectsCreated?.Invoke(projects);
    }

    public void NotifyUpdated(List<ProjectWithIdDto> projects)
    {
        ProjectsUpdated?.Invoke(projects);
    }

    public void NotifyDeleted(List<string> projectIds)
    {
        ProjectsDeleted?.Invoke(projectIds);
    }
}