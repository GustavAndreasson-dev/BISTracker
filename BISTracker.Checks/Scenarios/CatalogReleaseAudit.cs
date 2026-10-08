using System.Text.Json;
using BISTracker.Domain;
using BISTracker.Infrastructure;

namespace BISTracker.Checks.Scenarios;

internal static class CatalogReleaseAudit
{
    public static async Task<int> RunAsync(string outputPath)
    {
        var provider = new CharacterCatalog();
        var rows = new List<object>();
        var blocked = 0;
        foreach (var characterClass in Enum.GetValues<CharacterClass>())
            foreach (var spec in CharacterDefinition.Specializations(characterClass))
            {
                var catalog = await provider.LoadAsync(GameVersion.Forever, characterClass, spec.Id);
                var plan = catalog.EquipmentPlan;
                var alliance = plan.CatalogCoverageFor(CharacterFaction.Alliance);
                var horde = plan.CatalogCoverageFor(CharacterFaction.Horde);
                var unknownFaction = catalog.Items.Where(item => item.Details?.AvailableFactions is { Count: 0 })
                    .Select(item => item.Details!.ClassicItemId).Distinct().Order().ToArray();
                var ready = catalog.UnavailableReason is null && alliance.IsComplete && horde.IsComplete && unknownFaction.Length == 0;
                if (!ready) blocked++;
                rows.Add(new
                {
                    characterClass = characterClass.ToString(), specializationId = spec.Id,
                    levelCap = catalog.Set!.LevelCap, releaseStage = catalog.Set.ReleaseStage,
                    sourcePolicy = "Dungeon/Quest", rawRecommendationRows = catalog.Items.Count,
                    goalCount = plan.Goals.Count, weaponSetup = catalog.WeaponSetup.ToString(), catalog.WeaponSetupSourceUrl,
                    slotGroups = plan.Goals.Select(goal => new { slot = goal.Slot.ToString(), alternatives = goal.Alternatives.Count }).ToArray(),
                    exemptions = catalog.SlotExemptions ?? [],
                    allianceRequiredSlots = alliance.RequiredSlots.Select(slot => slot.ToString()).ToArray(),
                    allianceMissingSlots = alliance.MissingSlots.Select(slot => slot.ToString()).ToArray(),
                    hordeRequiredSlots = horde.RequiredSlots.Select(slot => slot.ToString()).ToArray(),
                    hordeMissingSlots = horde.MissingSlots.Select(slot => slot.ToString()).ToArray(),
                    unknownFactionItemIds = unknownFaction, releaseReady = ready,
                    recommendationSources = catalog.Items.Select(item => item.Details!.RecommendationUrl).Distinct().Order().ToArray()
                });
                Console.WriteLine($"{(ready ? "READY" : "BLOCKED")}: {characterClass} {spec.Name}; {plan.Goals.Count} slot goals; " +
                    $"Alliance missing [{string.Join(", ", alliance.MissingSlots)}]; Horde missing [{string.Join(", ", horde.MissingSlots)}]; " +
                    $"unknown quest faction IDs [{string.Join(", ", unknownFaction)}].");
            }
        if (File.Exists(outputPath))
        {
            using var existing = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            if (!existing.RootElement.TryGetProperty("auditType", out var type) || type.GetString() != "ForeverSlotRelease")
                throw new InvalidDataException("Existing unrelated audit data has been preserved.");
        }
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(new
        {
            schemaVersion = 1, auditType = "ForeverSlotRelease", reviewedOn = "2026-10-08",
            releaseReady = blocked == 0, checkedCatalogs = rows.Count, blockedCatalogs = blocked,
            quantityPolicy = "One recorded copy per item variant; unique applies to base item ID.", catalogs = rows
        }, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } }) + Environment.NewLine);
        Console.WriteLine($"{(blocked == 0 ? "PASS" : "BLOCKED")}: release audit; {rows.Count} catalogs checked, {blocked} incomplete catalogs.");
        return blocked == 0 ? 0 : 1;
    }
}
