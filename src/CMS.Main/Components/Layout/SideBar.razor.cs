using System.Security.Claims;
using CMS.Main.Abstractions;
using CMS.Main.DTOs;
using CMS.Main.DTOs.Pagination;
using CMS.Main.Services.State;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Layout;

[StreamRendering]
[Authorize]
public partial class SideBar : ComponentBase, IDisposable
{
    private readonly int pageSize = 20;
    private bool isLoadingMore;
    private int totalCount;
    private bool isHidden = false;

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    [Inject]
    private ProjectStateService ProjectStateService { get; set; } = default!;

    private List<ProjectDto> Projects { get; set; } = [];
    private bool HasMoreProjects => Projects.Count < totalCount;

    protected override async Task OnInitializedAsync()
    {
        ProjectStateService.ProjectsCreated += ProjectsCreated;
        ProjectStateService.ProjectsUpdated += ProjectsUpdated;
        ProjectStateService.ProjectsDeleted += OnProjectsDeleted;
        await LoadInitialProjectsAsync();
    }

    private async Task LoadInitialProjectsAsync()
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var result = await ProjectService.GetProjectsForUserAsync(
                        userId,
                        new PaginationParams(1, pageSize));

                    if (result.IsSuccess)
                    {
                        Projects = result.Value.Item1;
                        totalCount = result.Value.Item2.TotalCount;
                    }
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private async Task LoadMoreProjectsAsync()
    {
        if (isLoadingMore || !HasMoreProjects) return;
        isLoadingMore = true;
        StateHasChanged();
        await Task.Yield();
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var result = await ProjectService.GetProjectsForUserAsync(userId,
                        new PaginationParams(Projects.Count / pageSize + 1, pageSize));
                    if (result.IsSuccess)
                    {
                        var newProjects = result.Value.Item1;
                        foreach (var p in newProjects)
                            if (Projects.All(existing => existing.Id != p.Id))
                                Projects.Add(p);
                        totalCount = result.Value.Item2.TotalCount;
                    }
                }
            }
        }
        catch
        {
            // ignored
        }

        isLoadingMore = false;
        StateHasChanged();
    }

    private void ProjectsCreated(List<ProjectDto> projects)
    {
        Projects.AddRange(projects);
        Projects = Projects.OrderByDescending(p => p.LastUpdated).ToList();
        StateHasChanged();
    }

    private void ProjectsUpdated(List<ProjectDto> projects)
    {
        foreach (var updatedProject in projects)
        {
            var existingProject = Projects.FirstOrDefault(p => p.Id == updatedProject.Id);
            if (existingProject != null)
            {
                existingProject.Name = updatedProject.Name;
                existingProject.LastUpdated = updatedProject.LastUpdated;
            }
            else
            {
                Projects.Add(updatedProject);
            }
        }

        Projects = Projects.OrderByDescending(p => p.LastUpdated).ToList();
        StateHasChanged();
    }

    private void OnProjectsDeleted(List<string> projectIds)
    {
        foreach (var id in projectIds)
        {
            var project = Projects.FirstOrDefault(p => p.Id == id);
            if (project != null)
                Projects.Remove(project);
            totalCount--;
        }

        StateHasChanged();
    }

    private void ToggleSidebar()
    {
        isHidden = !isHidden;
        StateHasChanged();
    }

    public void Dispose()
    {
        ProjectStateService.ProjectsCreated -= ProjectsCreated;
        ProjectStateService.ProjectsUpdated -= ProjectsUpdated;
        ProjectStateService.ProjectsDeleted -= OnProjectsDeleted;
        GC.SuppressFinalize(this);
    }
}