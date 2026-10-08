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
    }

    private static void AssertRejects(Action action)
    {
        try { action(); } catch (ArgumentException) { return; }
        throw new InvalidOperationException("Ogiltig utrustningskombination accepterades.");
    }
    private static CharacterTrackerService Service(CheckRun checks, string name) => new(new LevelFixtureCatalog(),
        new JsonWorkspaceRepository(checks.PathFor(name + "-workspace.json")), checks.Repository(name + "-old.json"));
    private static Recommendation Item(string id, EquipmentSlot slot, int itemId, WeaponKind weapon = WeaponKind.None, bool unique = false) =>
        new(id, slot, "TEST ONLY", "Fictional", "", new ItemDetails(itemId, null, AcquisitionType.Dungeon, "", "https://example.com/item", "https://example.com/guide", weapon, unique));
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
