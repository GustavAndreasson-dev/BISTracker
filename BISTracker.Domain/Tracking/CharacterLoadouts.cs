namespace BISTracker.Domain;

// Ownership identifies a physical item variant; equipment identifies a recommendation in a spec.
public sealed class CharacterLoadouts
{
    private readonly Dictionary<string, Dictionary<string, Recommendation>> _lists;
    private readonly HashSet<string> _owned;
    private readonly Dictionary<string, Dictionary<EquipmentSlot, string>> _equipped;
    private readonly HashSet<string> _knownKeys;

    public CharacterLoadouts(CharacterClass characterClass,
        IReadOnlyDictionary<string, IReadOnlyList<Recommendation>> lists,
        IEnumerable<string> ownedItemKeys,
        IReadOnlyDictionary<string, Dictionary<EquipmentSlot, string>> equipped,
        IEnumerable<Recommendation>? inventoryItems = null)
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
        _knownKeys = _lists.Values.SelectMany(list => list.Values).Concat(inventoryItems ?? []).Select(ItemKey).ToHashSet(StringComparer.Ordinal);
        if (_owned.Any(key => !_knownKeys.Contains(key)))
            throw new ArgumentException("Ownership contains an unknown item variant.");
        foreach (var spec in specs)
            foreach (var entry in _equipped[spec.Id])
            {
                var item = Item(spec.Id, entry.Value);
                if (item.Slot != entry.Key || !_owned.Contains(ItemKey(item)))
                    throw new ArgumentException("Equipment must match its slot and be owned by the character.");
            }
        foreach (var spec in specs) ValidateEquipment(spec.Id);
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
        SetItemOwnership(key, owned);
    }

    public void SetItemOwnership(string key, bool owned)
    {
        if (!_knownKeys.Contains(key)) throw new ArgumentException("Unknown item variant.", nameof(key));
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
            var slots = _equipped[spec];
            // A boolean ownership entry represents one recorded copy, not an inventory quantity.
            foreach (var slot in slots.Where(pair => ItemKey(Item(spec, pair.Value)) == ItemKey(item)).Select(pair => pair.Key).ToArray())
                slots.Remove(slot);
            // Unique-equipped applies to the base item, including differently suffixed variants.
            var matching = slots.Where(pair => UniqueKey(Item(spec, pair.Value)) == UniqueKey(item)).ToArray();
            if (item.Details?.UniqueEquipped == true || matching.Any(pair => Item(spec, pair.Value).Details?.UniqueEquipped == true))
                foreach (var entry in matching) slots.Remove(entry.Key);
            if (item.Details?.WeaponKind == WeaponKind.TwoHanded) slots.Remove(EquipmentSlot.OffHand);
            if (item.Slot == EquipmentSlot.OffHand && slots.TryGetValue(EquipmentSlot.MainHand, out var main) &&
                Item(spec, main).Details?.WeaponKind == WeaponKind.TwoHanded) slots.Remove(EquipmentSlot.MainHand);
            _equipped[spec][item.Slot] = item.Id;
        }
        else if (IsEquipped(spec, recommendationId)) _equipped[spec].Remove(item.Slot);
    }

    private void ValidateEquipment(string spec)
    {
        var items = _equipped[spec].Values.Select(id => Item(spec, id)).ToArray();
        if (items.Any(item => item.Details?.WeaponKind == WeaponKind.TwoHanded) && _equipped[spec].ContainsKey(EquipmentSlot.OffHand))
            throw new ArgumentException("A two-handed weapon cannot be equipped with an off-hand item.");
        if (items.GroupBy(UniqueKey).Any(group => group.Count() > 1 && group.Any(item => item.Details?.UniqueEquipped == true)))
            throw new ArgumentException("A unique item cannot be equipped twice in one specialization.");
        if (items.GroupBy(ItemKey).Any(group => group.Count() > 1))
            throw new ArgumentException("Two equipped copies require separate inventory quantities, which this version does not record. The saved file has been preserved.");
    }

    private static string UniqueKey(Recommendation item) => item.Details is { } details
        ? $"item:{details.ClassicItemId}" : ItemKey(item);

    private Recommendation Item(string spec, string id) =>
        _lists.TryGetValue(spec, out var list) && list.TryGetValue(id, out var item)
            ? item : throw new ArgumentException("Unknown specialization or recommendation.");
}
