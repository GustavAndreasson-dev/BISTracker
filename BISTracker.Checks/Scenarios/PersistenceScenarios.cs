using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Infrastructure;
using static BISTracker.Checks.Scenarios.CheckAssertions;

namespace BISTracker.Checks.Scenarios;
internal static class PersistenceScenarios
{
    public static async Task RunAsync(CheckRun checks)
    {
        await checks.RunAsync("Saknad fil och katalog ger tomt framsteg", async () =>
        {
            var state = await checks.Repository("missing/subdirectory/progress.json").LoadAsync();
            Assert(state.OwnedItemIds.Length == 0 && state.EquippedItems.Count == 0, "Saknad fil ska ge tomt tillstånd.");
        });

        await checks.RunAsync("JSON kan återläsas i nya repository- och serviceinstanser", async () =>
        {
            var service = checks.Service("roundtrip/progress.json");
            await service.SetOwnedAsync("demo-neck", true);
            await service.SetEquippedAsync("demo-mainhand", true);
            var snapshot = await checks.Service("roundtrip/progress.json").LoadAsync();
            Assert(Entry(snapshot, "demo-neck").IsOwned && !Entry(snapshot, "demo-neck").IsEquipped,
                "Separat ägande ska bevaras vid återläsning.");
            Assert(Entry(snapshot, "demo-mainhand").IsOwned && Entry(snapshot, "demo-mainhand").IsEquipped,
                "Utrustning ska bevaras vid återläsning.");
            Assert(Directory.GetFiles(Path.GetDirectoryName(checks.PathFor("roundtrip/progress.json"))!, "*.tmp").Length == 0,
                "Lyckade skrivningar ska inte lämna temporära filer.");
        });

        await checks.RunAsync("Korrupt JSON ger läsfel och får inte skrivas över", async () =>
        {
            const string corrupt = "{ ogiltig JSON";
            await File.WriteAllTextAsync(checks.PathFor("corrupt.json"), corrupt);
            var repository = checks.Repository("corrupt.json");
            await Throws<InvalidDataException>(() => repository.LoadAsync());
            await Throws<InvalidDataException>(() => repository.SaveAsync(new ProgressState([], [])));
            await Throws<InvalidDataException>(() => checks.Service("corrupt.json").SetOwnedAsync("demo-head", true));
            Assert(await File.ReadAllTextAsync(checks.PathFor("corrupt.json")) == corrupt, "Korrupt fil ska bevaras för återställning.");
        });

        await checks.RunAsync("JSON med saknade eller null-samlingar ger läsfel", async () =>
        {
            foreach (var json in new[] { "null", "{}", "{\"ownedItemIds\":null,\"equippedItems\":{}}",
                "{\"ownedItemIds\":[],\"equippedItems\":null}",
                "{\"ownedItemIds\":[],\"equippedItems\":{\"UnknownSlot\":\"demo-head\"}}" })
            {
                await File.WriteAllTextAsync(checks.PathFor("invalid-shape.json"), json);
                await Throws<InvalidDataException>(() => checks.Repository("invalid-shape.json").LoadAsync());
            }
        });

        await checks.RunAsync("Avbruten sparning bevarar tidigare framsteg", async () =>
        {
            var repository = checks.Repository("cancel.json");
            await repository.SaveAsync(new ProgressState(["demo-head"], []));
            var original = await File.ReadAllTextAsync(checks.PathFor("cancel.json"));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Throws<OperationCanceledException>(() => repository.SaveAsync(new ProgressState([], []), cancellation.Token));
            Assert(await File.ReadAllTextAsync(checks.PathFor("cancel.json")) == original, "Avbruten sparning ska bevara tidigare fil.");
        });

        await checks.RunAsync("Låst målfil: ersättningsfel bevarar JSON och städar temporär fil", async () =>
        {
            var repository = checks.Repository("locked/progress.json");
            await repository.SaveAsync(new ProgressState(["demo-head"], []));
            var original = await File.ReadAllTextAsync(checks.PathFor("locked/progress.json"));
            // Allow reads, but prevent replacement/deletion of the target on Windows.
            await using (var lockedFile = new FileStream(checks.PathFor("locked/progress.json"), FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                await Throws<IOException>(() => repository.SaveAsync(new ProgressState(["demo-neck"], [])));
                Assert(await File.ReadAllTextAsync(checks.PathFor("locked/progress.json")) == original,
                    "Misslyckad ersättning ska bevara föregående fil.");
                Assert(Directory.GetFiles(Path.GetDirectoryName(checks.PathFor("locked/progress.json"))!, "*.tmp").Length == 0,
                    "Misslyckad ersättning ska städa sin temporära fil.");
            }
        });

        await checks.RunAsync("REQ-015: schema 1 med noll karaktärer kräver tomt aktivt ID; ogiltiga tomma filer bevaras", async () =>
        {
            var path = checks.PathFor("zero-characters/characters-v1.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            const string empty = "{\"schemaVersion\":1,\"activeCharacterId\":\"00000000-0000-0000-0000-000000000000\",\"characters\":[]}";
            await File.WriteAllTextAsync(path, empty);
            var repository = new JsonWorkspaceRepository(path);
            var loaded = (await repository.LoadAsync())!;
            Assert(loaded is { SchemaVersion: 1, Characters.Length: 0 } && loaded.ActiveCharacterId == Guid.Empty, "Tom schema 1-workspace läses.");
            await repository.SaveAsync(loaded);
            var saved = (await new JsonWorkspaceRepository(path).LoadAsync())!;
            Assert(saved.Characters.Length == 0 && saved.ActiveCharacterId == Guid.Empty, "Tom workspace sparas och återläses.");
            await Throws<ArgumentException>(() => repository.SaveAsync(new WorkspaceState(1, Guid.NewGuid(), [])));
            await Throws<ArgumentException>(() => repository.SaveAsync(new WorkspaceState(2, Guid.Empty, [])));
            foreach (var content in new[]
            {
                "{\"schemaVersion\":1,\"activeCharacterId\":\"" + Guid.NewGuid() + "\",\"characters\":[]}",
                "{\"schemaVersion\":1,\"activeCharacterId\":\"00000000-0000-0000-0000-000000000000\",\"characters\":null}",
                "{\"schemaVersion\":1,\"activeCharacterId\":\"00000000-0000-0000-0000-000000000000\"}",
                "{\"schemaVersion\":2,\"activeCharacterId\":\"00000000-0000-0000-0000-000000000000\",\"characters\":[]}"
            })
            {
                var invalidPath = checks.PathFor("zero-characters/invalid.json");
                await File.WriteAllTextAsync(invalidPath, content);
                var invalid = new JsonWorkspaceRepository(invalidPath);
                await Throws<InvalidDataException>(() => invalid.LoadAsync());
                await Throws<InvalidDataException>(() => invalid.SaveAsync(loaded));
                Assert(await File.ReadAllTextAsync(invalidPath) == content, "Ogiltig tom workspace bevaras.");
            }
        });

        await checks.RunAsync("REQ-014/015: befintliga schema 1-filer (1.0.0 och äldre form) läses och lämnas oförändrade", async () =>
        {
            var catalog = await new ClassicPhaseOneBisCatalog().LoadAsync();
            var item = catalog.Items[0];
            // 1.0.0 writes catalogSetId and archivedLoadouts; the older schema 1 shape lacks both.
            foreach (var withCatalogSet in new[] { true, false })
            {
                var id = Guid.NewGuid();
                var content = SchemaOneWorkspace(id, item, withCatalogSet);
                var path = checks.PathFor($"v1-workspace-{withCatalogSet}/characters-v1.json");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllTextAsync(path, content);
                var service = new CharacterTrackerService(new CharacterCatalog(), new JsonWorkspaceRepository(path), new CharacterScenarios.UnreadableLegacyRepository());
                var snapshot = await service.LoadAsync();
                Assert(snapshot.Selection!.ActiveCharacter.Id == id && snapshot.Selection.Characters.Count == 1 &&
                    Entry(snapshot, item.Id).IsOwned && Entry(snapshot, item.Id).IsEquipped, "Befintlig karaktär och framsteg läses.");
                Assert(await File.ReadAllTextAsync(path) == content, "Läsning skriver inte om en befintlig fil och legacy-filen läses inte.");
            }
        });
    }

    private static string SchemaOneWorkspace(Guid id, Recommendation item, bool withCatalogSet) => string.Join("\n",
    [
        "{",
        "  \"schemaVersion\": 1,",
        $"  \"activeCharacterId\": \"{id}\",",
        "  \"characters\": [",
        "    {",
        $"      \"id\": \"{id}\",",
        "      \"name\": \"My Priest\",",
        "      \"version\": \"Classic\",",
        "      \"class\": \"Priest\",",
        "      \"selectedSpecialization\": \"holy\",",
        $"      \"ownedItemKeys\": [ \"{CharacterLoadouts.ItemKey(item)}\" ],",
        $"      \"equippedBySpec\": {{ \"discipline\": {{}}, \"holy\": {{ \"{item.Slot}\": \"{item.Id}\" }}, \"shadow\": {{}} }}" + (withCatalogSet ? "," : ""),
        .. withCatalogSet ? new[] { $"      \"catalogSetId\": \"{CatalogSet.Default(GameVersion.Classic).Id}\",", "      \"archivedLoadouts\": {}" } : [],
        "    }",
        "  ]",
        "}",
        ""
    ]);
}
