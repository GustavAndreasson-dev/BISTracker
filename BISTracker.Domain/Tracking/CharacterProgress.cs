using System.Collections.ObjectModel;

namespace BISTracker.Domain;

/// <summary>Owns the consistency rules for a character's progress against a catalog.</summary>
public sealed class CharacterProgress
{
    private readonly Dictionary<string, Recommendation> _recommendations = new(StringComparer.Ordinal);
    private readonly HashSet<string> _ownedItemIds = new(StringComparer.Ordinal);
    private readonly Dictionary<EquipmentSlot, string> _equippedItems = new();

    public CharacterProgress(
        IEnumerable<Recommendation> recommendations,
        IEnumerable<string>? ownedItemIds = null,
        IReadOnlyDictionary<EquipmentSlot, string>? equippedItems = null)
    {
        ArgumentNullException.ThrowIfNull(recommendations);

        foreach (var recommendation in recommendations)
        {
            ArgumentNullException.ThrowIfNull(recommendation);
            ArgumentException.ThrowIfNullOrWhiteSpace(recommendation.Id);
            if (!Enum.IsDefined(recommendation.Slot))
                throw new ArgumentException("A recommendation has an unknown equipment slot.", nameof(recommendations));
            if (!_recommendations.TryAdd(recommendation.Id, recommendation))
                throw new ArgumentException($"Duplicate recommendation ID '{recommendation.Id}'.", nameof(recommendations));
        }

        foreach (var itemId in ownedItemIds ?? Enumerable.Empty<string>())
        {
            GetRecommendation(itemId);
            _ownedItemIds.Add(itemId);
        }

        if (equippedItems is null)
            return;

        foreach (var (slot, itemId) in equippedItems)
        {
            var recommendation = GetRecommendation(itemId);
            if (recommendation.Slot != slot)
                throw new ArgumentException($"Item '{itemId}' does not belong in slot '{slot}'.", nameof(equippedItems));
            if (!_ownedItemIds.Contains(itemId))
                throw new ArgumentException($"Equipped item '{itemId}' must also be owned.", nameof(equippedItems));
            _equippedItems.Add(slot, itemId);
        }
    }

    // Return detached, read-only copies so callers cannot mutate aggregate state.
    public IReadOnlyCollection<string> OwnedItemIds => Array.AsReadOnly(_ownedItemIds.Order(StringComparer.Ordinal).ToArray());

    public IReadOnlyDictionary<EquipmentSlot, string> EquippedItems =>
        new ReadOnlyDictionary<EquipmentSlot, string>(new Dictionary<EquipmentSlot, string>(_equippedItems));

    public bool IsOwned(string itemId)
    {
        GetRecommendation(itemId);
        return _ownedItemIds.Contains(itemId);
    }

    public bool IsEquipped(string itemId)
    {
        var recommendation = GetRecommendation(itemId);
        return _equippedItems.TryGetValue(recommendation.Slot, out var equippedId)
            && StringComparer.Ordinal.Equals(equippedId, itemId);
    }

    public void SetOwned(string itemId, bool owned)
    {
        var recommendation = GetRecommendation(itemId);
        if (owned)
        {
            _ownedItemIds.Add(itemId);
            return;
        }

        _ownedItemIds.Remove(itemId);
        if (_equippedItems.TryGetValue(recommendation.Slot, out var equippedId)
            && StringComparer.Ordinal.Equals(equippedId, itemId))
            _equippedItems.Remove(recommendation.Slot);
    }

    public void SetEquipped(string itemId, bool equipped)
    {
        var recommendation = GetRecommendation(itemId);
        if (equipped)
        {
            _ownedItemIds.Add(itemId);
            _equippedItems[recommendation.Slot] = itemId;
            return;
        }

        if (_equippedItems.TryGetValue(recommendation.Slot, out var equippedId)
            && StringComparer.Ordinal.Equals(equippedId, itemId))
            _equippedItems.Remove(recommendation.Slot);
    }

    private Recommendation GetRecommendation(string itemId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        return _recommendations.TryGetValue(itemId, out var recommendation)
            ? recommendation
            : throw new ArgumentException($"Unknown recommendation ID '{itemId}'.", nameof(itemId));
    }
}
