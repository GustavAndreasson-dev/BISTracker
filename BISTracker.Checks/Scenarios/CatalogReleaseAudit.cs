using System.Text.Json;
using BISTracker.Domain;
using BISTracker.Infrastructure;

namespace BISTracker.Checks.Scenarios;

internal static class CatalogReleaseAudit
{
    private static readonly string[] KnownAuditTypes = ["ForeverSlotRelease", "ClassicSlotRelease", "CatalogSlotRelease"];

    /// <summary>Audits the default catalog set of each version: Classic phase 1 and Forever level 30.</summary>
    public static async Task<int> RunAsync(string outputPath, GameVersion? onlyVersion = null)
    {
        var provider = new CharacterCatalog();
        var packs = provider.ReviewedPacks().ToDictionary(pack => pack.CatalogId, StringComparer.Ordinal);
        var versions = onlyVersion is { } selected ? [selected] : Enum.GetValues<GameVersion>();
        var rows = new List<object>();
        int blocked = 0, partial = 0, missing = 0;
        foreach (var version in versions)
            foreach (var characterClass in Enum.GetValues<CharacterClass>())
                foreach (var spec in CharacterDefinition.Specializations(characterClass))
                {
                    var catalog = await provider.LoadAsync(version, characterClass, spec.Id);
                    var catalogId = $"{catalog.Set!.Id}-{characterClass.ToString().ToLowerInvariant()}-{spec.Id}";
                    var status = packs.TryGetValue(catalogId, out var pack) ? pack.Status : "Missing";
                    if (status == "Partial") partial++;
                    if (status == "Missing") missing++;
                    var plan = catalog.EquipmentPlan;
                    var alliance = plan.CatalogCoverageFor(CharacterFaction.Alliance);
                    var horde = plan.CatalogCoverageFor(CharacterFaction.Horde);
                    var unknownFaction = catalog.Items.Where(item => item.Details?.AvailableFactions is { Count: 0 })
                        .Select(item => item.Details!.ClassicItemId).Distinct().Order().ToArray();
                    var ready = catalog.UnavailableReason is null && alliance.IsComplete && horde.IsComplete && unknownFaction.Length == 0;
                    if (!ready) blocked++;
                    rows.Add(new
                    {
                        gameVersion = version.ToString(), catalogId, catalogStatus = status,
                        characterClass = characterClass.ToString(), specializationId = spec.Id,
                        levelCap = catalog.Set.LevelCap, releaseStage = catalog.Set.ReleaseStage,
                        sourcePolicy = "Dungeon/Quest/Crafting", rawRecommendationRows = catalog.Items.Count,
                        slotGroupCount = plan.Goals.Count, allianceGoalCount = alliance.RequiredSlots.Count,
                        hordeGoalCount = horde.RequiredSlots.Count, weaponSetup = catalog.WeaponSetup.ToString(), catalog.WeaponSetupSourceUrl,
                        slotGroups = plan.Goals.Select(goal => new { slot = goal.Slot.ToString(), alternatives = goal.Alternatives.Count }).ToArray(),
                        exemptions = catalog.SlotExemptions ?? [],
                        allianceRequiredSlots = alliance.RequiredSlots.Select(slot => slot.ToString()).ToArray(),
                        allianceMissingSlots = alliance.MissingSlots.Select(slot => slot.ToString()).ToArray(),
                        hordeRequiredSlots = horde.RequiredSlots.Select(slot => slot.ToString()).ToArray(),
                        hordeMissingSlots = horde.MissingSlots.Select(slot => slot.ToString()).ToArray(),
                        unknownFactionItemIds = unknownFaction, releaseReady = ready,
                        recommendationSources = catalog.Items.Select(item => item.Details!.RecommendationUrl).Distinct().Order().ToArray()
                    });
                    Console.WriteLine($"{(ready ? "READY" : "BLOCKED")}: {version} {characterClass} {spec.Name} ({status}); " +
                        $"{alliance.RequiredSlots.Count}/{horde.RequiredSlots.Count} relevant Alliance/Horde goals; " +
                        $"Alliance missing [{string.Join(", ", alliance.MissingSlots)}]; Horde missing [{string.Join(", ", horde.MissingSlots)}]; " +
                        $"unknown quest faction IDs [{string.Join(", ", unknownFaction)}].");
                }
        if (File.Exists(outputPath))
        {
            using var existing = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            if (!existing.RootElement.TryGetProperty("auditType", out var type) || !KnownAuditTypes.Contains(type.GetString()))
                throw new InvalidDataException("Existing unrelated audit data has been preserved.");
        }
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(new
        {
            schemaVersion = 1, auditType = onlyVersion is { } only ? $"{only}SlotRelease" : "CatalogSlotRelease", reviewedOn = "2026-10-08",
            gameVersions = versions.Select(version => version.ToString()).ToArray(),
            releaseReady = blocked == 0, checkedCatalogs = rows.Count, blockedCatalogs = blocked,
            partialCatalogs = partial, missingCatalogs = missing,
            validationScope = "Slot combinations, variant capacity, unique, weapon plans and faction availability. Quest/profession source evidence and distribution checks are separate.",
            statusPolicy = "Partial is the pack's own source-review status and is reported, not blocking; a missing pack blocks.",
            quantityPolicy = "One recorded copy per item variant; unique applies to base item ID.", catalogs = rows
        }, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } }) + Environment.NewLine);
        Console.WriteLine($"{(blocked == 0 ? "PASS" : "BLOCKED")}: release audit; {rows.Count} catalogs checked, {blocked} incomplete, " +
            $"{missing} missing and {partial} partial catalogs.");
        return blocked == 0 ? 0 : 1;
    }
}
