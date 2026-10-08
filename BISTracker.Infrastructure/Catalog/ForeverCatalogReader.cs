using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BISTracker.Application;
using BISTracker.Domain;

namespace BISTracker.Infrastructure;

public sealed record ReviewedCatalogPack(string CatalogId, CatalogSet Set, CharacterClass Class, string SpecializationId, BisCatalog Catalog);

public static class ForeverCatalogReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    public static ReviewedCatalogPack Read(Stream stream, bool includeOtherSources = false)
    {
        CatalogDocument document;
        try { document = JsonSerializer.Deserialize<CatalogDocument>(stream, Options) ?? throw new JsonException("Empty catalog."); }
        catch (JsonException exception) { throw new InvalidDataException("Invalid Forever catalog JSON.", exception); }
        if (document.SchemaVersion != 1 || document.GameVersion != GameVersion.Forever ||
            document.LevelCap is not (30 or 60) || document.ReleaseStage is not ("Beta" or "Launch") ||
            !Enum.IsDefined(document.CharacterClass) || string.IsNullOrWhiteSpace(document.Phase) ||
            !IsHttps(document.SourceUrl) || string.IsNullOrWhiteSpace(document.SelectionMethod) ||
            document.Status is not ("Reviewed" or "Partial") || document.Items is not { Length: > 0 } ||
            !DateOnly.TryParseExact(document.ReviewedOn, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new InvalidDataException("Unsupported Forever catalog context or missing source review.");
        Specialization spec;
        try { spec = CharacterDefinition.Specialization(document.CharacterClass, document.SpecializationId); }
        catch (ArgumentException exception) { throw new InvalidDataException("Invalid catalog specialization.", exception); }
        var setId = $"forever-{document.ReleaseStage.ToLowerInvariant()}-level{document.LevelCap}";
        if (document.CatalogId != $"{setId}-{document.CharacterClass.ToString().ToLowerInvariant()}-{spec.Id}")
            throw new InvalidDataException("The catalog ID must identify its version, release stage, level, class and specialization.");
        var set = new CatalogSet(setId, GameVersion.Forever, document.LevelCap, document.ReleaseStage,
            $"Level {document.LevelCap} · {document.ReleaseStage}");
        var mapped = document.Items.Select(item => Map(item, document)).OrderBy(item => item.Slot).ThenBy(item => item.Name, StringComparer.Ordinal).ToArray();
        try { _ = new CharacterProgress(mapped); }
        catch (ArgumentException exception) { throw new InvalidDataException("Invalid or duplicated catalog recommendations.", exception); }
        var items = includeOtherSources ? mapped : mapped.Where(item => item.Details!.AcquisitionType is AcquisitionType.Dungeon or AcquisitionType.Quest or AcquisitionType.Crafting).ToArray();
        if (!Enum.IsDefined(document.WeaponSetup) || (document.WeaponSetup != WeaponSetup.Flexible && !IsHttps(document.WeaponSetupSourceUrl)) ||
            document.SlotExemptions is null || document.SlotExemptions.Any(value => value is null || !IsHttps(value.SourceUrl)))
            throw new InvalidDataException("Equipment-slot or weapon-setup exemptions need a reviewed source.");
        var exemptions = document.SlotExemptions.Select(value => new SlotExemption(value.Slot, value.Reason, value.SourceUrl)).ToArray();
        try { _ = new EquipmentPlan(items, exemptions, document.WeaponSetup); }
        catch (ArgumentException exception) { throw new InvalidDataException("Invalid equipment-slot coverage metadata.", exception); }
        var phase = document.Phase + (string.IsNullOrWhiteSpace(document.Patch) ? "" : $" · Patch {document.Patch}");
        var method = includeOtherSources ? "Guide alternatives · all reviewed sources" : "Guide alternatives · dungeons, quests & crafting";
        var catalog = new BisCatalog(new GameContext(CharacterDefinition.VersionName(GameVersion.Forever), $"{spec.Name} {document.CharacterClass}", phase),
            Array.AsReadOnly(items), false, items.Length == 0 ? "No reviewed items match the catalog source policy." : null, set, method,
            exemptions, document.WeaponSetup, document.WeaponSetupSourceUrl);
        return new ReviewedCatalogPack(document.CatalogId, set, document.CharacterClass, spec.Id, catalog);
    }

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
        return new Recommendation($"{document.CatalogId}-{item.Id}", item.Slot, name, item.Source, item.Note,
            new ItemDetails(item.ItemId, item.RequiredSuffix, item.AcquisitionType, item.IconUrl ?? "", item.ItemUrl,
                item.RecommendationUrl, item.WeaponKind, item.UniqueEquipped,
                item.AvailableFactions ?? (item.AcquisitionType == AcquisitionType.Quest ? [] : Enum.GetValues<CharacterFaction>())));
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
