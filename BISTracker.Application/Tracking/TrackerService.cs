using BISTracker.Domain;

namespace BISTracker.Application;

public sealed class TrackerService
{
    private readonly IBisCatalog _catalog;
    private readonly IProgressRepository _progress;
    private readonly SemaphoreSlim _operationGate = new(1, 1);

    public TrackerService(IBisCatalog catalog, IProgressRepository progress)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(progress);
        _catalog = catalog;
        _progress = progress;
    }

    public Task<TrackerSnapshot> LoadAsync(CancellationToken cancellationToken = default) =>
        ExecuteAsync(null, cancellationToken);

    public Task<TrackerSnapshot> SetOwnedAsync(string itemId, bool owned, CancellationToken cancellationToken = default) =>
        ExecuteAsync(progress => progress.SetOwned(itemId, owned), cancellationToken);

    public Task<TrackerSnapshot> SetEquippedAsync(string itemId, bool equipped, CancellationToken cancellationToken = default) =>
        ExecuteAsync(progress => progress.SetEquipped(itemId, equipped), cancellationToken);

    private async Task<TrackerSnapshot> ExecuteAsync(Action<CharacterProgress>? change, CancellationToken cancellationToken)
    {
        // Serialize this service's read/change/save operations to avoid lost updates.
        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var loadedCatalog = await _catalog.LoadAsync(cancellationToken).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(loadedCatalog);
            ArgumentNullException.ThrowIfNull(loadedCatalog.Context);
            ArgumentNullException.ThrowIfNull(loadedCatalog.Items);
            var catalog = loadedCatalog with { Items = Array.AsReadOnly(loadedCatalog.Items.ToArray()) };

            var state = await _progress.LoadAsync(cancellationToken).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(state.OwnedItemIds);
            ArgumentNullException.ThrowIfNull(state.EquippedItems);
            var character = new CharacterProgress(catalog.Items, state.OwnedItemIds, state.EquippedItems);

            if (change is not null)
            {
                change(character);
                var nextState = new ProgressState(character.OwnedItemIds.ToArray(), new Dictionary<EquipmentSlot, string>(character.EquippedItems));
                await _progress.SaveAsync(nextState, cancellationToken).ConfigureAwait(false);
            }

            var entries = catalog.Items.Select(item => new TrackerEntry(item, character.IsOwned(item.Id), character.IsEquipped(item.Id))).ToArray();
            return new TrackerSnapshot(catalog, Array.AsReadOnly(entries));
        }
        finally
        {
            _operationGate.Release();
        }
    }
}
