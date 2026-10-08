namespace BISTracker.Application;

public interface ITrackerService
{
    Task<TrackerSnapshot> LoadAsync(CancellationToken cancellationToken = default);
    Task<TrackerSnapshot> SetOwnedAsync(string itemId, bool owned, CancellationToken cancellationToken = default);
    Task<TrackerSnapshot> SetEquippedAsync(string itemId, bool equipped, CancellationToken cancellationToken = default);
}
