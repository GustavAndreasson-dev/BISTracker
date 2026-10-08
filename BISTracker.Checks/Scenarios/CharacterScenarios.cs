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
                                catalog.Items.All(item => item.Details!.AcquisitionType is AcquisitionType.Dungeon or AcquisitionType.Quest or AcquisitionType.Crafting) :
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

        await checks.RunAsync("REQ-014: namnbyte sparas efter omstart och ändrar inte version, klass, ägande eller utrustning", async () =>
        {
            var service = Service(checks, "rename");
            var priest = (await service.LoadAsync()).Selection!.ActiveCharacter.Id;
            await service.SetEquippedAsync("holy-head", true);
            await service.SelectAsync(priest, "discipline");
            await service.SetEquippedAsync("discipline-chest", true);
            var mage = (await service.CreateAsync("Forever Mage", GameVersion.Forever, CharacterClass.Mage)).Selection!.ActiveCharacter.Id;
            var repository = new JsonWorkspaceRepository(checks.PathFor("rename-workspace.json"));
            var before = (await repository.LoadAsync())!;
            var renamed = await service.RenameAsync(priest, "  Renamed Priest  ");
            Assert(renamed.Selection!.ActiveCharacter.Id == mage && renamed.Selection.Characters.Single(item => item.Id == priest).Name == "Renamed Priest",
                "Namnet trimmas och aktivt val ändras inte av namnbyte.");
            var after = (await repository.LoadAsync())!;
            Assert(after.ActiveCharacterId == before.ActiveCharacterId && after.Characters.Length == 2, "Workspace behåller aktiv karaktär och antal.");
            Assert(Json(after.Characters.Single(item => item.Id == priest)) == Json(before.Characters.Single(item => item.Id == priest) with { Name = "Renamed Priest" }) &&
                Json(after.Characters.Single(item => item.Id == mage)) == Json(before.Characters.Single(item => item.Id == mage)),
                "Endast namnet ändras; version, klass, spec, ägande, utrustning och andra karaktärer är oförändrade.");
            var restarted = Service(checks, "rename");
            var holy = await restarted.SelectAsync(priest, "holy");
            Assert(holy.Selection!.ActiveCharacter is { Name: "Renamed Priest", Version: GameVersion.Classic, Class: CharacterClass.Priest } &&
                Entry(holy, "holy-head").IsEquipped && Entry(holy, "holy-chest").IsOwned, "Namn och framsteg återläses efter omstart.");
            Assert(Entry(await restarted.SelectAsync(priest, "discipline"), "discipline-chest").IsEquipped, "Annan specs utrustning bevaras.");
        });

        await checks.RunAsync("REQ-014: ogiltiga namn och okänd karaktär avvisas före ändring", async () =>
        {
            var service = Service(checks, "rename-invalid");
            var id = (await service.LoadAsync()).Selection!.ActiveCharacter.Id;
            var path = checks.PathFor("rename-invalid-workspace.json");
            var original = await File.ReadAllTextAsync(path);
            foreach (var name in new[] { "", "   ", new string('x', 41), "Bad\nName", "Tab\tName" })
                await Throws<ArgumentException>(() => service.RenameAsync(id, name));
            await Throws<ArgumentException>(() => service.RenameAsync(Guid.NewGuid(), "Valid"));
            await Throws<ArgumentException>(() => service.RenameAsync(Guid.Empty, "Valid"));
            Assert(await File.ReadAllTextAsync(path) == original, "Avvisat namnbyte får inte ändra filen.");
            Assert((await service.LoadAsync()).Selection!.ActiveCharacter.Name == "My Priest", "Avvisat namnbyte syns inte i resultatet.");
            Assert((await service.RenameAsync(id, new string('y', 40))).Selection!.ActiveCharacter.Name.Length == 40, "40 tecken är tillåtet som vid skapande.");
        });

        await checks.RunAsync("REQ-015: borttagning av inaktiv och aktiv karaktär lämnar övriga karaktärers data orörda", async () =>
        {
            var service = Service(checks, "delete");
            var first = (await service.LoadAsync()).Selection!.ActiveCharacter.Id;
            await service.SetEquippedAsync("holy-head", true);
            var second = (await service.CreateAsync("Second", GameVersion.Forever, CharacterClass.Priest)).Selection!.ActiveCharacter.Id;
            await service.SetEquippedAsync("discipline-chest", true);
            var third = (await service.CreateAsync("Third", GameVersion.Classic, CharacterClass.Mage)).Selection!.ActiveCharacter.Id;
            var repository = new JsonWorkspaceRepository(checks.PathFor("delete-workspace.json"));
            var before = (await repository.LoadAsync())!;
            var inactive = await service.DeleteAsync(first);
            Assert(inactive.Selection!.ActiveCharacter.Id == third && inactive.Selection.Characters.Select(item => item.Id).SequenceEqual([second, third]),
                "Borttagen inaktiv karaktär försvinner; aktiv karaktär är oförändrad.");
            var afterInactive = (await repository.LoadAsync())!;
            Assert(afterInactive.ActiveCharacterId == third && afterInactive.Characters.Select(Json).SequenceEqual(before.Characters.Where(item => item.Id != first).Select(Json)),
                "Övriga karaktärers sparade data är byte-för-byte desamma.");
            var active = await service.DeleteAsync(third);
            Assert(active.Selection!.ActiveCharacter.Id == second && active.Selection.Characters.Count == 1 && Entry(active, "discipline-chest").IsEquipped,
                "Borttagen aktiv karaktär ersätts av första kvarvarande med dess framsteg.");
            var restarted = await Service(checks, "delete").LoadAsync();
            Assert(restarted.Selection!.ActiveCharacter.Id == second && restarted.Selection.Characters.Count == 1 &&
                Json((await repository.LoadAsync())!.Characters[0]) == Json(before.Characters.Single(item => item.Id == second)), "Borttagning återläses efter omstart.");
            await Throws<ArgumentException>(() => service.DeleteAsync(first));
        });

        await checks.RunAsync("REQ-015: sista karaktären tas bort; tom workspace återläses utan legacy-import och ny karaktär kan skapas", async () =>
        {
            var catalog = await new ClassicPhaseOneBisCatalog().LoadAsync();
            var legacy = checks.Repository("empty-legacy.json");
            await legacy.SaveAsync(new ProgressState([catalog.Items[0].Id], new()));
            var path = checks.PathFor("empty-workspace.json");
            var service = new CharacterTrackerService(new CharacterCatalog(), new JsonWorkspaceRepository(path), legacy);
            var migrated = await service.LoadAsync();
            Assert(migrated.Selection!.ActiveCharacter.Name == "My Priest" && Entry(migrated, catalog.Items[0].Id).IsOwned, "Första start importerar legacy en gång.");
            var empty = await service.DeleteAsync(migrated.Selection.ActiveCharacter.Id);
            Assert(empty.Selection is null && empty.Entries.Count == 0 && empty.Catalog == TrackerSnapshot.NoCharacterCatalog, "Tomt läge saknar val och rader.");
            var stored = (await new JsonWorkspaceRepository(path).LoadAsync())!;
            Assert(stored is { SchemaVersion: 1, Characters.Length: 0 } && stored.ActiveCharacterId == Guid.Empty, "Noll karaktärer sparas i schema 1 med tomt aktivt ID.");
            var emptyFile = await File.ReadAllTextAsync(path);
            var restarted = new CharacterTrackerService(new CharacterCatalog(), new JsonWorkspaceRepository(path), new UnreadableLegacyRepository());
            var reloaded = await restarted.LoadAsync();
            Assert(reloaded.Selection is null && reloaded.Entries.Count == 0, "Tom workspace återläses tom; legacy läses inte och My Priest återskapas inte.");
            await Throws<InvalidOperationException>(() => restarted.SetOwnedAsync(catalog.Items[0].Id, true));
            await Throws<InvalidOperationException>(() => restarted.SelectCatalogAsync(CatalogSet.Default(GameVersion.Classic).Id));
            await Throws<ArgumentException>(() => restarted.SelectAsync(Guid.NewGuid(), "holy"));
            await Throws<ArgumentException>(() => restarted.RenameAsync(Guid.NewGuid(), "Name"));
            await Throws<ArgumentException>(() => restarted.DeleteAsync(Guid.NewGuid()));
            Assert(await File.ReadAllTextAsync(path) == emptyFile, "Avvisade operationer i tomt läge ändrar inte filen.");
            var created = await restarted.CreateAsync("Fresh Priest", GameVersion.Classic, CharacterClass.Priest);
            var createdHoly = await restarted.SelectAsync(created.Selection!.ActiveCharacter.Id, "holy");
            Assert(created.Selection.ActiveCharacter.Name == "Fresh Priest" && created.Selection.Characters.Count == 1 && createdHoly.Entries.Count > 0 &&
                createdHoly.Entries.All(entry => !entry.IsOwned), "Ny karaktär från tomt läge blir aktiv utan importerat legacy-ägande.");
            var afterCreate = await new CharacterTrackerService(new CharacterCatalog(), new JsonWorkspaceRepository(path), new UnreadableLegacyRepository()).LoadAsync();
            Assert(afterCreate.Selection!.ActiveCharacter.Id == created.Selection.ActiveCharacter.Id, "Skapad karaktär återläses efter omstart.");
        });

        await checks.RunAsync("REQ-014/015: sparfel vid namnbyte och borttagning lämnar sparat och returnerat tillstånd oförändrat", async () =>
        {
            var path = checks.PathFor("failed-edit-workspace.json");
            var repository = new FailOnSaveRepository(new JsonWorkspaceRepository(path));
            var service = new CharacterTrackerService(new FixtureCatalog(), repository, checks.Repository("failed-edit-legacy.json"));
            var first = (await service.LoadAsync()).Selection!.ActiveCharacter.Id;
            await service.SetEquippedAsync("holy-head", true);
            var second = (await service.CreateAsync("Second", GameVersion.Forever, CharacterClass.Mage)).Selection!.ActiveCharacter.Id;
            var original = await File.ReadAllTextAsync(path);
            repository.Fail = true;
            await Throws<IOException>(() => service.RenameAsync(first, "Renamed"));
            await Throws<IOException>(() => service.DeleteAsync(first));
            await Throws<IOException>(() => service.DeleteAsync(second));
            repository.Fail = false;
            Assert(await File.ReadAllTextAsync(path) == original, "Sparfel får inte ändra filen.");
            var after = await service.LoadAsync();
            Assert(after.Selection!.ActiveCharacter.Id == second && after.Selection.Characters.Count == 2 &&
                after.Selection.Characters.Single(item => item.Id == first).Name == "My Priest", "Sparfel syns inte som lyckat namnbyte eller borttagning.");
            await service.DeleteAsync(second);
            repository.Fail = true;
            await Throws<IOException>(() => service.DeleteAsync(first));
            repository.Fail = false;
            var last = await service.LoadAsync();
            Assert(last.Selection!.ActiveCharacter.Id == first && Entry(last, "holy-head").IsEquipped, "Sparfel vid borttagning av sista karaktären bevarar den.");
        });
    }

    private static string Json(CharacterState character) => System.Text.Json.JsonSerializer.Serialize(character);

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

    internal sealed class UnreadableLegacyRepository : IProgressRepository
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
