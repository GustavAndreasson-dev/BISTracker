using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Infrastructure;
using static BISTracker.Checks.Scenarios.CheckAssertions;

namespace BISTracker.Checks.Scenarios;

internal static class CharacterScenarios
{
    public static async Task RunAsync(CheckRun checks)
    {
        await checks.RunAsync("Två spelversioner, nio klasser och tre giltiga specs; otillgänglig data är explicit", async () =>
        {
            var catalogs = new CharacterCatalog();
            foreach (var version in Enum.GetValues<GameVersion>())
                foreach (var characterClass in Enum.GetValues<CharacterClass>())
                {
                    var specs = CharacterDefinition.Specializations(characterClass);
                    Assert(specs.Count == 3 && specs.Select(spec => spec.Id).Distinct().Count() == 3, "Exakt tre specs per klass.");
                    foreach (var spec in specs)
                    {
                        var catalog = await catalogs.LoadAsync(version, characterClass, spec.Id);
                        Assert(catalog.Context.Version == CharacterDefinition.VersionName(version), "Rätt spelversion i katalogen.");
                        var reviewed = version == GameVersion.Classic && characterClass == CharacterClass.Priest && spec.Id == "holy";
                        Assert(reviewed ? catalog.Items.Count == 17 && catalog.UnavailableReason is null :
                            version == GameVersion.Forever ? catalog.Items.Count > 0 && catalog.UnavailableReason is null && catalog.Set?.LevelCap == 30 &&
                                catalog.Items.All(item => item.Details!.AcquisitionType is AcquisitionType.Dungeon or AcquisitionType.Quest) :
                                catalog.Items.Count == 0 && catalog.UnavailableReason is not null, "Rätt granskad katalog och källpolicy för varje kontext.");
                    }
                }
            await Throws<ArgumentException>(() => catalogs.LoadAsync(GameVersion.Forever, CharacterClass.Mage, "holy"));
            await Throws<ArgumentException>(() => catalogs.LoadAsync((GameVersion)999, CharacterClass.Mage, "fire"));
        });

        await checks.RunAsync("Äldre riktiga framsteg importeras en gång; originalfilen bevaras", async () =>
        {
            var catalog = await new ClassicPhaseOneBisCatalog().LoadAsync();
            var id = catalog.Items[0].Id;
            var legacy = checks.Repository("migration-legacy.json");
            await legacy.SaveAsync(new ProgressState([id], new() { [catalog.Items[0].Slot] = id }));
            var original = await File.ReadAllTextAsync(checks.PathFor("migration-legacy.json"));
            var workspace = new JsonWorkspaceRepository(checks.PathFor("migration-characters.json"));
            var service = new CharacterTrackerService(new CharacterCatalog(), workspace, legacy);
            var migrated = await service.LoadAsync();
            Assert(Entry(migrated, id).IsOwned && Entry(migrated, id).IsEquipped, "Ägande och Holy-utrustning migreras.");
            Assert(migrated.Selection!.Characters.Count == 1 && (await workspace.LoadAsync())!.Characters[0].EquippedBySpec.Count == 3, "Tre listor skapades.");
            await service.SetOwnedAsync(id, false);
            Assert(await File.ReadAllTextAsync(checks.PathFor("migration-legacy.json")) == original, "Gamla filen ska vara byte-för-byte bevarad.");
            // Prove the old file is no longer consulted once the new workspace exists.
            var restart = new CharacterTrackerService(new CharacterCatalog(), workspace, new UnreadableLegacyRepository());
            var reloaded = await restart.LoadAsync();
            Assert(!Entry(reloaded, id).IsOwned && reloaded.Selection!.ActiveCharacter.Id == migrated.Selection.ActiveCharacter.Id,
                "Migration upprepas inte och karaktärens ID är stabilt.");
        });

        await checks.RunAsync("Gemensamt itemägande skiljs från spec-rekommendation, slot och suffix", async () =>
        {
            var service = Service(checks, "identity");
            var initial = await service.LoadAsync();
            var character = initial.Selection!.ActiveCharacter.Id;
            await service.SetEquippedAsync("holy-head", true);
            var discipline = await service.SelectAsync(character, "discipline");
            Assert(Entry(discipline, "discipline-head").IsOwned && !Entry(discipline, "discipline-head").IsEquipped,
                "Samma verkliga item ägs i andra specs, men utrustas inte automatiskt.");
            Assert(!Entry(discipline, "discipline-other-suffix").IsOwned, "Annat suffix är en annan variant.");
            Assert(Entry(discipline, "discipline-finger2").IsOwned, "Itemidentitet ska fungera över rekommendationsslots.");
            await service.SetEquippedAsync("discipline-head", true);
            await service.SetEquippedAsync("discipline-head", false);
            var holy = await service.SelectAsync(character, "holy");
            Assert(Entry(holy, "holy-head").IsEquipped, "Avmarkera utrustad påverkar bara vald spec.");
        });

        await checks.RunAsync("Tre olika listor, karaktärer och spelversioner bevaras efter omstart", async () =>
        {
            var service = Service(checks, "restart");
            var first = (await service.LoadAsync()).Selection!.ActiveCharacter.Id;
            foreach (var spec in new[] { "holy", "discipline", "shadow" })
            {
                await service.SelectAsync(first, spec);
                await service.SetEquippedAsync($"{spec}-head", true);
            }
            var second = await service.CreateAsync("Forever Priest", GameVersion.Forever, CharacterClass.Priest);
            Assert(second.Entries.All(item => !item.IsOwned && !item.IsEquipped), "Spelversioner får inte dela karaktärsägande.");
            await service.SetEquippedAsync("discipline-chest", true);
            var third = await service.CreateAsync("Other Priest", GameVersion.Classic, CharacterClass.Priest);
            Assert(third.Entries.All(item => !item.IsOwned), "Karaktärer inom samma version ska också isoleras.");
            var secondId = second.Selection!.ActiveCharacter.Id;
            var restarted = Service(checks, "restart");
            Assert((await restarted.LoadAsync()).Selection!.ActiveCharacter.Id == third.Selection!.ActiveCharacter.Id, "Aktiv karaktär återläses.");
            foreach (var spec in new[] { "holy", "discipline", "shadow" })
            {
                var snapshot = await restarted.SelectAsync(first, spec);
                Assert(Entry(snapshot, $"{spec}-head").IsEquipped && !Entry(snapshot, $"{spec}-chest").IsEquipped, "Specens separata utrustning återläses.");
            }
            Assert(Entry(await restarted.SelectAsync(secondId, "discipline"), "discipline-chest").IsEquipped, "Andra karaktären bevaras.");
            Assert((await Service(checks, "restart").LoadAsync()).Selection!.ActiveSpecialization.Id == "discipline", "Aktiv spec återläses.");
        });

        await checks.RunAsync("Avmarkerat ägande rensar alla tre utrustningslistor; slotbyte och parallella sparningar är säkra", async () =>
        {
            var service = Service(checks, "ownership");
            var character = (await service.LoadAsync()).Selection!.ActiveCharacter.Id;
            foreach (var spec in new[] { "holy", "discipline", "shadow" })
            {
                await service.SelectAsync(character, spec);
                await service.SetEquippedAsync($"{spec}-head", true);
            }
            await service.SetOwnedAsync("shadow-head", false);
            foreach (var spec in new[] { "holy", "discipline", "shadow" })
                Assert(!Entry(await service.SelectAsync(character, spec), $"{spec}-head").IsEquipped, "Borttaget ägande ska rensa alla specs.");
            await service.SetEquippedAsync("shadow-head", true);
            await service.SetEquippedAsync("shadow-other-suffix", true);
            await service.SetEquippedAsync("shadow-head", false);
            var alternate = await service.LoadAsync();
            Assert(Entry(alternate, "shadow-head").IsOwned && !Entry(alternate, "shadow-head").IsEquipped && Entry(alternate, "shadow-other-suffix").IsEquipped,
                "Slotbyte behåller ägande; avmarkera annat item får inte ta bort utrustningen.");
            await Task.WhenAll(service.SetOwnedAsync("shadow-chest", true), service.SetOwnedAsync("shadow-finger2", true));
            var saved = await Service(checks, "ownership").LoadAsync();
            Assert(Entry(saved, "shadow-chest").IsOwned && Entry(saved, "shadow-finger2").IsOwned, "Inga uppdateringar tappas inom tjänsten.");
        });

        await checks.RunAsync("Sparfel, fel spec, fel item och avbrott bevarar sparade listor och aktivt val", async () =>
        {
            var path = checks.PathFor("failed-workspace.json");
            var repository = new FailOnSaveRepository(new JsonWorkspaceRepository(path));
            var service = new CharacterTrackerService(new FixtureCatalog(), repository, checks.Repository("failed-old.json"));
            var initial = await service.LoadAsync();
            var original = await File.ReadAllTextAsync(path);
            repository.Fail = true;
            await Throws<IOException>(() => service.SetEquippedAsync("holy-head", true));
            await Throws<IOException>(() => service.SelectAsync(initial.Selection!.ActiveCharacter.Id, "shadow"));
            await Throws<IOException>(() => service.CreateAsync("New", GameVersion.Forever, CharacterClass.Mage));
            repository.Fail = false;
            await Throws<ArgumentException>(() => service.SelectAsync(initial.Selection!.ActiveCharacter.Id, "fire"));
            await Throws<ArgumentException>(() => service.SetOwnedAsync("unknown", true));
            await Throws<ArgumentException>(() => service.CreateAsync(" ", GameVersion.Classic, CharacterClass.Mage));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Throws<OperationCanceledException>(() => service.SetOwnedAsync("holy-head", true, cancellation.Token));
            Assert(await File.ReadAllTextAsync(path) == original, "Ingen feloperation får ändra filen.");
            var after = await service.LoadAsync();
            Assert(after.Selection!.ActiveSpecialization.Id == "holy" && after.Selection.Characters.Count == 1 && after.Entries.All(item => !item.IsOwned),
                "Fel får inte synas som sparade resultat.");
        });

        await checks.RunAsync("Korrupta filer och okända scheman skrivs inte över vid läsning eller direkt sparning", async () =>
        {
            var valid = await Service(checks, "valid-shape").LoadAsync();
            var validState = (await new JsonWorkspaceRepository(checks.PathFor("valid-shape-workspace.json")).LoadAsync())!;
            foreach (var content in new[] { "{broken", "null", "{}", "{\"schemaVersion\":2}",
                System.Text.Json.JsonSerializer.Serialize(validState with { Characters = [validState.Characters[0] with { EquippedBySpec = new() }] }) })
            {
                var path = checks.PathFor("corrupt-workspace.json");
                await File.WriteAllTextAsync(path, content);
                var repository = new JsonWorkspaceRepository(path);
                await Throws<InvalidDataException>(() => repository.LoadAsync());
                await Throws<InvalidDataException>(() => repository.SaveAsync(validState));
                Assert(await File.ReadAllTextAsync(path) == content, "Korrupt/för ny fil måste bevaras.");
            }
            var invalidDomain = validState with { Characters = [validState.Characters[0] with { OwnedItemKeys = ["unknown-key"] }] };
            var semanticRepository = new JsonWorkspaceRepository(checks.PathFor("semantic-invalid.json"));
            await semanticRepository.SaveAsync(invalidDomain);
            var semanticService = new CharacterTrackerService(new FixtureCatalog(), semanticRepository, checks.Repository("unused-legacy.json"));
            await Throws<ArgumentException>(() => semanticService.CreateAsync("New", GameVersion.Classic, CharacterClass.Mage));
            Assert((await semanticRepository.LoadAsync())!.Characters.Length == 1, "Även inaktiva domänfel ska avvisa hela ändringen.");
            await File.WriteAllTextAsync(checks.PathFor("invalid-legacy.json"), "{bad");
            var migration = new CharacterTrackerService(new FixtureCatalog(), new JsonWorkspaceRepository(checks.PathFor("no-overwrite.json")), checks.Repository("invalid-legacy.json"));
            await Throws<InvalidDataException>(() => migration.LoadAsync());
            Assert(!File.Exists(checks.PathFor("no-overwrite.json")), "Misslyckad migration får inte skapa nytt tillstånd.");
        });

        await checks.RunAsync("Låst JSON-mål och avbruten sparning lämnar tidigare data och inga tempfiler", async () =>
        {
            var service = Service(checks, "locked");
            await service.LoadAsync();
            var path = checks.PathFor("locked-workspace.json");
            var repository = new JsonWorkspaceRepository(path);
            var state = (await repository.LoadAsync())!;
            var original = await File.ReadAllBytesAsync(path);
            using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                await Throws<IOException>(() => repository.SaveAsync(state));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Throws<OperationCanceledException>(() => repository.SaveAsync(state, cancellation.Token));
            Assert((await File.ReadAllBytesAsync(path)).SequenceEqual(original), "Tidigare fil bevaras.");
            Assert(Directory.GetFiles(Path.GetDirectoryName(path)!, ".locked-workspace.json.*.tmp").Length == 0, "Tempfiler rensas.");
        });

        await checks.RunAsync("Domänen avvisar fel antal specs, fel slot, okänt ägande och oägd utrustning; kopior är fristående", () =>
        {
            var lists = CharacterDefinition.Specializations(CharacterClass.Priest).ToDictionary(spec => spec.Id,
                spec => (IReadOnlyList<Recommendation>)FixtureCatalog.Items(spec.Id));
            var equipment = lists.ToDictionary(pair => pair.Key, _ => new Dictionary<EquipmentSlot, string>());
            var progress = new CharacterLoadouts(CharacterClass.Priest, lists, [], equipment);
            progress.SetEquipped("holy", "holy-head", true);
            var copy = progress.EquippedBySpec;
            copy["holy"].Clear();
            Assert(progress.IsEquipped("holy", "holy-head"), "Exponerad kopia kan inte ändra domänens tillstånd.");
            AssertRejects(() => new CharacterLoadouts(CharacterClass.Priest, lists, ["invalid"], equipment));
            AssertRejects(() => new CharacterLoadouts(CharacterClass.Priest, new Dictionary<string, IReadOnlyList<Recommendation>>(), [], equipment));
            equipment["holy"][EquipmentSlot.Chest] = "holy-head";
            AssertRejects(() => new CharacterLoadouts(CharacterClass.Priest, lists, progress.OwnedItemKeys, equipment));
            equipment["holy"].Clear();
            equipment["holy"][EquipmentSlot.Head] = "holy-head";
            AssertRejects(() => new CharacterLoadouts(CharacterClass.Priest, lists, [], equipment));
            return Task.CompletedTask;
        });
    }

    private static CharacterTrackerService Service(CheckRun checks, string name) => new(new FixtureCatalog(),
        new JsonWorkspaceRepository(checks.PathFor(name + "-workspace.json")), checks.Repository(name + "-legacy.json"));

    private static void AssertRejects(Action action)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        throw new InvalidOperationException("Ogiltigt tillstånd accepterades.");
    }

    // Deliberately fictional metadata, confined to behavior checks; never shipped as a catalog.
    private sealed class FixtureCatalog : ICharacterCatalog
    {
        public Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string specializationId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var spec = CharacterDefinition.Specialization(characterClass, specializationId);
            return Task.FromResult(new BisCatalog(new GameContext(CharacterDefinition.VersionName(version), $"{spec.Name} {characterClass}", "TEST ONLY"), Items(spec.Id), true));
        }

        public static Recommendation[] Items(string spec) =>
        [
            Item(spec + "-head", EquipmentSlot.Head, 1, "of Healing"),
            Item(spec + "-other-suffix", EquipmentSlot.Head, 1, "of Intellect"),
            Item(spec + "-chest", EquipmentSlot.Chest, 2, null),
            Item(spec + "-finger2", EquipmentSlot.Finger2, 1, "of Healing")
        ];
        private static Recommendation Item(string id, EquipmentSlot slot, int itemId, string? suffix) =>
            new(id, slot, "TEST ONLY", "Fictional fixture", "", new ItemDetails(itemId, suffix, AcquisitionType.Dungeon,
                "https://example.com/icon", "https://example.com/item", "https://example.com/guide"));
    }

    private sealed class UnreadableLegacyRepository : IProgressRepository
    {
        public Task<ProgressState> LoadAsync(CancellationToken cancellationToken = default) => throw new InvalidOperationException("Must not read legacy data twice.");
        public Task SaveAsync(ProgressState state, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Must never write legacy data.");
    }

    private sealed class FailOnSaveRepository(IWorkspaceRepository inner) : IWorkspaceRepository
    {
        public bool Fail { get; set; }
        public Task<WorkspaceState?> LoadAsync(CancellationToken cancellationToken = default) => inner.LoadAsync(cancellationToken);
        public Task SaveAsync(WorkspaceState state, CancellationToken cancellationToken = default) => Fail
            ? throw new IOException("Simulated write failure.") : inner.SaveAsync(state, cancellationToken);
    }
}
