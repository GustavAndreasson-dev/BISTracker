namespace BISTracker.Domain;

public sealed record CatalogSet(string Id, GameVersion Version, int LevelCap, string ReleaseStage, string Name)
{
    public static CatalogSet ClassicPhaseOne { get; } = new("classic-phase1-level60", GameVersion.Classic, 60, "Classic", "Level 60 · Phase 1");
    public static CatalogSet ForeverLevelThirty { get; } = new("forever-beta-level30", GameVersion.Forever, 30, "Beta", "Level 30 · Beta");
    public static CatalogSet ForeverLevelSixty { get; } = new("forever-launch-level60", GameVersion.Forever, 60, "Launch", "Level 60 · Awaiting data");
    public static CatalogSet Default(GameVersion version) => version switch
    {
        GameVersion.Classic => ClassicPhaseOne,
        GameVersion.Forever => ForeverLevelThirty,
        _ => throw new ArgumentException("Unknown game version.", nameof(version))
    };
    public static IReadOnlyList<CatalogSet> Defaults(GameVersion version) => version == GameVersion.Classic
        ? Array.AsReadOnly(new[] { ClassicPhaseOne })
        : Array.AsReadOnly(new[] { Default(version), ForeverLevelSixty });
}
