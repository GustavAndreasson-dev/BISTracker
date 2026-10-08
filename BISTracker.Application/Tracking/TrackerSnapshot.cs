using BISTracker.Domain;

namespace BISTracker.Application;

public sealed record TrackerSnapshot(BisCatalog Catalog, IReadOnlyList<TrackerEntry> Entries, CharacterSelection? Selection = null)
{
    /// <summary>Catalog placeholder for a character workspace without characters. It has no items and no game context.</summary>
    public static BisCatalog NoCharacterCatalog { get; } = new(new GameContext("", "", ""), Array.Empty<Recommendation>(), false, "No characters.");

    /// <summary>Snapshot for a character workspace without characters: no selection and no entries.</summary>
    public static TrackerSnapshot NoCharacters { get; } = new(NoCharacterCatalog, Array.Empty<TrackerEntry>());
}
