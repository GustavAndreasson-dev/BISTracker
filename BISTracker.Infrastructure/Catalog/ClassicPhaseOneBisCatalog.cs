using BISTracker.Application;
using BISTracker.Domain;

namespace BISTracker.Infrastructure;

/// <summary>
/// The single-list view used by the 1.0.0 tracker and its legacy progress file: the rows of the built-in
/// Classic Holy Priest pack that keep a 1.0.0 recommendation ID. No separate catalog data exists.
/// </summary>
public sealed class ClassicPhaseOneBisCatalog : IBisCatalog
{
    private static readonly Lazy<BisCatalog> Catalog = new(ReadCatalog);

    public Task<BisCatalog> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Catalog.Value);
    }

    private static BisCatalog ReadCatalog()
    {
        using var stream = typeof(ClassicPhaseOneBisCatalog).Assembly.GetManifestResourceStream(
            "BISTracker.Infrastructure.Catalog.Data.Classic.phase1.priest-holy.json")
            ?? throw new InvalidDataException("The Classic Holy Priest catalog is missing.");
        var catalog = ReviewedCatalogReader.Read(stream).Catalog;
        var legacy = catalog.Items.Where(item => item.Id.StartsWith(ReviewedCatalogReader.LegacyRecommendationPrefix, StringComparison.Ordinal)).ToArray();
        if (legacy.Length != 17 || legacy.Select(item => item.Slot).Distinct().Count() != 17)
            throw new InvalidDataException("The Classic Holy Priest catalog must keep one 1.0.0 target for each of the 17 equipment slots.");
        return catalog with { Items = Array.AsReadOnly(legacy) };
    }
}
