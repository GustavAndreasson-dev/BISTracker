using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Infrastructure;
using static BISTracker.Checks.Scenarios.CheckAssertions;

namespace BISTracker.Checks.Scenarios;

internal static class CatalogContextScenarios
{
    public static async Task RunAsync(CheckRun checks)
    {
        await checks.RunAsync("Nivå 30 och 60 bevarar tre separata listor och gemensamt ägande efter omstart", async () =>
        {
            var service = Service(checks, "levels");
            var character = (await service.CreateAsync("Forever", GameVersion.Forever, CharacterClass.Priest)).Selection!.ActiveCharacter.Id;
            foreach (var spec in new[] { "holy", "discipline", "shadow" })
            {
                await service.SelectAsync(character, spec);
                await service.SetEquippedAsync($"l30-{spec}-head", true);
            }
            var sixty = await service.SelectCatalogAsync(CatalogSet.ForeverLevelSixty.Id);
            Assert(Entry(sixty, "l60-shadow-head").IsOwned && !Entry(sixty, "l60-shadow-head").IsEquipped,
                "Itemägande delas inom karaktären, utrustning ska vara separat mellan nivåer.");
            await service.SetEquippedAsync("l60-shadow-chest", true);
            var restart = Service(checks, "levels");
            Assert((await restart.LoadAsync()).Selection!.ActiveCatalogSet!.LevelCap == 60, "Vald nivå återläses.");
            await restart.SelectCatalogAsync(CatalogSet.ForeverLevelThirty.Id);
            foreach (var spec in new[] { "holy", "discipline", "shadow" })
                Assert(Entry(await restart.SelectAsync(character, spec), $"l30-{spec}-head").IsEquipped, "Beta-listorna bevaras.");
            await restart.SelectCatalogAsync(CatalogSet.ForeverLevelSixty.Id);
            await restart.SelectAsync(character, "shadow");
            Assert(Entry(await restart.LoadAsync(), "l60-shadow-chest").IsEquipped, "Nivå60-utrustningen bevaras.");
            await restart.SetOwnedAsync("l60-shadow-head", false);
            await restart.SelectCatalogAsync(CatalogSet.ForeverLevelThirty.Id);
            foreach (var spec in new[] { "holy", "discipline", "shadow" })
                Assert(!Entry(await restart.SelectAsync(character, spec), $"l30-{spec}-head").IsEquipped, "Borttaget ägande rensar även arkiverade nivåer.");
        });

        await checks.RunAsync("Nivåval avvisar fel version och gamla v1-filer utan kontext läses förlustfritt", async () =>
        {
            var service = Service(checks, "legacy-context");
            var snapshot = await service.LoadAsync();
            await Throws<ArgumentException>(() => service.SelectCatalogAsync(CatalogSet.ForeverLevelThirty.Id));
            var repository = new JsonWorkspaceRepository(checks.PathFor("legacy-context-workspace.json"));
            var state = (await repository.LoadAsync())!;
            state = state with { Characters = [state.Characters[0] with { CatalogSetId = null, ArchivedLoadouts = null }] };
            await repository.SaveAsync(state);
            var restored = await Service(checks, "legacy-context").LoadAsync();
            Assert(restored.Selection!.ActiveCharacter.Id == snapshot.Selection!.ActiveCharacter.Id &&
                restored.Selection.ActiveCatalogSet!.Id == CatalogSet.ClassicPhaseOne.Id, "Äldre schema1 behåller samma identitet och fas.");
        });

        await checks.RunAsync("Tvåhandsvapen/offhand och unika items kan inte dubbelutrustas; andra specs bevaras", () =>
        {
            var items = new[]
            {
                Item("staff", EquipmentSlot.MainHand, 10, WeaponKind.TwoHanded),
                Item("orb", EquipmentSlot.OffHand, 11, WeaponKind.OffHand),
                Item("ring1", EquipmentSlot.Finger1, 12, WeaponKind.None, true),
                Item("ring2", EquipmentSlot.Finger2, 12, WeaponKind.None, true)
            };
            var lists = CharacterDefinition.Specializations(CharacterClass.Priest).ToDictionary(spec => spec.Id, _ => (IReadOnlyList<Recommendation>)items);
            var equipment = lists.ToDictionary(pair => pair.Key, _ => new Dictionary<EquipmentSlot, string>());
            var progress = new CharacterLoadouts(CharacterClass.Priest, lists, [], equipment);
            progress.SetEquipped("holy", "orb", true);
            progress.SetEquipped("shadow", "orb", true);
            progress.SetEquipped("holy", "staff", true);
            Assert(!progress.IsEquipped("holy", "orb") && progress.IsOwned("holy", "orb") && progress.IsEquipped("shadow", "orb"), "Tvåhandsbyte tar bara bort vald specs offhand.");
            progress.SetEquipped("holy", "orb", true);
            Assert(!progress.IsEquipped("holy", "staff"), "Offhand ersätter tvåhandsvapen.");
            progress.SetEquipped("holy", "ring1", true);
            progress.SetEquipped("holy", "ring2", true);
            Assert(!progress.IsEquipped("holy", "ring1") && progress.IsEquipped("holy", "ring2"), "Unique flyttar slot.");
            equipment["holy"] = new() { [EquipmentSlot.MainHand] = "staff", [EquipmentSlot.OffHand] = "orb" };
            AssertRejects(() => new CharacterLoadouts(CharacterClass.Priest, lists, items.Select(CharacterLoadouts.ItemKey), equipment));
            equipment["holy"] = new() { [EquipmentSlot.Finger1] = "ring1", [EquipmentSlot.Finger2] = "ring2" };
            AssertRejects(() => new CharacterLoadouts(CharacterClass.Priest, lists, items.Select(CharacterLoadouts.ItemKey), equipment));
            return Task.CompletedTask;
        });

        await checks.RunAsync("Unique-equipped omfattar alla suffixvarianter; separat variantägande och andra specs bevaras", () =>
        {
            var healing = Item("unique-healing", EquipmentSlot.Finger1, 50, unique: true, suffix: "of Healing");
            var fire = Item("unique-fire", EquipmentSlot.Finger2, 50, unique: true, suffix: "of Fire Wrath");
            var items = new[] { healing, fire };
            var lists = CharacterDefinition.Specializations(CharacterClass.Priest).ToDictionary(spec => spec.Id, _ => (IReadOnlyList<Recommendation>)items);
            var equipment = lists.ToDictionary(pair => pair.Key, _ => new Dictionary<EquipmentSlot, string>());
            var progress = new CharacterLoadouts(CharacterClass.Priest, lists, [], equipment);
            progress.SetEquipped("holy", healing.Id, true);
            progress.SetEquipped("shadow", healing.Id, true);
            Assert(!progress.IsOwned("holy", fire.Id), "Ägande av ett suffix innebär inte ägande av ett annat.");
            progress.SetEquipped("holy", fire.Id, true);
            Assert(!progress.IsEquipped("holy", healing.Id) && progress.IsEquipped("holy", fire.Id),
                "Unique-regeln ska flytta utrustningen även mellan olika suffix av samma basitem.");
            Assert(progress.IsOwned("holy", healing.Id) && progress.IsOwned("holy", fire.Id) && progress.IsEquipped("shadow", healing.Id),
                "Båda varianternas ägande och andra specens utrustning ska bevaras.");
            var restored = new CharacterLoadouts(CharacterClass.Priest, lists, progress.OwnedItemKeys, progress.EquippedBySpec);
            Assert(restored.IsEquipped("holy", fire.Id) && restored.IsEquipped("shadow", healing.Id), "Giltiga separata specval återläses.");
            equipment["holy"] = new() { [EquipmentSlot.Finger1] = healing.Id, [EquipmentSlot.Finger2] = fire.Id };
            AssertRejects(() => new CharacterLoadouts(CharacterClass.Priest, lists, progress.OwnedItemKeys, equipment));
            return Task.CompletedTask;
        });

        await checks.RunAsync("Sparfel vid nivåbyte bevarar aktiv nivå och båda nivåernas tre listor", async () =>
        {
            const string name = "failed-level-selection";
            var path = checks.PathFor(name + "-workspace.json");
            var repository = new FailOnSaveRepository(new JsonWorkspaceRepository(path));
            var service = new CharacterTrackerService(new LevelFixtureCatalog(), repository, checks.Repository(name + "-old.json"));
            var character = await EquipBothLevelsAsync(service);
            var original = await File.ReadAllTextAsync(path);
            repository.Fail = true;
            await Throws<IOException>(() => service.SelectCatalogAsync(CatalogSet.ForeverLevelThirty.Id));
            Assert(await File.ReadAllTextAsync(path) == original, "Misslyckat nivåbyte får inte skriva om aktuell eller arkiverad utrustning.");
            var snapshot = await service.LoadAsync();
            Assert(snapshot.Selection!.ActiveCatalogSet!.Id == CatalogSet.ForeverLevelSixty.Id &&
                snapshot.Selection.ActiveSpecialization.Id == "shadow", "Misslyckat nivåbyte ska inte ändra aktivt val.");
            var restarted = Service(checks, name);
            await AssertBothLevelsAsync(restarted, character);
        });

        await checks.RunAsync("Sparfel vid borttaget ägande bevarar aktiva och arkiverade utrustningslistor", async () =>
        {
            const string name = "failed-archived-ownership";
            var path = checks.PathFor(name + "-workspace.json");
            var repository = new FailOnSaveRepository(new JsonWorkspaceRepository(path));
            var service = new CharacterTrackerService(new LevelFixtureCatalog(), repository, checks.Repository(name + "-old.json"));
            var character = await EquipBothLevelsAsync(service);
            var original = await File.ReadAllTextAsync(path);
            repository.Fail = true;
            await Throws<IOException>(() => service.SetOwnedAsync("l60-shadow-head", false));
            Assert(await File.ReadAllTextAsync(path) == original, "Misslyckat borttaget ägande får inte rensa någon sparad nivå eller spec.");
            var snapshot = await service.LoadAsync();
            Assert(Entry(snapshot, "l60-shadow-head").IsOwned && Entry(snapshot, "l60-shadow-head").IsEquipped,
                "Ägande och aktiv utrustning ska finnas kvar efter sparfelet.");
            var restarted = Service(checks, name);
            await AssertBothLevelsAsync(restarted, character);
        });
    }

    private static async Task<Guid> EquipBothLevelsAsync(CharacterTrackerService service)
    {
        var character = (await service.CreateAsync("TEST ONLY", GameVersion.Forever, CharacterClass.Priest)).Selection!.ActiveCharacter.Id;
        foreach (var set in new[] { CatalogSet.ForeverLevelThirty, CatalogSet.ForeverLevelSixty })
        {
            await service.SelectCatalogAsync(set.Id);
            var prefix = set.LevelCap == 30 ? "l30" : "l60";
            foreach (var spec in new[] { "holy", "discipline", "shadow" })
            {
                await service.SelectAsync(character, spec);
                await service.SetEquippedAsync($"{prefix}-{spec}-head", true);
                await service.SetEquippedAsync($"{prefix}-{spec}-chest", true);
            }
        }
        return character;
    }

    private static async Task AssertBothLevelsAsync(CharacterTrackerService service, Guid character)
    {
        Assert((await service.LoadAsync()).Selection!.ActiveCatalogSet!.Id == CatalogSet.ForeverLevelSixty.Id, "Omstart behåller den sparade aktiva nivån.");
        foreach (var set in new[] { CatalogSet.ForeverLevelThirty, CatalogSet.ForeverLevelSixty })
        {
            await service.SelectCatalogAsync(set.Id);
            var prefix = set.LevelCap == 30 ? "l30" : "l60";
            foreach (var spec in new[] { "holy", "discipline", "shadow" })
            {
                var snapshot = await service.SelectAsync(character, spec);
                Assert(Entry(snapshot, $"{prefix}-{spec}-head").IsOwned && Entry(snapshot, $"{prefix}-{spec}-head").IsEquipped &&
                    Entry(snapshot, $"{prefix}-{spec}-chest").IsOwned && Entry(snapshot, $"{prefix}-{spec}-chest").IsEquipped,
                    "Alla sex nivå/spec-kombinationer behåller ägande och utrustning.");
            }
        }
    }

    private sealed class FailOnSaveRepository(IWorkspaceRepository inner) : IWorkspaceRepository
    {
        public bool Fail { get; set; }
        public Task<WorkspaceState?> LoadAsync(CancellationToken cancellationToken = default) => inner.LoadAsync(cancellationToken);
        public Task SaveAsync(WorkspaceState state, CancellationToken cancellationToken = default) => Fail
            ? throw new IOException("Simulated write failure.") : inner.SaveAsync(state, cancellationToken);
    }

    private static void AssertRejects(Action action)
    {
        try { action(); } catch (ArgumentException) { return; }
        throw new InvalidOperationException("Ogiltig utrustningskombination accepterades.");
    }
    private static CharacterTrackerService Service(CheckRun checks, string name) => new(new LevelFixtureCatalog(),
        new JsonWorkspaceRepository(checks.PathFor(name + "-workspace.json")), checks.Repository(name + "-old.json"));
    private static Recommendation Item(string id, EquipmentSlot slot, int itemId, WeaponKind weapon = WeaponKind.None, bool unique = false, string? suffix = null) =>
        new(id, slot, "TEST ONLY", "Fictional", "", new ItemDetails(itemId, suffix, AcquisitionType.Dungeon, "", "https://example.com/item", "https://example.com/guide", weapon, unique));
    private sealed class LevelFixtureCatalog : ICharacterCatalog
    {
        public Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string spec, CancellationToken cancellationToken = default) =>
            LoadAsync(version, characterClass, spec, CatalogSet.Default(version).Id, cancellationToken);
        public IReadOnlyList<CatalogSet> CatalogSets(GameVersion version) => CatalogSet.Defaults(version);
        public Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string spec, string setId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var set = CatalogSets(version).Single(set => set.Id == setId);
            var prefix = set.LevelCap == 30 ? "l30" : "l60";
            var items = new[] { Item($"{prefix}-{spec}-head", EquipmentSlot.Head, 1), Item($"{prefix}-{spec}-chest", EquipmentSlot.Chest, set.LevelCap) };
            return Task.FromResult(new BisCatalog(new GameContext(CharacterDefinition.VersionName(version), spec, "TEST ONLY"), items, true, Set: set));
        }
    }
}
