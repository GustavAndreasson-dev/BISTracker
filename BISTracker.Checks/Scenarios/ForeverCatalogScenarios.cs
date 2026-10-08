using System.Text;
using System.Text.Json.Nodes;
using BISTracker.Domain;
using BISTracker.Infrastructure;
using static BISTracker.Checks.Scenarios.CheckAssertions;

namespace BISTracker.Checks.Scenarios;

internal static class ForeverCatalogScenarios
{
    public static async Task RunAsync(CheckRun checks)
    {
        await checks.RunAsync("27 Forever-kataloger innehåller källbelagda nivå 30-alternativ enligt dungeon/quest-policyn", async () =>
        {
            var provider = new CharacterCatalog();
            var allSources = new CharacterCatalog(includeOtherSources: true);
            var count = 0;
            foreach (var characterClass in Enum.GetValues<CharacterClass>())
                foreach (var spec in CharacterDefinition.Specializations(characterClass))
                {
                    var catalog = await provider.LoadAsync(GameVersion.Forever, characterClass, spec.Id);
                    var unfiltered = await allSources.LoadAsync(GameVersion.Forever, characterClass, spec.Id);
                    Assert(catalog.Items.Count > 0 && catalog.UnavailableReason is null && !catalog.IsSample, "Varje spec behöver riktiga rekommendationer.");
                    Assert(catalog.Set == CatalogSet.ForeverLevelThirty && catalog.Context.Version == "WoW Forever" &&
                        catalog.Context.Specialization == $"{spec.Name} {characterClass}" && catalog.Context.Phase.Contains("30", StringComparison.Ordinal) &&
                        catalog.Context.Phase.Contains("beta", StringComparison.OrdinalIgnoreCase), "Nivå, beta, klass och spec är uttryckliga.");
                    Assert(catalog.Items.Select(item => item.Id).Distinct().Count() == catalog.Items.Count, "Rekommendations-ID:n är unika.");
                    Assert(unfiltered.Items.Count >= catalog.Items.Count, "Källpolicyn får bara filtrera alternativ.");
                    foreach (var item in catalog.Items)
                    {
                        var details = item.Details ?? throw new InvalidOperationException("Riktig itemmetadata saknas.");
                        Assert(item.Id.StartsWith($"forever-beta-level30-{characterClass.ToString().ToLowerInvariant()}-{spec.Id}-", StringComparison.Ordinal), "Identiteten är kontextavgränsad.");
                        Assert(details.ClassicItemId > 0 && !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Source), "Item-ID, namn och anskaffning finns.");
                        Assert(details.AcquisitionType is AcquisitionType.Dungeon or AcquisitionType.Quest, "Default får inte visa crafting, köp eller world drops.");
                        Assert(Https(details.ItemUrl) && Https(details.RecommendationUrl) &&
                            (details.IconUrl.Length == 0 || Https(details.IconUrl)), "Källänkar och eventuell ikon använder HTTPS.");
                        Assert(Enum.IsDefined(details.WeaponKind) &&
                            (details.WeaponKind != WeaponKind.TwoHanded || item.Slot == EquipmentSlot.MainHand) &&
                            (details.WeaponKind != WeaponKind.OffHand || item.Slot == EquipmentSlot.OffHand) &&
                            (details.WeaponKind != WeaponKind.MainHand || item.Slot == EquipmentSlot.MainHand), "Vapen metadata matchar slot.");
                    }
                    count++;
                }
            Assert(count == 27 && provider.CatalogIds().Count == 27, "Alla nio klasser och 27 specs laddades.");
        });

        await checks.RunAsync("Katalogläsaren bevarar fullständigt suffixnamn, alternativa källänkar och källfiltrering", () =>
        {
            var document = Fixture();
            var first = document["items"]!.AsArray()[0]!.AsObject();
            first["name"] = "TEST ONLY Circlet of Healing";
            first["requiredSuffix"] = "of Healing";
            first["recommendationUrl"] = "https://example.com/test-only-independent-guide";
            var crafted = Item("crafted", 900002, "Chest", "None", "Crafting");
            document["items"]!.AsArray().Add(crafted);
            var filtered = Read(document);
            var complete = Read(document, true);
            Assert(filtered.Catalog.Items.Single().Name == "TEST ONLY Circlet of Healing", "Suffixet ska inte läggas till två gånger.");
            Assert(filtered.Catalog.Items.Single().Details!.RequiredSuffix == "of Healing" &&
                filtered.Catalog.Items.Single().Details!.RecommendationUrl == "https://example.com/test-only-independent-guide", "Suffixidentitet och separat rekommendationskälla bevaras.");
            Assert(complete.Catalog.Items.Count == 2 && complete.Catalog.Items.Single(item => item.Slot == EquipmentSlot.Chest).Details!.AcquisitionType == AcquisitionType.Crafting,
                "Rawimporten granskar även källor som defaultfiltreringen döljer.");
            first["name"] = "TEST ONLY Circlet";
            Assert(Read(document).Catalog.Items.Single().Name == "TEST ONLY Circlet of Healing", "Ett saknat suffix läggs till exakt en gång.");
            var thirty = Fixture(30, "Beta");
            Assert(Read(thirty).Catalog.Items.Single().Id != filtered.Catalog.Items.Single().Id, "Nivå 30 och 60 ska inte dela rekommendations-ID även när rå-ID är samma.");
            return Task.CompletedTask;
        });

        await checks.RunAsync("Level 60-pack importeras atomiskt och upptäcks av samma levande katalogprovider", async () =>
        {
            var target = checks.PathFor("forever-refresh/imports");
            var source = checks.PathFor("forever-refresh/source");
            Directory.CreateDirectory(source);
            var provider = new CharacterCatalog(target);
            var before = await provider.LoadAsync(GameVersion.Forever, CharacterClass.Mage, "fire", CatalogSet.ForeverLevelSixty.Id);
            var thirty = await provider.LoadAsync(GameVersion.Forever, CharacterClass.Mage, "fire");
            Assert(before.Items.Count == 0 && before.UnavailableReason is not null, "Nivå 60 börjar utan påhittad produktdata.");
            var fixture = Fixture();
            var sourcePath = Path.Combine(source, "TEST-ONLY-fire-60.json");
            await File.WriteAllTextAsync(sourcePath, fixture.ToJsonString());
            var original = await File.ReadAllBytesAsync(sourcePath);
            await new CatalogPackImporter(target).ImportAsync(source);
            var after = await provider.LoadAsync(GameVersion.Forever, CharacterClass.Mage, "fire", CatalogSet.ForeverLevelSixty.Id);
            Assert(after.Items.Single().Name.StartsWith("TEST ONLY", StringComparison.Ordinal) && after.Set!.LevelCap == 60 && after.Set.ReleaseStage == "Launch",
                "Samma provider måste upptäcka en komplett importerad 60-katalog utan omstart.");
            Assert(after.Items.Single().Id.StartsWith("forever-launch-level60-mage-fire-", StringComparison.Ordinal), "Importerade ID:n har egen kontext.");
            Assert((await provider.LoadAsync(GameVersion.Forever, CharacterClass.Mage, "fire")).Items.Select(item => item.Id).SequenceEqual(thirty.Items.Select(item => item.Id)),
                "Nivå 30-katalogen bevaras efter nivå 60-import.");
            Assert((await File.ReadAllBytesAsync(sourcePath)).SequenceEqual(original), "Originalpack ska kopieras, aldrig flyttas eller skrivas om.");
            Assert(Directory.GetDirectories(target).Length == 1 && Directory.GetFiles(target, "*.json", SearchOption.AllDirectories).Length == 1,
                "En ny publicerad batch innehåller hela den validerade importen.");
        });

        await checks.RunAsync("Ogiltig JSON, schema, klass/spec-kontext, nivåkrav och vapenhand avvisas utan filförändring", async () =>
        {
            var target = checks.PathFor("forever-invalid/imports");
            var originalSource = checks.PathFor("forever-invalid/valid-source");
            await WriteFixtureAsync(originalSource, "existing.json", Fixture());
            var importer = new CatalogPackImporter(target);
            await importer.ImportAsync(originalSource);
            var progressPath = checks.PathFor("forever-invalid/player-progress.json");
            var progress = "{\"TEST ONLY\":\"unrelated saved progress must remain unchanged\"}";
            await File.WriteAllTextAsync(progressPath, progress);
            var original = await SnapshotAsync(target);
            var cases = new List<(string Name, string Json)>
            {
                ("broken-json", "{broken"), ("null-json", "null"), ("missing-schema", "{}")
            };
            Invalid("schema", doc => doc["schemaVersion"] = 2);
            Invalid("classic-context", doc => doc["gameVersion"] = "Classic");
            Invalid("wrong-class-spec", doc => doc["specializationId"] = "holy");
            Invalid("level-31", doc => doc["items"]!.AsArray()[0]!["requiredLevel"] = 31, 30, "Beta");
            Invalid("id-context-mismatch", doc => doc["catalogId"] = "forever-launch-level60-priest-holy");
            Invalid("unknown-level", doc => doc["levelCap"] = 40);
            Invalid("missing-review", doc => doc["reviewedOn"] = "2026-99-99");
            Invalid("http-source", doc => doc["sourceUrl"] = "http://example.com/test-only-guide");
            Invalid("wrong-twohand-slot", doc => doc["items"]!.AsArray()[0]!["weaponKind"] = "TwoHanded");
            Invalid("wrong-offhand-slot", doc => doc["items"]!.AsArray()[0]!["weaponKind"] = "OffHand");
            Invalid("duplicate-rows", doc => doc["items"]!.AsArray().Add(doc["items"]!.AsArray()[0]!.DeepClone()));
            foreach (var invalid in cases)
            {
                var source = checks.PathFor("forever-invalid/" + invalid.Name);
                Directory.CreateDirectory(source);
                var sourcePath = Path.Combine(source, "invalid.json");
                await File.WriteAllTextAsync(sourcePath, invalid.Json);
                await Throws<InvalidDataException>(() => importer.ImportAsync(source));
                Assert(SameSnapshot(original, await SnapshotAsync(target)), $"{invalid.Name}: importerade data ska bevaras.");
                Assert(await File.ReadAllTextAsync(progressPath) == progress && await File.ReadAllTextAsync(sourcePath) == invalid.Json,
                    $"{invalid.Name}: spelarens framsteg och källfilen får inte ändras.");
                Assert(Directory.GetDirectories(checks.PathFor("forever-invalid"), ".catalog-import-*").Length == 0, "Ingen stagingmapp lämnas vid valideringsfel.");
            }

            void Invalid(string name, Action<JsonObject> mutate, int level = 60, string stage = "Beta")
            {
                var document = Fixture(level, stage);
                mutate(document);
                cases.Add((name, document.ToJsonString()));
            }
        });

        await checks.RunAsync("Duplicerad packidentitet skyddar både inbyggda och tidigare importerade kataloger", async () =>
        {
            var target = checks.PathFor("forever-duplicates/imports");
            var source = checks.PathFor("forever-duplicates/initial");
            await WriteFixtureAsync(source, "initial.json", Fixture());
            var importer = new CatalogPackImporter(target);
            await importer.ImportAsync(source);
            var original = await SnapshotAsync(target);
            await Throws<InvalidDataException>(() => importer.ImportAsync(source));
            var embedded = checks.PathFor("forever-duplicates/embedded");
            await WriteFixtureAsync(embedded, "embedded-id.json", Fixture(30, "Beta"));
            await Throws<InvalidDataException>(() => importer.ImportAsync(embedded));
            var batch = checks.PathFor("forever-duplicates/batch");
            await WriteFixtureAsync(batch, "one.json", Fixture(60, "Beta"));
            await WriteFixtureAsync(batch, "two.json", Fixture(60, "Beta"));
            await Throws<InvalidDataException>(() => importer.ImportAsync(batch));
            Assert(SameSnapshot(original, await SnapshotAsync(target)), "Varken befintlig import, inbyggd ID eller dubbla batch-ID:n ska skrivas över.");
        });

        await checks.RunAsync("Blandad giltig/ogiltig packbatch publicerar inga delkataloger", async () =>
        {
            var target = checks.PathFor("forever-mixed/imports");
            var initial = checks.PathFor("forever-mixed/initial");
            await WriteFixtureAsync(initial, "initial.json", Fixture());
            var importer = new CatalogPackImporter(target);
            await importer.ImportAsync(initial);
            var original = await SnapshotAsync(target);
            var source = checks.PathFor("forever-mixed/batch");
            await WriteFixtureAsync(source, "01-valid.json", Fixture(60, "Beta"));
            await File.WriteAllTextAsync(Path.Combine(source, "02-invalid.json"), "{broken");
            await Throws<InvalidDataException>(() => importer.ImportAsync(source));
            Assert(SameSnapshot(original, await SnapshotAsync(target)), "Hela batchen måste avvisas; giltiga filer får inte publiceras separat.");
            Assert(!new CharacterCatalog(target).CatalogIds().Contains("forever-beta-level60-mage-fire"), "Den giltiga delkatalogen ska inte vara synlig.");
        });

        await checks.RunAsync("Avbruten eller tom katalogimport bevarar tidigare kataloger och lämnar ingen staging", async () =>
        {
            var target = checks.PathFor("forever-cancel/imports");
            var initial = checks.PathFor("forever-cancel/initial");
            await WriteFixtureAsync(initial, "initial.json", Fixture());
            var importer = new CatalogPackImporter(target);
            await importer.ImportAsync(initial);
            var original = await SnapshotAsync(target);
            var source = checks.PathFor("forever-cancel/batch");
            await WriteFixtureAsync(source, "valid.json", Fixture(60, "Beta"));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Throws<OperationCanceledException>(() => importer.ImportAsync(source, cancellation.Token));
            var empty = checks.PathFor("forever-cancel/empty");
            Directory.CreateDirectory(empty);
            await Throws<InvalidDataException>(() => importer.ImportAsync(empty));
            Assert(SameSnapshot(original, await SnapshotAsync(target)), "Avbrott eller tom mapp får inte ändra tidigare import.");
            Assert(Directory.GetDirectories(checks.PathFor("forever-cancel"), ".catalog-import-*").Length == 0, "Ingen temporär stagingmapp lämnas.");
            await importer.ImportAsync(source);
            Assert(new CharacterCatalog(target).CatalogIds().Contains("forever-beta-level60-mage-fire"), "Efter ett avbrott ska samma importer kunna användas igen.");
        });
    }

    // Fictional equipment and URLs exist only in disposable checks, never in product data.
    private static JsonObject Fixture(int level = 60, string stage = "Launch") => new()
    {
        ["schemaVersion"] = 1,
        ["catalogId"] = $"forever-{stage.ToLowerInvariant()}-level{level}-mage-fire",
        ["gameVersion"] = "Forever", ["characterClass"] = "Mage", ["specializationId"] = "fire",
        ["levelCap"] = level, ["releaseStage"] = stage, ["phase"] = $"TEST ONLY level {level} {stage}",
        ["patch"] = "TEST ONLY", ["sourceUrl"] = "https://example.com/test-only-guide", ["reviewedOn"] = "2026-10-08",
        ["selectionMethod"] = "TEST ONLY fictional behavior fixture", ["status"] = "Reviewed",
        ["items"] = new JsonArray(Item("head", 900001, "Head", "None", "Dungeon"))
    };

    private static JsonObject Item(string id, int itemId, string slot, string weapon, string source) => new()
    {
        ["id"] = id, ["slot"] = slot, ["itemId"] = itemId, ["name"] = "TEST ONLY Circlet", ["requiredSuffix"] = null,
        ["requiredLevel"] = 1, ["acquisitionType"] = source, ["source"] = "TEST ONLY fixture dungeon",
        ["note"] = "TEST ONLY", ["iconUrl"] = null, ["itemUrl"] = "https://example.com/test-only-item",
        ["recommendationUrl"] = "https://example.com/test-only-guide", ["weaponKind"] = weapon, ["uniqueEquipped"] = false
    };

    private static ReviewedCatalogPack Read(JsonObject document, bool otherSources = false)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()), writable: false);
        return ForeverCatalogReader.Read(stream, otherSources);
    }

    private static async Task WriteFixtureAsync(string directory, string name, JsonObject document)
    {
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(Path.Combine(directory, name), document.ToJsonString());
    }

    private static async Task<Dictionary<string, byte[]>> SnapshotAsync(string directory)
    {
        var snapshot = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        if (Directory.Exists(directory))
            foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
                snapshot.Add(Path.GetRelativePath(directory, file), await File.ReadAllBytesAsync(file));
        return snapshot;
    }

    private static bool SameSnapshot(Dictionary<string, byte[]> first, Dictionary<string, byte[]> second) =>
        first.Count == second.Count && first.All(pair => second.TryGetValue(pair.Key, out var bytes) && bytes.SequenceEqual(pair.Value));
    private static bool Https(string url) => Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
}
