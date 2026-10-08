using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BISTracker.Application;
using BISTracker.Domain;

namespace BISTracker.Infrastructure;

public sealed record ReviewedCatalogPack(string CatalogId, CatalogSet Set, CharacterClass Class, string SpecializationId, BisCatalog Catalog, string Status);

/// <summary>Reads source-reviewed schema 1 catalog packs for Classic phase 1 and Forever.</summary>
public static class ReviewedCatalogReader
{
    public const string ClassicPhase = "Phase 1 · pre-raid";
    // Only the Holy Priest pack shipped in 1.0.0 may keep its saved recommendation IDs (DEC-016).
    public const string LegacyRecommendationCatalogId = "classic-phase1-level60-priest-holy";
    public const string LegacyRecommendationPrefix = "classic-p1-item-";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    public static ReviewedCatalogPack Read(Stream stream, bool includeOtherSources = false)
    {
        CatalogDocument document;
        try { document = JsonSerializer.Deserialize<CatalogDocument>(stream, Options) ?? throw new JsonException("Empty catalog."); }
        catch (JsonException exception) { throw new InvalidDataException("Invalid catalog JSON.", exception); }
        if (document.SchemaVersion != 1 || !Enum.IsDefined(document.CharacterClass) || string.IsNullOrWhiteSpace(document.Phase) ||
            !IsHttps(document.SourceUrl) || string.IsNullOrWhiteSpace(document.SelectionMethod) ||
            document.Status is not ("Reviewed" or "Partial") || document.Items is not { Length: > 0 } ||
            !DateOnly.TryParseExact(document.ReviewedOn, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new InvalidDataException("Unsupported catalog schema or missing source review.");
        var set = SetFor(document) ?? throw new InvalidDataException("Unsupported catalog game version, level, release stage or phase.");
        Specialization spec;
        try { spec = CharacterDefinition.Specialization(document.CharacterClass, document.SpecializationId); }
        catch (ArgumentException exception) { throw new InvalidDataException("Invalid catalog specialization.", exception); }
        if (document.CatalogId != $"{set.Id}-{document.CharacterClass.ToString().ToLowerInvariant()}-{spec.Id}")
            throw new InvalidDataException("The catalog ID must identify its version, release stage, level, class and specialization.");
        if (document.Items.Any(item => item is null) ||
            document.Items.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != document.Items.Length)
            throw new InvalidDataException("Catalog rows must exist and have unique row IDs.");
        var mapped = document.Items.Select(item => Map(item, document)).OrderBy(item => item.Slot).ThenBy(item => item.Name, StringComparer.Ordinal).ToArray();
        try { _ = new CharacterProgress(mapped); }
        catch (ArgumentException exception) { throw new InvalidDataException("Invalid or duplicated catalog recommendations.", exception); }
        var items = includeOtherSources ? mapped : mapped.Where(item => item.Details!.AcquisitionType is AcquisitionType.Dungeon or AcquisitionType.Quest or AcquisitionType.Crafting).ToArray();
        if (!includeOtherSources)
            items = items.Where(item => document.WeaponSetup switch
            {
                WeaponSetup.TwoHanded => item.Slot != EquipmentSlot.OffHand &&
                    (item.Slot != EquipmentSlot.MainHand || item.Details!.WeaponKind == WeaponKind.TwoHanded),
                WeaponSetup.OneHandAndOffHand => item.Details!.WeaponKind != WeaponKind.TwoHanded,
                _ => true
            }).ToArray();
        if (!Enum.IsDefined(document.WeaponSetup) || (document.WeaponSetup != WeaponSetup.Flexible && !IsHttps(document.WeaponSetupSourceUrl)) ||
            document.SlotExemptions is null || document.SlotExemptions.Any(value => value is null || !IsHttps(value.SourceUrl)))
            throw new InvalidDataException("Equipment-slot or weapon-setup exemptions need a reviewed source.");
        var exemptions = document.SlotExemptions.Select(value => new SlotExemption(value.Slot, value.Reason, value.SourceUrl)).ToArray();
        try { _ = new EquipmentPlan(items, exemptions, document.WeaponSetup); }
        catch (ArgumentException exception) { throw new InvalidDataException("Invalid equipment-slot coverage metadata.", exception); }
        var phase = document.Phase + (string.IsNullOrWhiteSpace(document.Patch) ? "" : $" · Patch {document.Patch}");
        var method = includeOtherSources ? "Guide alternatives · all reviewed sources" : "Guide alternatives · dungeons, quests & crafting";
        var catalog = new BisCatalog(new GameContext(CharacterDefinition.VersionName(document.GameVersion), $"{spec.Name} {document.CharacterClass}", phase),
            Array.AsReadOnly(items), false, items.Length == 0 ? "No reviewed items match the catalog source policy." : null, set, method,
            exemptions, document.WeaponSetup, document.WeaponSetupSourceUrl);
        return new ReviewedCatalogPack(document.CatalogId, set, document.CharacterClass, spec.Id, catalog, document.Status);
    }

    private static CatalogSet? SetFor(CatalogDocument document) => document.GameVersion switch
    {
        GameVersion.Classic when document.LevelCap == CatalogSet.ClassicPhaseOne.LevelCap &&
            document.ReleaseStage == CatalogSet.ClassicPhaseOne.ReleaseStage && document.Phase == ClassicPhase => CatalogSet.ClassicPhaseOne,
        GameVersion.Forever when document.LevelCap is 30 or 60 && document.ReleaseStage is "Beta" or "Launch" =>
            new CatalogSet($"forever-{document.ReleaseStage.ToLowerInvariant()}-level{document.LevelCap}", GameVersion.Forever,
                document.LevelCap, document.ReleaseStage, $"Level {document.LevelCap} · {document.ReleaseStage}"),
        _ => null
    };

    private static Recommendation Map(CatalogItem item, CatalogDocument document)
    {
        if (item is null || item.ItemId <= 0 || string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Name) ||
            string.IsNullOrWhiteSpace(item.Source) || item.Note is null || !Enum.IsDefined(item.Slot) ||
            !Enum.IsDefined(item.AcquisitionType) || !Enum.IsDefined(item.WeaponKind) || !IsHttps(item.ItemUrl) || !IsHttps(item.RecommendationUrl) ||
            (item.IconUrl is not null && !IsHttps(item.IconUrl)) || item.RequiredLevel is < 0 || item.RequiredLevel > document.LevelCap ||
            (item.RequiredSuffix is not null && string.IsNullOrWhiteSpace(item.RequiredSuffix)) ||
            (item.AvailableFactions is not null && (item.AvailableFactions.Any(value => !Enum.IsDefined(value)) || item.AvailableFactions.Distinct().Count() != item.AvailableFactions.Length)))
            throw new InvalidDataException("Invalid catalog item, source, reference or level requirement.");
        if ((item.WeaponKind == WeaponKind.TwoHanded && item.Slot != EquipmentSlot.MainHand) ||
            (item.WeaponKind == WeaponKind.OffHand && item.Slot != EquipmentSlot.OffHand) ||
            (item.WeaponKind == WeaponKind.MainHand && item.Slot != EquipmentSlot.MainHand) ||
            (item.WeaponKind == WeaponKind.OneHanded && item.Slot is not (EquipmentSlot.MainHand or EquipmentSlot.OffHand)))
            throw new InvalidDataException("Weapon type does not match its recommendation slot.");
        var name = item.RequiredSuffix is null || item.Name.EndsWith(item.RequiredSuffix, StringComparison.OrdinalIgnoreCase)
            ? item.Name : $"{item.Name} {item.RequiredSuffix}";
        return new Recommendation(RecommendationId(item, document), item.Slot, name, item.Source, item.Note,
            new ItemDetails(item.ItemId, item.RequiredSuffix, item.AcquisitionType, item.IconUrl ?? "", item.ItemUrl,
                item.RecommendationUrl, item.WeaponKind, item.UniqueEquipped,
                item.AvailableFactions ?? (item.AcquisitionType == AcquisitionType.Quest ? [] : Enum.GetValues<CharacterFaction>())));
    }

    private static string RecommendationId(CatalogItem item, CatalogDocument document)
    {
        if (document.GameVersion == GameVersion.Forever)
        {
            if (item.RecommendationId is not null) throw new InvalidDataException("Forever catalogs cannot set a recommendation ID.");
            // Forever packs share raw row IDs across levels; the catalog ID keeps their saved identities apart.
            return $"{document.CatalogId}-{item.Id}";
        }
        // Classic row IDs are already context-qualified: <catalogId>-<slot>-<itemId>, optionally with a lowercase disambiguator.
        var rowPattern = $"^{Regex.Escape(document.CatalogId)}-{item.Slot.ToString().ToLowerInvariant()}-{item.ItemId}(-[a-z0-9]+)*$";
        if (!Regex.IsMatch(item.Id, rowPattern, RegexOptions.CultureInvariant))
            throw new InvalidDataException("A Classic row ID must be <catalogId>-<slot>-<itemId>.");
        if (item.RecommendationId is null) return item.Id;
        var legacy = $"{LegacyRecommendationPrefix}{item.ItemId}" + item.RequiredSuffix switch
        {
            null => "",
            "of Healing" => "-of-healing",
            _ => throw new InvalidDataException("Saved 1.0.0 recommendation IDs only exist for base items and of Healing variants.")
        };
        if (document.CatalogId != LegacyRecommendationCatalogId || item.RecommendationId != legacy)
            throw new InvalidDataException("A recommendation ID may only preserve a 1.0.0 Holy Priest row ID for the same item variant.");
        return item.RecommendationId;
    }

    private static bool IsHttps(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    private sealed record CatalogDocument
    {
        public required int SchemaVersion { get; init; }
        public required string CatalogId { get; init; }
        public required GameVersion GameVersion { get; init; }
        public required CharacterClass CharacterClass { get; init; }
        public required string SpecializationId { get; init; }
        public required int LevelCap { get; init; }
        public required string ReleaseStage { get; init; }
        public required string Phase { get; init; }
        public string? Patch { get; init; }
        public required string SourceUrl { get; init; }
        public required string ReviewedOn { get; init; }
        public required string SelectionMethod { get; init; }
        public required string Status { get; init; }
        public required CatalogItem[] Items { get; init; }
        public SlotExemptionDocument[] SlotExemptions { get; init; } = [];
        public WeaponSetup WeaponSetup { get; init; }
        public string? WeaponSetupSourceUrl { get; init; }
    }
    private sealed record SlotExemptionDocument
    {
        public required EquipmentSlot Slot { get; init; }
        public required string Reason { get; init; }
        public required string SourceUrl { get; init; }
    }
    private sealed record CatalogItem
    {
        public required string Id { get; init; }
        public string? RecommendationId { get; init; }
        public required EquipmentSlot Slot { get; init; }
        public required int ItemId { get; init; }
        public required string Name { get; init; }
        public string? RequiredSuffix { get; init; }
        public int? RequiredLevel { get; init; }
        public required AcquisitionType AcquisitionType { get; init; }
        public required string Source { get; init; }
        public required string Note { get; init; }
        public string? IconUrl { get; init; }
        public required string ItemUrl { get; init; }
        public required string RecommendationUrl { get; init; }
        public required WeaponKind WeaponKind { get; init; }
        public required bool UniqueEquipped { get; init; }
        public CharacterFaction[]? AvailableFactions { get; init; }
    }
}
