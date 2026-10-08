namespace BISTracker.Domain;

public enum AcquisitionType { Dungeon, Quest, Crafting, WorldDrop, Vendor, PvP, Reputation }
public enum WeaponKind { None, OneHanded, MainHand, OffHand, TwoHanded }
public enum CharacterFaction { Alliance, Horde }

/// <summary>Item identity and reference facts, independent of any database API.</summary>
public sealed record ItemDetails(int ClassicItemId, string? RequiredSuffix, AcquisitionType AcquisitionType,
    string IconUrl, string ItemUrl, string RecommendationUrl, WeaponKind WeaponKind = WeaponKind.None, bool UniqueEquipped = false,
    IReadOnlyList<CharacterFaction>? AvailableFactions = null);
