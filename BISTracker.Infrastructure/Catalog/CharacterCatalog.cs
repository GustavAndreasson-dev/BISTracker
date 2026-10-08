using BISTracker.Application;
using BISTracker.Domain;

namespace BISTracker.Infrastructure;

public sealed class CharacterCatalog : ICharacterCatalog
{
    private readonly ClassicPhaseOneBisCatalog _classicHoly = new();
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
        if (version == GameVersion.Forever)
            foreach (var pack in Packs().Values) sets[pack.Set.Id] = pack.Set;
        return Array.AsReadOnly(sets.Values.OrderBy(set => set.LevelCap).ThenBy(set => set.ReleaseStage, StringComparer.Ordinal).ToArray());
    }

    public Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string specializationId, CancellationToken cancellationToken = default) =>
        LoadAsync(version, characterClass, specializationId, CatalogSet.Default(version).Id, cancellationToken);

    public async Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string specializationId, string catalogSetId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var versionName = CharacterDefinition.VersionName(version);
        var spec = CharacterDefinition.Specialization(characterClass, specializationId);
        var set = CatalogSets(version).SingleOrDefault(item => item.Id == catalogSetId) ?? throw new ArgumentException("Unknown catalog set.");
        if (version == GameVersion.Classic && characterClass == CharacterClass.Priest && spec.Id == "holy")
            return (await _classicHoly.LoadAsync(cancellationToken).ConfigureAwait(false)) with { Set = set };
        if (version == GameVersion.Forever && Packs().TryGetValue($"{set.Id}-{characterClass.ToString().ToLowerInvariant()}-{spec.Id}", out var pack))
            return pack.Catalog;
        return new BisCatalog(new GameContext(versionName, $"{spec.Name} {characterClass}", set.Name),
            Array.Empty<Recommendation>(), false, "No reviewed BiS list available yet.", set);
    }

    public IReadOnlyCollection<string> CatalogIds() => Array.AsReadOnly(Packs().Keys.ToArray());

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
            foreach (var resource in assembly.GetManifestResourceNames().Where(name => name.StartsWith("BISTracker.Infrastructure.Catalog.Data.Forever.", StringComparison.Ordinal) && name.EndsWith(".json", StringComparison.Ordinal)))
            {
                using var stream = assembly.GetManifestResourceStream(resource)!;
                Add(ForeverCatalogReader.Read(stream, _includeOtherSources));
            }
            foreach (var path in paths)
            {
                using var stream = File.OpenRead(path);
                Add(ForeverCatalogReader.Read(stream, _includeOtherSources));
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
