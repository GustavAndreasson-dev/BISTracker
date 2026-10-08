using BISTracker.Application;
using BISTracker.Domain;

namespace BISTracker.Infrastructure;

public sealed class CharacterCatalog : ICharacterCatalog
{
    private const string EmbeddedPrefix = "BISTracker.Infrastructure.Catalog.Data.";
    // Classic phase 1 packs are built in only; Forever packs may also be imported.
    private static readonly string[] EmbeddedFolders = ["Classic.", "Forever."];
    private readonly string? _importDirectory;
    private readonly bool _includeOtherSources;
    private readonly object _gate = new();
    private string? _fingerprint;
    private Dictionary<string, ReviewedCatalogPack>? _packs;

    public CharacterCatalog(string? importDirectory = null, bool includeOtherSources = false)
    {
        _importDirectory = importDirectory is null ? null : Path.GetFullPath(importDirectory);
        _includeOtherSources = includeOtherSources;
    }

    public IReadOnlyList<CatalogSet> CatalogSets(GameVersion version)
    {
        var sets = CatalogSet.Defaults(version).ToDictionary(set => set.Id);
        foreach (var pack in Packs().Values.Where(pack => pack.Set.Version == version)) sets[pack.Set.Id] = pack.Set;
        return Array.AsReadOnly(sets.Values.OrderBy(set => set.LevelCap).ThenBy(set => set.ReleaseStage, StringComparer.Ordinal).ToArray());
    }

    public Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string specializationId, CancellationToken cancellationToken = default) =>
        LoadAsync(version, characterClass, specializationId, CatalogSet.Default(version).Id, cancellationToken);

    public Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string specializationId, string catalogSetId,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<BisCatalog>(cancellationToken);
        try { return Task.FromResult(Load(version, characterClass, specializationId, catalogSetId)); }
        catch (Exception exception) { return Task.FromException<BisCatalog>(exception); }
    }

    private BisCatalog Load(GameVersion version, CharacterClass characterClass, string specializationId, string catalogSetId)
    {
        var versionName = CharacterDefinition.VersionName(version);
        var spec = CharacterDefinition.Specialization(characterClass, specializationId);
        var set = CatalogSets(version).SingleOrDefault(item => item.Id == catalogSetId) ?? throw new ArgumentException("Unknown catalog set.");
        // Every built-in Classic spec and every Forever set use the same <setId>-<class>-<spec> pack identity.
        if (Packs().TryGetValue($"{set.Id}-{characterClass.ToString().ToLowerInvariant()}-{spec.Id}", out var pack) && pack.Set.Version == version)
            return pack.Catalog;
        return new BisCatalog(new GameContext(versionName, $"{spec.Name} {characterClass}", set.Name),
            Array.Empty<Recommendation>(), false, "No reviewed BiS list available yet.", set);
    }

    public IReadOnlyCollection<string> CatalogIds() => Array.AsReadOnly(Packs().Keys.ToArray());

    /// <summary>All loaded reviewed packs, including their review status, for release audits.</summary>
    public IReadOnlyList<ReviewedCatalogPack> ReviewedPacks() => Array.AsReadOnly(Packs().Values.OrderBy(pack => pack.CatalogId, StringComparer.Ordinal).ToArray());

    private Dictionary<string, ReviewedCatalogPack> Packs()
    {
        lock (_gate)
        {
            var paths = _importDirectory is not null && Directory.Exists(_importDirectory)
                ? Directory.GetFiles(_importDirectory, "*.json", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray() : [];
            var fingerprint = string.Join("|", paths.Select(path => { var file = new FileInfo(path); return $"{path}:{file.Length}:{file.LastWriteTimeUtc.Ticks}"; }));
            if (_packs is not null && fingerprint == _fingerprint) return _packs;
            var packs = new Dictionary<string, ReviewedCatalogPack>(StringComparer.Ordinal);
            var assembly = typeof(CharacterCatalog).Assembly;
            foreach (var resource in assembly.GetManifestResourceNames().Where(name => EmbeddedFolders.Any(folder =>
                name.StartsWith(EmbeddedPrefix + folder, StringComparison.Ordinal)) && name.EndsWith(".json", StringComparison.Ordinal)).Order(StringComparer.Ordinal))
            {
                using var stream = assembly.GetManifestResourceStream(resource)!;
                var pack = ReviewedCatalogReader.Read(stream, _includeOtherSources);
                if (!resource.StartsWith($"{EmbeddedPrefix}{pack.Set.Version}.", StringComparison.Ordinal))
                    throw new InvalidDataException("A built-in catalog is stored under the wrong game version.");
                Add(pack);
            }
            foreach (var path in paths)
            {
                using var stream = File.OpenRead(path);
                var pack = ReviewedCatalogReader.Read(stream, _includeOtherSources);
                if (pack.Set.Version != GameVersion.Forever)
                    throw new InvalidDataException("Imported catalogs must be Forever catalogs. Classic catalogs are built in.");
                Add(pack);
            }
            _packs = packs;
            _fingerprint = fingerprint;
            return packs;
            void Add(ReviewedCatalogPack pack)
            {
                if (!packs.TryAdd(pack.CatalogId, pack)) throw new InvalidDataException("Duplicate catalog ID. Existing catalogs must not be overwritten.");
            }
        }
    }
}
