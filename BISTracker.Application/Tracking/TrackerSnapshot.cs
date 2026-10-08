namespace BISTracker.Application;

public sealed record TrackerSnapshot(BisCatalog Catalog, IReadOnlyList<TrackerEntry> Entries, CharacterSelection? Selection = null);
