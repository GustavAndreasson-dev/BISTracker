using System.Text;
using System.Text.Json.Nodes;
using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Infrastructure;
using static BISTracker.Checks.Scenarios.CheckAssertions;

namespace BISTracker.Checks.Scenarios;

/// <summary>REQ-013/DEC-016: built-in Classic phase 1 packs and Holy Priest compatibility with 1.0.0.</summary>
internal static class ClassicCatalogScenarios
{
    // The 17 recommendation IDs shipped in 1.0.0; saved equipment refers to them.
    private static readonly (EquipmentSlot Slot, int ItemId, bool OfHealing)[] LegacyTargets =
    [
        (EquipmentSlot.Head, 18727, false), (EquipmentSlot.Neck, 18723, false), (EquipmentSlot.Shoulder, 18681, false),
        (EquipmentSlot.Back, 13386, true), (EquipmentSlot.Chest, 13346, false), (EquipmentSlot.Wrist, 11766, true),
        (EquipmentSlot.Hands, 10787, true), (EquipmentSlot.Waist, 12589, false), (EquipmentSlot.Legs, 11841, false),
        (EquipmentSlot.Feet, 11822, false), (EquipmentSlot.Finger1, 16058, false), (EquipmentSlot.Finger2, 18103, false),
        (EquipmentSlot.Trinket1, 12930, false), (EquipmentSlot.Trinket2, 11819, false), (EquipmentSlot.MainHand, 11923, false),
        (EquipmentSlot.OffHand, 11928, false), (EquipmentSlot.Ranged, 16997, false)
    ];

    public static async Task RunAsync(CheckRun checks)
    {
        await checks.RunAsync("Classic fas 1-läsaren: egen kontext, crafting tillåts, övriga källor filtreras och fel kontext avvisas", () =>
        {
            var document = Fixture();
            var id = document["catalogId"]!.GetValue<string>();
            var items = document["items"]!.AsArray();
            items.Add(Item(id, "Chest", 900102, "Crafting"));
            items.Add(Item(id, "Neck", 900103, "WorldDrop"));
            items.Add(Item(id, "Back", 900104, "Vendor"));
            var quest = Item(id, "Finger1", 900105, "Quest");
            quest["availableFactions"] = new JsonArray("Horde");
            items.Add(quest);
            var filtered = Read(document);
            var complete = Read(document, true);
            Assert(filtered.Set == CatalogSet.ClassicPhaseOne && filtered.Catalog.Set == CatalogSet.ClassicPhaseOne &&
                filtered.Catalog.Context.Version == "WoW Classic (Vanilla)" && filtered.Catalog.Context.Phase == ReviewedCatalogReader.ClassicPhase &&
                filtered.Status == "Partial", "Classic-paketet får Classic fas 1-kontext och behåller sin granskningsstatus.");
            Assert(filtered.Catalog.Items.Select(item => item.Details!.AcquisitionType).Order()
                    .SequenceEqual([AcquisitionType.Dungeon, AcquisitionType.Quest, AcquisitionType.Crafting]) && complete.Catalog.Items.Count == 5,
                "Dungeon, Quest och Crafting visas; world drop och vendor filtreras men granskas i rådata.");
            Assert(filtered.Catalog.Items.All(item => item.Id.StartsWith(id + "-", StringComparison.Ordinal)) &&
                filtered.Catalog.Items.Single(item => item.Slot == EquipmentSlot.Head).Id == $"{id}-head-900101",
                "Classic rad-ID:n är redan kontextavgränsade och används oförändrade.");
            Assert(!filtered.Catalog.SelectionMethod!.Contains("Forever", StringComparison.OrdinalIgnoreCase), "Urvalsetiketten får inte påstå Forever.");
            Assert(filtered.Catalog.Items.Single(item => item.Slot == EquipmentSlot.Finger1).Details!.AvailableFactions!.SequenceEqual([CharacterFaction.Horde]),
                "Questens fraktion bevaras.");
            Assert(Read(Fixture()).Catalog.Items.Single().Id != Read(ForeverFixture()).Catalog.Items.Single().Id, "Classic och Forever delar inte rekommendations-ID.");

            Invalid(doc => doc["levelCap"] = 30);
            Invalid(doc => doc["releaseStage"] = "Launch");
            Invalid(doc => doc["phase"] = "Phase 2 · pre-raid");
            Invalid(doc => doc["catalogId"] = "forever-launch-level60-mage-frost");
            Invalid(doc => doc["catalogId"] = "classic-phase1-level60-mage-fire");
            Invalid(doc => doc["gameVersion"] = "Forever");
            Invalid(doc => doc["status"] = "Draft");
            Invalid(doc => doc["items"]!.AsArray()[0]!["id"] = "head-900101");
            Invalid(doc => doc["items"]!.AsArray()[0]!["id"] = "classic-phase1-level60-mage-frost-chest-900101");
            Invalid(doc => doc["items"]!.AsArray()[0]!["id"] = "classic-phase1-level60-mage-frost-head-900102");
            Invalid(doc => doc["items"]!.AsArray()[0]!["requiredLevel"] = 61);
            Invalid(doc => doc["items"]!.AsArray()[0]!["availableFactions"] = new JsonArray("Alliance", "Alliance"));
            Invalid(doc => doc["items"]!.AsArray().Add(doc["items"]!.AsArray()[0]!.DeepClone()));
            return Task.CompletedTask;
        });

        await checks.RunAsync("Classic recommendationId bevarar bara Holy Priest-ID:n från 1.0.0 för samma itemvariant", () =>
        {
            var holy = Fixture("Priest", "holy");
            var id = holy["catalogId"]!.GetValue<string>();
            Assert(id == ReviewedCatalogReader.LegacyRecommendationCatalogId, "Fixturen använder Holy Priest-paketets identitet.");
            var head = holy["items"]!.AsArray()[0]!.AsObject();
            head["recommendationId"] = "classic-p1-item-900101";
            var cape = Item(id, "Back", 900202, "Dungeon");
            cape["requiredSuffix"] = "of Healing";
            cape["recommendationId"] = "classic-p1-item-900202-of-healing";
            holy["items"]!.AsArray().Add(cape);
            holy["items"]!.AsArray().Add(Item(id, "Chest", 900203, "Crafting"));
            var pack = Read(holy);
            Assert(pack.Catalog.Items.Select(item => item.Id).Order(StringComparer.Ordinal).SequenceEqual(
                    ["classic-p1-item-900101", "classic-p1-item-900202-of-healing", $"{id}-chest-900203"]),
                "Angivet 1.0.0-ID används ordagrant; nya rader får kontextavgränsade ID:n.");

            InvalidFrom(holy, doc => Row(doc, 0)["recommendationId"] = "classic-p1-item-1");
            InvalidFrom(holy, doc => Row(doc, 0)["recommendationId"] = "classic-p1-item-900101-of-healing");
            InvalidFrom(holy, doc => Row(doc, 0)["recommendationId"] = "custom-900101");
            InvalidFrom(holy, doc => Row(doc, 0)["recommendationId"] = "");
            InvalidFrom(holy, doc => Row(doc, 1)["recommendationId"] = "classic-p1-item-900202");
            InvalidFrom(holy, doc => Row(doc, 1)["requiredSuffix"] = "of the Owl");
            InvalidFrom(holy, doc =>
            {
                var duplicate = Item(id, "Finger2", 900101, "Dungeon");
                duplicate["recommendationId"] = "classic-p1-item-900101";
                Row(doc, 0)["slot"] = "Finger1";
                Row(doc, 0)["id"] = $"{id}-finger1-900101";
                doc["items"]!.AsArray().Add(duplicate);
            });
            var mage = Fixture();
            Row(mage, 0)["recommendationId"] = "classic-p1-item-900101";
            Throws(() => Read(mage));
            var forever = ForeverFixture();
            Row(forever, 0)["recommendationId"] = "classic-p1-item-900101";
            Throws(() => Read(forever));
            return Task.CompletedTask;
        });

        await checks.RunAsync("Classic-pack kan inte importeras eller läggas i importmappen", async () =>
        {
            var target = checks.PathFor("classic-import/imports");
            var source = checks.PathFor("classic-import/source");
            Directory.CreateDirectory(source);
            var sourcePath = Path.Combine(source, "TEST-ONLY-classic.json");
            var json = Fixture().ToJsonString();
            await File.WriteAllTextAsync(sourcePath, json);
            await Throws<InvalidDataException>(() => new CatalogPackImporter(target).ImportAsync(source));
            Assert(!Directory.Exists(target) || Directory.GetFiles(target, "*", SearchOption.AllDirectories).Length == 0,
                "Ett Classic-pack publiceras aldrig som import.");
            Assert(await File.ReadAllTextAsync(sourcePath) == json, "Källfilen bevaras.");
            Assert(Directory.GetDirectories(checks.PathFor("classic-import"), ".catalog-import-*").Length == 0, "Ingen staging lämnas.");
            var planted = checks.PathFor("classic-import/planted");
            Directory.CreateDirectory(planted);
            await File.WriteAllTextAsync(Path.Combine(planted, "classic.json"), json);
            await Throws<InvalidDataException>(() => new CharacterCatalog(planted).LoadAsync(GameVersion.Classic, CharacterClass.Mage, "frost"));
        });

        await checks.RunAsync("Classic Holy Priest behåller alla 17 rad-ID:n; varje Classic-spec har paket eller uttrycklig lucka", async () =>
        {
            var provider = new CharacterCatalog();
            var holy = await provider.LoadAsync(GameVersion.Classic, CharacterClass.Priest, "holy");
            Assert(holy.Set == CatalogSet.ClassicPhaseOne && holy.UnavailableReason is null && !holy.IsSample, "Holy Priest är en aktiv Classic-katalog.");
            foreach (var target in LegacyTargets)
            {
                var id = $"classic-p1-item-{target.ItemId}{(target.OfHealing ? "-of-healing" : "")}";
                var item = holy.Items.SingleOrDefault(item => item.Id == id) ?? throw new InvalidOperationException($"1.0.0-ID saknas: {id}.");
                Assert(item.Slot == target.Slot && item.Details!.ClassicItemId == target.ItemId &&
                    item.Details.RequiredSuffix == (target.OfHealing ? "of Healing" : null), $"{id}: slot, item och suffix är oförändrade.");
            }
            var legacy = await new ClassicPhaseOneBisCatalog().LoadAsync();
            Assert(legacy.Items.Count == 17 && legacy.Items.All(item => holy.Items.Any(row => row.Id == item.Id && row.Slot == item.Slot && row.Details!.ClassicItemId == item.Details!.ClassicItemId)), "Den äldre enkellistan är exakt 1.0.0-målen ur samma paket.");
            Assert(holy.Items.Where(item => item.Details!.AcquisitionType == AcquisitionType.Quest)
                    .All(item => item.Details!.AvailableFactions!.Count == 2) &&
                Enum.GetValues<CharacterFaction>().All(faction => holy.EquipmentPlan.CatalogCoverageFor(faction).IsComplete),
                "Fordring's Seal och Stormrager är tillgängliga för båda fraktioner; båda fraktionsseten är kompletta.");
            var classicPacks = provider.ReviewedPacks().Where(pack => pack.Set.Version == GameVersion.Classic).ToArray();
            Assert(classicPacks.Any(pack => pack.CatalogId == ReviewedCatalogReader.LegacyRecommendationCatalogId) &&
                classicPacks.All(pack => pack.Set == CatalogSet.ClassicPhaseOne), "Inbyggda Classic-paket läses som Classic fas 1.");
            foreach (var characterClass in Enum.GetValues<CharacterClass>())
                foreach (var spec in CharacterDefinition.Specializations(characterClass))
                {
                    var catalog = await provider.LoadAsync(GameVersion.Classic, characterClass, spec.Id);
                    var catalogId = $"classic-phase1-level60-{characterClass.ToString().ToLowerInvariant()}-{spec.Id}";
                    var hasPack = classicPacks.Any(pack => pack.CatalogId == catalogId);
                    Assert(catalog.Set == CatalogSet.ClassicPhaseOne && catalog.Context.Version == "WoW Classic (Vanilla)", "Rätt Classic-kontext.");
                    Assert(hasPack
                        ? catalog.Items.Count > 0 && catalog.UnavailableReason is null && catalog.Items.All(item =>
                            (item.Id.StartsWith(catalogId + "-", StringComparison.Ordinal) ||
                             (catalogId == ReviewedCatalogReader.LegacyRecommendationCatalogId && item.Id.StartsWith(ReviewedCatalogReader.LegacyRecommendationPrefix, StringComparison.Ordinal))) &&
                            item.Details!.AcquisitionType is AcquisitionType.Dungeon or AcquisitionType.Quest or AcquisitionType.Crafting)
                        : catalog.Items.Count == 0 && catalog.UnavailableReason is not null,
                        $"{catalogId}: källgranskat paket med godkänd policy eller uttryckligen ingen lista.");
                }
        });

        await checks.RunAsync("Sparfil schema 1 från 1.0.0 med Holy Priest-ägande och utrustning laddas oförändrad", async () =>
        {
            var path = checks.PathFor("classic-1.0.0/characters.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, ReleaseOneWorkspace);
            var original = await File.ReadAllBytesAsync(path);
            var service = new CharacterTrackerService(new CharacterCatalog(), new JsonWorkspaceRepository(path), new UnreadableLegacyRepository());
            var priest = await service.LoadAsync();
            Assert(priest.Selection!.ActiveCharacter.Id == Guid.Parse("6f0c1b5e-2a43-4c1e-9d61-0a1b2c3d4e5f") &&
                priest.Selection.ActiveCharacter.Version == GameVersion.Classic && priest.Selection.ActiveSpecialization.Id == "holy" &&
                priest.Selection.ActiveCatalogSet == CatalogSet.ClassicPhaseOne, "Karaktär, version, spec och katalogset är oförändrade.");
            foreach (var id in new[] { "classic-p1-item-13386-of-healing", "classic-p1-item-18103", "classic-p1-item-11923", "classic-p1-item-11928", "classic-p1-item-16997" })
                Assert(Entry(priest, id).IsOwned && Entry(priest, id).IsEquipped, $"{id}: ägt och utrustat som i 1.0.0.");
            Assert(Entry(priest, "classic-p1-item-12930").IsOwned && !Entry(priest, "classic-p1-item-12930").IsEquipped &&
                priest.Entries.Count(entry => entry.IsOwned && entry.Item.Id.StartsWith(ReviewedCatalogReader.LegacyRecommendationPrefix, StringComparison.Ordinal)) == 6 && priest.Entries.Count(entry => entry.IsEquipped) == 5,
                "Ägt men ej utrustat item bevaras och inget annat markeras.");
            var shadow = await service.SelectAsync(priest.Selection.ActiveCharacter.Id, "shadow");
            Assert(shadow.Entries.All(entry => !entry.IsEquipped), "Övriga specs har fortfarande egna tomma listor.");
            var older = await service.SelectAsync(Guid.Parse("0d6f8e2a-91b4-4f37-8c5d-7e6a5b4c3d2e"), "holy");
            Assert(older.Selection!.ActiveCatalogSet == CatalogSet.ClassicPhaseOne && Entry(older, "classic-p1-item-18727").IsEquipped &&
                Entry(older, "classic-p1-item-11766-of-healing").IsOwned && !Entry(older, "classic-p1-item-11766-of-healing").IsEquipped,
                "Äldre schema 1 utan catalogSetId använder Classic fas 1 och behåller ägande och utrustning.");

            var reread = new CharacterTrackerService(new CharacterCatalog(), new JsonWorkspaceRepository(checks.PathFor("classic-1.0.0/reread.json")), new UnreadableLegacyRepository());
            await File.WriteAllBytesAsync(checks.PathFor("classic-1.0.0/reread.json"), original);
            _ = await reread.LoadAsync();
            Assert((await File.ReadAllBytesAsync(checks.PathFor("classic-1.0.0/reread.json"))).SequenceEqual(original),
                "Att bara läsa 1.0.0-filen skriver inte om den.");

            var legacyPath = checks.PathFor("classic-1.0.0/legacy-progress.json");
            await File.WriteAllTextAsync(legacyPath, """
                {"ownedItemIds":["classic-p1-item-13386-of-healing","classic-p1-item-18727"],"equippedItems":{"Back":"classic-p1-item-13386-of-healing"}}
                """);
            var legacyOriginal = await File.ReadAllBytesAsync(legacyPath);
            var migrated = await new CharacterTrackerService(new CharacterCatalog(), new JsonWorkspaceRepository(checks.PathFor("classic-1.0.0/migrated.json")),
                new JsonProgressRepository(legacyPath)).LoadAsync();
            Assert(Entry(migrated, "classic-p1-item-13386-of-healing").IsEquipped && Entry(migrated, "classic-p1-item-18727").IsOwned &&
                (await File.ReadAllBytesAsync(legacyPath)).SequenceEqual(legacyOriginal), "Äldsta framstegsfilen migreras fortfarande och bevaras.");
        });
    }

    // Literal 1.0.0 workspace shape (camelCase, enum names). The second character predates catalogSetId.
    private const string ReleaseOneWorkspace = """
        {
          "schemaVersion": 1,
          "activeCharacterId": "6f0c1b5e-2a43-4c1e-9d61-0a1b2c3d4e5f",
          "characters": [
            {
              "id": "6f0c1b5e-2a43-4c1e-9d61-0a1b2c3d4e5f",
              "name": "My Priest",
              "version": "Classic",
              "class": "Priest",
              "selectedSpecialization": "holy",
              "ownedItemKeys": [ "item:11923:", "item:11928:", "item:12930:", "item:13386:of Healing", "item:16997:", "item:18103:" ],
              "equippedBySpec": {
                "holy": {
                  "Back": "classic-p1-item-13386-of-healing",
                  "Finger2": "classic-p1-item-18103",
                  "MainHand": "classic-p1-item-11923",
                  "OffHand": "classic-p1-item-11928",
                  "Ranged": "classic-p1-item-16997"
                },
                "discipline": {},
                "shadow": {}
              },
              "catalogSetId": "classic-phase1-level60",
              "archivedLoadouts": {}
            },
            {
              "id": "0d6f8e2a-91b4-4f37-8c5d-7e6a5b4c3d2e",
              "name": "Old Priest",
              "version": "Classic",
              "class": "Priest",
              "selectedSpecialization": "holy",
              "ownedItemKeys": [ "item:11766:of Healing", "item:18727:" ],
              "equippedBySpec": { "holy": { "Head": "classic-p1-item-18727" }, "discipline": {}, "shadow": {} }
            }
          ]
        }
        """;

    // Fictional equipment and URLs exist only in disposable checks, never in product data.
    private static JsonObject Fixture(string characterClass = "Mage", string spec = "frost")
    {
        var id = $"classic-phase1-level60-{characterClass.ToLowerInvariant()}-{spec}";
        return new()
        {
            ["schemaVersion"] = 1, ["catalogId"] = id, ["gameVersion"] = "Classic", ["characterClass"] = characterClass,
            ["specializationId"] = spec, ["levelCap"] = 60, ["releaseStage"] = "Classic", ["phase"] = ReviewedCatalogReader.ClassicPhase,
            ["sourceUrl"] = "https://example.com/test-only-classic-guide", ["reviewedOn"] = "2026-10-08",
            ["selectionMethod"] = "TEST ONLY fictional Classic fixture", ["status"] = "Partial",
            ["items"] = new JsonArray(Item(id, "Head", 900101, "Dungeon"))
        };
    }

    private static JsonObject ForeverFixture() => new()
    {
        ["schemaVersion"] = 1, ["catalogId"] = "forever-launch-level60-mage-frost", ["gameVersion"] = "Forever", ["characterClass"] = "Mage",
        ["specializationId"] = "frost", ["levelCap"] = 60, ["releaseStage"] = "Launch", ["phase"] = "TEST ONLY level 60",
        ["sourceUrl"] = "https://example.com/test-only-guide", ["reviewedOn"] = "2026-10-08",
        ["selectionMethod"] = "TEST ONLY fictional Forever fixture", ["status"] = "Reviewed",
        ["items"] = new JsonArray(Item("classic-phase1-level60-mage-frost", "Head", 900101, "Dungeon"))
    };

    private static JsonObject Item(string catalogId, string slot, int itemId, string source) => new()
    {
        ["id"] = $"{catalogId}-{slot.ToLowerInvariant()}-{itemId}", ["slot"] = slot, ["itemId"] = itemId, ["name"] = "TEST ONLY Classic item",
        ["requiredSuffix"] = null, ["requiredLevel"] = 60, ["acquisitionType"] = source, ["source"] = "TEST ONLY fixture",
        ["note"] = "TEST ONLY", ["iconUrl"] = null, ["itemUrl"] = "https://example.com/test-only-item",
        ["recommendationUrl"] = "https://example.com/test-only-guide", ["weaponKind"] = "None", ["uniqueEquipped"] = false
    };

    private static JsonObject Row(JsonObject document, int index) => document["items"]!.AsArray()[index]!.AsObject();

    private static ReviewedCatalogPack Read(JsonObject document, bool otherSources = false)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()), writable: false);
        return ReviewedCatalogReader.Read(stream, otherSources);
    }

    private static void Invalid(Action<JsonObject> mutate) => InvalidFrom(Fixture(), mutate);

    private static void InvalidFrom(JsonObject valid, Action<JsonObject> mutate)
    {
        var document = valid.DeepClone().AsObject();
        mutate(document);
        Throws(() => Read(document));
    }

    private static void Throws(Action action)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidOperationException("Ett ogiltigt katalogpaket accepterades.");
    }

    private sealed class UnreadableLegacyRepository : IProgressRepository
    {
        public Task<ProgressState> LoadAsync(CancellationToken cancellationToken = default) => throw new InvalidOperationException("Workspace exists; legacy data must not be read.");
        public Task SaveAsync(ProgressState state, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Legacy data must never be written.");
    }
}
