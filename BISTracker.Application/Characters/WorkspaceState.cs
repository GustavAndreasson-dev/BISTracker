using BISTracker.Domain;

namespace BISTracker.Application;

public sealed record CharacterState(Guid Id, string Name, GameVersion Version, CharacterClass Class,
    string SelectedSpecialization, string[] OwnedItemKeys, Dictionary<string, Dictionary<EquipmentSlot, string>> EquippedBySpec,
    string? CatalogSetId = null, Dictionary<string, Dictionary<string, Dictionary<EquipmentSlot, string>>>? ArchivedLoadouts = null);

public sealed record WorkspaceState(int SchemaVersion, Guid ActiveCharacterId, CharacterState[] Characters);

public sealed record CharacterOption(Guid Id, string Name, GameVersion Version, CharacterClass Class, string SelectedSpecialization)
{
    public string DisplayName => $"{Name} · {(Version == GameVersion.Classic ? "Classic" : "Forever")}";
}

public sealed record CharacterSelection(IReadOnlyList<CharacterOption> Characters, CharacterOption ActiveCharacter, Specialization ActiveSpecialization,
    IReadOnlyList<CatalogSet>? CatalogSets = null, CatalogSet? ActiveCatalogSet = null);
