namespace BISTracker.Domain;

// Ownership identifies a physical item variant; equipment identifies a recommendation in a spec.
public sealed class CharacterLoadouts
{
    private readonly Dictionary<string, Dictionary<string, Recommendation>> _lists;
    private readonly HashSet<string> _owned;
    private readonly Dictionary<string, Dictionary<EquipmentSlot, string>> _equipped;

    public CharacterLoadouts(CharacterClass characterClass,
        IReadOnlyDictionary<string, IReadOnlyList<Recommendation>> lists,
        IEnumerable<string> ownedItemKeys,
        IReadOnlyDictionary<string, Dictionary<EquipmentSlot, string>> equipped)
    {
        ArgumentNullException.ThrowIfNull(lists);
        ArgumentNullException.ThrowIfNull(ownedItemKeys);
        ArgumentNullException.ThrowIfNull(equipped);
        var specs = CharacterDefinition.Specializations(characterClass);
        if (lists.Count != 3 || equipped.Count != 3 || specs.Any(spec => !lists.ContainsKey(spec.Id) || !equipped.ContainsKey(spec.Id)))
            throw new ArgumentException("A character must have exactly its three specialization lists.");

        _lists = new();
        _equipped = new();
        foreach (var spec in specs)
        {
            var items = lists[spec.Id];
            ArgumentNullException.ThrowIfNull(items);
            // Reuse catalog validation, including IDs and valid equipment slots.
            _ = new CharacterProgress(items);
            _lists.Add(spec.Id, items.ToDictionary(item => item.Id));
            ArgumentNullException.ThrowIfNull(equipped[spec.Id]);
            _equipped.Add(spec.Id, new(equipped[spec.Id]));
        }
        _owned = new(ownedItemKeys, StringComparer.Ordinal);
        var knownKeys = _lists.Values.SelectMany(list => list.Values).Select(ItemKey).ToHashSet(StringComparer.Ordinal);
        if (_owned.Any(key => !knownKeys.Contains(key)))
            throw new ArgumentException("Ownership contains an unknown item variant.");
        foreach (var spec in specs)
            foreach (var entry in _equipped[spec.Id])
            {
                var item = Item(spec.Id, entry.Value);
                if (item.Slot != entry.Key || !_owned.Contains(ItemKey(item)))
                    throw new ArgumentException("Equipment must match its slot and be owned by the character.");
            }
    }

    public string[] OwnedItemKeys => _owned.Order(StringComparer.Ordinal).ToArray();
    public Dictionary<string, Dictionary<EquipmentSlot, string>> EquippedBySpec =>
        _equipped.ToDictionary(pair => pair.Key, pair => new Dictionary<EquipmentSlot, string>(pair.Value));

    public static string ItemKey(Recommendation item) => item.Details is { } details
        ? $"item:{details.ClassicItemId}:{details.RequiredSuffix ?? ""}"
        : $"recommendation:{item.Id}";

    public bool IsOwned(string spec, string recommendationId) => _owned.Contains(ItemKey(Item(spec, recommendationId)));
    public bool IsEquipped(string spec, string recommendationId)
    {
        var item = Item(spec, recommendationId);
        return _equipped[spec].TryGetValue(item.Slot, out var id) && id == recommendationId;
    }

    public void SetOwned(string spec, string recommendationId, bool owned)
    {
        var key = ItemKey(Item(spec, recommendationId));
        if (owned) _owned.Add(key);
        else
        {
            _owned.Remove(key);
            foreach (var list in _equipped)
                foreach (var slot in list.Value.Where(pair => ItemKey(Item(list.Key, pair.Value)) == key).Select(pair => pair.Key).ToArray())
                    list.Value.Remove(slot);
        }
    }

    public void SetEquipped(string spec, string recommendationId, bool equipped)
    {
        var item = Item(spec, recommendationId);
        if (equipped)
        {
            _owned.Add(ItemKey(item));
            _equipped[spec][item.Slot] = item.Id;
        }
        else if (IsEquipped(spec, recommendationId)) _equipped[spec].Remove(item.Slot);
    }

    private Recommendation Item(string spec, string id) =>
        _lists.TryGetValue(spec, out var list) && list.TryGetValue(id, out var item)
            ? item : throw new ArgumentException("Unknown specialization or recommendation.");
}
