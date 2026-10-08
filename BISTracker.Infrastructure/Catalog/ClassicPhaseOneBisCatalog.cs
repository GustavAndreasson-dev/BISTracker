using System.Text.Json;
using System.Text.Json.Serialization;
using BISTracker.Application;
using BISTracker.Domain;

namespace BISTracker.Infrastructure;

/// <summary>A reviewed, embedded snapshot. No network requests are needed to load the catalog.</summary>
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
            "BISTracker.Infrastructure.Catalog.Data.holy-priest-classic-phase1.json")
            ?? throw new InvalidDataException("The Phase 1 item catalog is missing.");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
        };
        CatalogDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CatalogDocument>(stream, options)
                ?? throw new JsonException("The catalog is empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The Phase 1 item catalog contains invalid data.", exception);
        }

        if (document.SchemaVersion != 1 || document.Context is null || document.Items is null ||
            document.Context.Version != "WoW Classic (Vanilla)" || document.Context.Specialization != "Holy Priest" ||
            document.Context.Phase != "Phase 1 · dungeons and quests")
            throw new InvalidDataException("The item catalog has an unsupported version or game context.");

        var items = document.Items.Select(Map).OrderBy(item => item.Slot).ToArray();
        if (items.Length != 17 || items.Select(item => item.Slot).Distinct().Count() != 17)
            throw new InvalidDataException("The Phase 1 catalog must cover all 17 equipment slots exactly once.");
        _ = new CharacterProgress(items); // Also validates slot values and duplicate tracking identities.
        return new BisCatalog(document.Context, Array.AsReadOnly(items), IsSample: false);
    }

    private static Recommendation Map(CatalogItem item)
    {
        if (item is null || item.ItemId <= 0 || string.IsNullOrWhiteSpace(item.Name) ||
            string.IsNullOrWhiteSpace(item.Source) || item.Note is null ||
            item.AcquisitionType is not (AcquisitionType.Dungeon or AcquisitionType.Quest) || !Enum.IsDefined(item.Slot) ||
            (item.RequiredSuffix is not null && item.RequiredSuffix != "of Healing") ||
            !IsHttps(item.IconUrl) || !IsHttps(item.ItemUrl) || !IsHttps(item.RecommendationUrl))
            throw new InvalidDataException("The Phase 1 catalog contains an invalid item or acquisition source.");

        var suffixKey = item.RequiredSuffix is null ? "" : "-of-healing";
        var name = item.RequiredSuffix is null ? item.Name : $"{item.Name} {item.RequiredSuffix}";
        return new Recommendation($"classic-p1-item-{item.ItemId}{suffixKey}", item.Slot, name, item.Source, item.Note,
            new ItemDetails(item.ItemId, item.RequiredSuffix, item.AcquisitionType,
                item.IconUrl, item.ItemUrl, item.RecommendationUrl));
    }

    private static bool IsHttps(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps;

    private sealed record CatalogDocument
    {
        public required int SchemaVersion { get; init; }
        public required GameContext Context { get; init; }
        public required CatalogItem[] Items { get; init; }
    }

    private sealed record CatalogItem
    {
        public required int ItemId { get; init; }
        public required string Name { get; init; }
        public required EquipmentSlot Slot { get; init; }
        public required string Source { get; init; }
        public required string Note { get; init; }
        public string? RequiredSuffix { get; init; }
        public required AcquisitionType AcquisitionType { get; init; }
        public required string IconUrl { get; init; }
        public required string ItemUrl { get; init; }
        public required string RecommendationUrl { get; init; }
    }
}
