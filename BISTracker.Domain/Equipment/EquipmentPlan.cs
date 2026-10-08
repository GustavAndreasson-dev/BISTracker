namespace BISTracker.Domain;

public enum WeaponSetup { Flexible, TwoHanded, OneHandAndOffHand }

public sealed record SlotExemption(EquipmentSlot Slot, string Reason, string SourceUrl);
public sealed record EquipmentGoal(EquipmentSlot Slot, IReadOnlyList<Recommendation> Alternatives);
public sealed record EquipmentCoverage(IReadOnlyList<EquipmentSlot> RequiredSlots, IReadOnlyList<EquipmentSlot> CoveredSlots)
{
    public IReadOnlyList<EquipmentSlot> MissingSlots => RequiredSlots.Except(CoveredSlots).ToArray();
    public bool IsComplete => MissingSlots.Count == 0;
}

// A recommendation is an option for a slot, never an additional equipment goal.
public sealed class EquipmentPlan
{
    private readonly Recommendation[] _items;
    private readonly EquipmentSlot[] _slots;
    private readonly WeaponSetup _weaponSetup;
    private readonly HashSet<int> _uniqueIds;

    public EquipmentPlan(IEnumerable<Recommendation> items, IEnumerable<SlotExemption>? exemptions = null,
        WeaponSetup weaponSetup = WeaponSetup.Flexible)
    {
        _items = items.ToArray();
        _ = new CharacterProgress(_items);
        var excluded = (exemptions ?? []).ToArray();
        if (!Enum.IsDefined(weaponSetup) || excluded.Select(value => value.Slot).Distinct().Count() != excluded.Length ||
            excluded.Any(value => !Enum.IsDefined(value.Slot) || string.IsNullOrWhiteSpace(value.Reason) ||
                string.IsNullOrWhiteSpace(value.SourceUrl) || _items.Any(item => item.Slot == value.Slot)))
            throw new ArgumentException("Invalid or contradictory equipment-slot exemption.");
        _weaponSetup = weaponSetup;
        _slots = Enum.GetValues<EquipmentSlot>().Except(excluded.Select(value => value.Slot)).ToArray();
        _uniqueIds = _items.Where(item => item.Details?.UniqueEquipped == true).Select(item => item.Details!.ClassicItemId).ToHashSet();
        Goals = _slots.Select(slot => new EquipmentGoal(slot, Array.AsReadOnly(_items.Where(item => item.Slot == slot).ToArray()))).ToArray();
    }

    public IReadOnlyList<EquipmentGoal> Goals { get; }
    public EquipmentCoverage CatalogCoverage => Cover(_items);
    public EquipmentCoverage CatalogCoverageFor(CharacterFaction faction)
    {
        if (!Enum.IsDefined(faction)) throw new ArgumentException("Unknown faction.");
        return Cover(_items.Where(item => item.Details?.AvailableFactions is not { } factions || factions.Contains(faction)).ToArray());
    }

    // Ownership currently records one known copy per physical item variant. Do not count
    // the same ring/trinket/weapon twice merely because it occurs in two recommendation rows.
    public EquipmentCoverage OwnedCoverage(IEnumerable<string> ownedKeys, bool useTwoHandedWeapon = false)
    {
        var keys = ownedKeys.ToHashSet(StringComparer.Ordinal);
        return Cover(_items.Where(item => keys.Contains(CharacterLoadouts.ItemKey(item))).ToArray(), useTwoHandedWeapon);
    }

    private EquipmentCoverage Cover(Recommendation[] available, bool useTwoHandedWeapon = false)
    {
        var mains = available.Where(item => item.Slot == EquipmentSlot.MainHand).ToArray();
        var offs = available.Where(item => item.Slot == EquipmentSlot.OffHand).ToArray();
        var modes = new List<(Recommendation? Main, Recommendation? Off, bool TwoHanded)>();
        // A missing hand choice remains a missing goal; alternatives are not silently dropped.
        modes.Add((null, null, _weaponSetup == WeaponSetup.TwoHanded || useTwoHandedWeapon));
        foreach (var main in mains)
        {
            var twoHanded = main.Details?.WeaponKind == WeaponKind.TwoHanded;
            if ((_weaponSetup == WeaponSetup.TwoHanded && !twoHanded) ||
                (_weaponSetup == WeaponSetup.OneHandAndOffHand && twoHanded) || (useTwoHandedWeapon && !twoHanded)) continue;
            if (twoHanded) modes.Add((main, null, true));
            else
            {
                modes.Add((main, null, false));
                foreach (var off in offs.Where(off => CapacityKey(off) != CapacityKey(main))) modes.Add((main, off, false));
            }
        }
        if (_weaponSetup != WeaponSetup.TwoHanded && !useTwoHandedWeapon)
            foreach (var off in offs) modes.Add((null, off, false));

        EquipmentCoverage? best = null;
        foreach (var mode in modes)
        {
            var required = _slots.Where(slot => slot != EquipmentSlot.OffHand || !mode.TwoHanded).ToArray();
            var reserved = new HashSet<string>(StringComparer.Ordinal);
            var covered = new HashSet<EquipmentSlot>();
            foreach (var hand in new[] { mode.Main, mode.Off }.OfType<Recommendation>())
            {
                reserved.Add(CapacityKey(hand));
                covered.Add(hand.Slot);
            }
            var matching = new Dictionary<string, EquipmentSlot>(StringComparer.Ordinal);
            var options = required.Where(slot => slot is not (EquipmentSlot.MainHand or EquipmentSlot.OffHand))
                .ToDictionary(slot => slot, slot => available.Where(item => item.Slot == slot).Select(CapacityKey).Distinct().ToArray());
            foreach (var slot in options.Keys) Match(slot, new HashSet<string>(StringComparer.Ordinal));
            covered.UnionWith(matching.Values);
            var candidate = new EquipmentCoverage(required, required.Where(covered.Contains).ToArray());
            if (best is null || candidate.MissingSlots.Count < best.MissingSlots.Count ||
                (candidate.MissingSlots.Count == best.MissingSlots.Count && candidate.CoveredSlots.Count > best.CoveredSlots.Count)) best = candidate;

            bool Match(EquipmentSlot slot, HashSet<string> visited)
            {
                foreach (var key in options[slot])
                {
                    if (reserved.Contains(key) || !visited.Add(key)) continue;
                    if (!matching.TryGetValue(key, out var previous) || Match(previous, visited))
                    {
                        matching[key] = slot;
                        return true;
                    }
                }
                return false;
            }
        }
        return best!;
    }

    private string CapacityKey(Recommendation item) => item.Details is { } details && _uniqueIds.Contains(details.ClassicItemId)
        ? $"unique:{details.ClassicItemId}" : CharacterLoadouts.ItemKey(item);
}
