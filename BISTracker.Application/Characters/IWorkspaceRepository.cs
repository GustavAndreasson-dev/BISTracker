namespace BISTracker.Application;

public interface IWorkspaceRepository
{
    Task<WorkspaceState?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(WorkspaceState state, CancellationToken cancellationToken = default);
}
