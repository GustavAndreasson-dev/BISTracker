using System.Text.Json.Nodes;
using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Infrastructure;
using static BISTracker.Checks.Scenarios.CheckAssertions;

namespace BISTracker.Checks.Scenarios;

/// <summary>Exercises shipped catalogs and production JSON persistence using disposable disk paths.</summary>
internal static class DistributionScenarios
{
    public static async Task RunAsync(CheckRun checks)
    {
        await checks.RunAsync("Distribution: isolerad datamapp kräver absolut sökväg och normal profil bevaras", () =>
        {
            Assert(ProgressFilePaths.DataDirectory() == Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BISTracker"),
                "Normal start ska behålla spelarens befintliga datamapp.");
            Assert(ProgressFilePaths.DataDirectory(checks.PathFor("isolated-profile")) == checks.PathFor("isolated-profile"),
                "Distributionsprovet ska kunna använda en egen absolut testprofil.");
            try
            {
                ProgressFilePaths.DataDirectory("relative-profile");
                Assert(false, "En relativ datamapp får inte bero på programmappens placering.");
            }
            catch (ArgumentException) { }
            return Task.CompletedTask;
        });

        await checks.RunAsync("Distribution: Classic start, skapa, spara och återläs med faktisk JSON-lagring", async () =>
        {
            const string name = "distribution-classic";
            var service = Service(checks, name);
            var startup = await service.LoadAsync();
            Assert(startup.Selection!.ActiveCharacter.Version == GameVersion.Classic &&
                startup.Selection.ActiveCharacter.Class == CharacterClass.Priest &&
                startup.Selection.ActiveSpecialization.Id == "holy" && startup.Entries.Count >= 17,
                "Den levererade Classic-katalogen ska användas vid första start.");
            Assert(!startup.Catalog.IsSample && startup.Entries.All(entry => !entry.IsOwned && !entry.IsEquipped),
                "En ny isolerad arbetsyta ska visa verkliga, tomma Classic-framsteg.");
            var originalCharacter = startup.Selection.ActiveCharacter.Id;
            // 1.0.0 Holy Priest targets keep their IDs even when reviewed alternatives are added to the slot.
            var head = startup.Entries.Single(entry => entry.Item.Id == "classic-p1-item-18727").Item;
            var chest = startup.Entries.Single(entry => entry.Item.Id == "classic-p1-item-13346").Item;
            await service.SetEquippedAsync(head.Id, true);
            var created = await service.CreateAsync("Distribution Classic", GameVersion.Classic, CharacterClass.Priest);
            var createdId = created.Selection!.ActiveCharacter.Id;
            var holy = await service.SelectAsync(createdId, "holy");
            Assert(holy.Entries.All(entry => !entry.IsOwned && !entry.IsEquipped), "Den skapade karaktären ska ha eget ägande.");
            await service.SetEquippedAsync(chest.Id, true);

            var disk = (await new JsonWorkspaceRepository(checks.PathFor(name + "/characters.json")).LoadAsync())!;
            Assert(disk.Characters.Length == 2 && disk.ActiveCharacterId == createdId,
                "Båda karaktärerna och aktivt val ska vara skrivna till faktisk JSON-fil.");
            var restarted = Service(checks, name);
            var restored = await restarted.LoadAsync();
            Assert(restored.Selection!.ActiveCharacter.Id == createdId && restored.Selection.ActiveSpecialization.Id == "holy" &&
                Entry(restored, chest.Id).IsOwned && Entry(restored, chest.Id).IsEquipped && !Entry(restored, head.Id).IsOwned,
                "En ny tjänst, katalogprovider och repository ska återläsa den skapade karaktärens sparade val.");
            var first = await restarted.SelectAsync(originalCharacter, "holy");
            Assert(Entry(first, head.Id).IsOwned && Entry(first, head.Id).IsEquipped && !Entry(first, chest.Id).IsOwned,
                "Första karaktärens utrustning ska också finnas kvar och vara isolerad efter omstart.");
        });

        await checks.RunAsync("Distribution: verklig Forever Rogue, tre specs och diskpersistens över nivå30/60 och omstart", async () =>
        {
            const string name = "distribution-forever-rogue";
            var importDirectory = checks.PathFor(name + "/catalogs");
            var provider = new CharacterCatalog(importDirectory);
            var specs = CharacterDefinition.Specializations(CharacterClass.Rogue).Select(spec => spec.Id).ToArray();
            var catalogs = new Dictionary<string, BisCatalog>();
            foreach (var spec in specs)
            {
                var catalog = await provider.LoadAsync(GameVersion.Forever, CharacterClass.Rogue, spec);
                Assert(catalog.Items.Count > 0 && !catalog.IsSample && catalog.Set!.LevelCap == 30,
                    "Alla tre Rogue-specs måste komma från levererade, verkliga nivå30-resurser.");
                catalogs.Add(spec, catalog);
            }
            var heads = SharedItemAtSlot(catalogs, EquipmentSlot.Head);
            var chests = SharedItemAtSlot(catalogs, EquipmentSlot.Chest);
            var service = new CharacterTrackerService(provider,
                new JsonWorkspaceRepository(checks.PathFor(name + "/characters.json")), checks.Repository(name + "/legacy.json"));
            var created = await service.CreateAsync("Distribution Rogue", GameVersion.Forever, CharacterClass.Rogue);
            var character = created.Selection!.ActiveCharacter.Id;
            Assert(created.Selection.ActiveCatalogSet!.Id == CatalogSet.ForeverLevelThirty.Id, "Rogue ska börja i riktig Forever30-kontext.");

            await service.SelectAsync(character, specs[0]);
            await service.SetEquippedAsync(heads[specs[0]].Id, true);
            var second = await service.SelectAsync(character, specs[1]);
            Assert(Entry(second, heads[specs[1]].Id).IsOwned && !Entry(second, heads[specs[1]].Id).IsEquipped,
                "Verkligt samma item ska ägas i nästa spec utan att automatiskt utrustas.");
            await service.SetEquippedAsync(chests[specs[1]].Id, true);
            var third = await service.SelectAsync(character, specs[2]);
            Assert(Entry(third, heads[specs[2]].Id).IsOwned && Entry(third, chests[specs[2]].Id).IsOwned &&
                !Entry(third, heads[specs[2]].Id).IsEquipped && !Entry(third, chests[specs[2]].Id).IsEquipped,
                "Tredje spec ska dela båda items ägande men ha egen tom utrustning.");
            await service.SetEquippedAsync(heads[specs[2]].Id, true);
            await service.SetEquippedAsync(chests[specs[2]].Id, true);

            var restarted = Service(checks, name);
            Assert((await restarted.LoadAsync()).Selection!.ActiveSpecialization.Id == specs[2], "Aktiv Rogue-spec ska återläsas från disk.");
            await AssertGearAsync(restarted, character, specs, heads, chests, [true, false, true], [false, true, true]);
            var pending = await restarted.SelectCatalogAsync(CatalogSet.ForeverLevelSixty.Id);
            Assert(pending.Entries.Count == 0 && pending.Catalog.UnavailableReason is not null,
                "Den levererade nivå60-kontexten ska vara explicit tom före testimport, inte kopierad nivå30-data.");
            var sixtyRestart = Service(checks, name);
            Assert((await sixtyRestart.LoadAsync()).Selection!.ActiveCatalogSet!.Id == CatalogSet.ForeverLevelSixty.Id,
                "Även valet av en ännu tom nivå60-kontext ska sparas på disk.");

            // Test-only level60 documents are confined to this disposable run. Shared physical
            // identities intentionally exercise ownership across levels; these are not real level60 recommendations.
            var fixtureDirectory = checks.PathFor(name + "/test-only-level60");
            Directory.CreateDirectory(fixtureDirectory);
            foreach (var spec in specs)
                await File.WriteAllTextAsync(Path.Combine(fixtureDirectory, spec + ".json"),
                    LevelSixtyFixture(spec, heads[spec], chests[spec]).ToJsonString());
            await new CatalogPackImporter(importDirectory).ImportAsync(fixtureDirectory);
            var refreshed = await sixtyRestart.LoadAsync();
            Assert(refreshed.Entries.Count == 2 && refreshed.Entries.All(entry => entry.IsOwned && !entry.IsEquipped),
                "Testimport ska uppdatera den levande providern; gemensamt ägande ska återfinnas utan kopierad utrustning.");
            var sixtyHeads = new Dictionary<string, Recommendation>();
            var sixtyChests = new Dictionary<string, Recommendation>();
            foreach (var spec in specs)
            {
                var selected = await sixtyRestart.SelectAsync(character, spec);
                sixtyHeads.Add(spec, selected.Entries.Single(entry => entry.Item.Slot == EquipmentSlot.Head).Item);
                sixtyChests.Add(spec, selected.Entries.Single(entry => entry.Item.Slot == EquipmentSlot.Chest).Item);
                Assert(selected.Entries.All(entry => entry.IsOwned && !entry.IsEquipped), "Alla tre nivå60-listor ska börja utan utrustning.");
            }
            await sixtyRestart.SelectAsync(character, specs[0]);
            await sixtyRestart.SetEquippedAsync(sixtyChests[specs[0]].Id, true);
            await sixtyRestart.SelectAsync(character, specs[1]);
            await sixtyRestart.SetEquippedAsync(sixtyHeads[specs[1]].Id, true);
            await sixtyRestart.SelectAsync(character, specs[2]);

            var disk = (await new JsonWorkspaceRepository(checks.PathFor(name + "/characters.json")).LoadAsync())!;
            var persisted = disk.Characters.Single(item => item.Id == character);
            Assert(persisted.OwnedItemKeys.Length == 2 && persisted.EquippedBySpec.Count == 3 &&
                persisted.ArchivedLoadouts![CatalogSet.ForeverLevelThirty.Id].Count == 3,
                "Fysisk fil ska innehålla gemensamt ägande och tre separata listor för både aktiv och arkiverad nivå.");
            var finalRestart = Service(checks, name);
            Assert((await finalRestart.LoadAsync()).Selection!.ActiveCatalogSet!.Id == CatalogSet.ForeverLevelSixty.Id,
                "Importerat nivå60-val ska återläsas med en helt ny provider och tjänst.");
            await AssertGearAsync(finalRestart, character, specs, sixtyHeads, sixtyChests, [false, true, false], [true, false, false]);
            await finalRestart.SelectCatalogAsync(CatalogSet.ForeverLevelThirty.Id);
            await AssertGearAsync(finalRestart, character, specs, heads, chests, [true, false, true], [false, true, true]);
            await finalRestart.SelectCatalogAsync(CatalogSet.ForeverLevelSixty.Id);
            await AssertGearAsync(finalRestart, character, specs, sixtyHeads, sixtyChests, [false, true, false], [true, false, false]);
        });
    }

    private static CharacterTrackerService Service(CheckRun checks, string name) => new(
        new CharacterCatalog(checks.PathFor(name + "/catalogs")),
        new JsonWorkspaceRepository(checks.PathFor(name + "/characters.json")), checks.Repository(name + "/legacy.json"));

    private static Dictionary<string, Recommendation> SharedItemAtSlot(Dictionary<string, BisCatalog> catalogs, EquipmentSlot slot)
    {
        var shared = catalogs.Values.First().Items.Where(item => item.Slot == slot).FirstOrDefault(candidate =>
            catalogs.Values.All(catalog => catalog.Items.Any(item => item.Slot == slot &&
                CharacterLoadouts.ItemKey(item) == CharacterLoadouts.ItemKey(candidate))));
        Assert(shared is not null, $"Levererade Rogue-kataloger behöver ett gemensamt verkligt {slot}-item för distributionskontrollen.");
        var key = CharacterLoadouts.ItemKey(shared!);
        return catalogs.ToDictionary(pair => pair.Key,
            pair => pair.Value.Items.First(item => item.Slot == slot && CharacterLoadouts.ItemKey(item) == key));
    }

    private static async Task AssertGearAsync(CharacterTrackerService service, Guid character, string[] specs,
        Dictionary<string, Recommendation> heads, Dictionary<string, Recommendation> chests, bool[] headEquipped, bool[] chestEquipped)
    {
        for (var index = 0; index < specs.Length; index++)
        {
            var spec = specs[index];
            var snapshot = await service.SelectAsync(character, spec);
            Assert(Entry(snapshot, heads[spec].Id).IsOwned && Entry(snapshot, chests[spec].Id).IsOwned &&
                Entry(snapshot, heads[spec].Id).IsEquipped == headEquipped[index] &&
                Entry(snapshot, chests[spec].Id).IsEquipped == chestEquipped[index],
                $"Rogue {spec} ska återläsa rätt eget utrustningsval och gemensamt ägande.");
            Assert(snapshot.Entries.Count(entry => entry.IsEquipped) ==
                (headEquipped[index] ? 1 : 0) + (chestEquipped[index] ? 1 : 0), "Inga extra utrustningsval får tillkomma.");
        }
    }

    private static JsonObject LevelSixtyFixture(string spec, Recommendation head, Recommendation chest) => new()
    {
        ["schemaVersion"] = 1, ["catalogId"] = $"{CatalogSet.ForeverLevelSixty.Id}-rogue-{spec}",
        ["gameVersion"] = "Forever", ["characterClass"] = "Rogue", ["specializationId"] = spec,
        ["levelCap"] = 60, ["releaseStage"] = CatalogSet.ForeverLevelSixty.ReleaseStage, ["phase"] = "TEST ONLY distribution level60",
        ["patch"] = "TEST ONLY", ["sourceUrl"] = "https://example.com/test-only-distribution",
        ["reviewedOn"] = "2026-10-08", ["selectionMethod"] = "TEST ONLY ownership and persistence fixture", ["status"] = "Partial",
        ["items"] = new JsonArray(FixtureItem("head", head), FixtureItem("chest", chest))
    };

    private static JsonObject FixtureItem(string id, Recommendation original) => new()
    {
        ["id"] = id, ["slot"] = original.Slot.ToString(), ["itemId"] = original.Details!.ClassicItemId,
        ["name"] = "TEST ONLY distribution fixture", ["requiredSuffix"] = original.Details.RequiredSuffix,
        ["requiredLevel"] = 60, ["acquisitionType"] = "Dungeon", ["source"] = "TEST ONLY fictional distribution dungeon",
        ["note"] = "TEST ONLY, not a real level60 recommendation", ["iconUrl"] = null,
        ["itemUrl"] = "https://example.com/test-only-distribution-item",
        ["recommendationUrl"] = "https://example.com/test-only-distribution", ["weaponKind"] = "None", ["uniqueEquipped"] = false
    };
}
