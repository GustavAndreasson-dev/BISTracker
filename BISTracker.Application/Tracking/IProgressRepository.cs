namespace BISTracker.Application;

public interface IProgressRepository
{
    Task<ProgressState> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(ProgressState state, CancellationToken cancellationToken = default);
}
