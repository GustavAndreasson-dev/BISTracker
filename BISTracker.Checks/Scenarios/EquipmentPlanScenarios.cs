using BISTracker.Domain;
using BISTracker.Infrastructure;
using static BISTracker.Checks.Scenarios.CheckAssertions;

namespace BISTracker.Checks.Scenarios;

internal static class EquipmentPlanScenarios
{
    public static async Task RunAsync(CheckRun checks)
    {
        await checks.RunAsync("Booleskt variantägande kan inte utrusta två okända exemplar i olika slots", async () =>
        {
            var first = Item("one-ring-first", EquipmentSlot.Finger1, 100005);
            var second = Item("one-ring-second", EquipmentSlot.Finger2, 100005);
            var specs = CharacterDefinition.Specializations(CharacterClass.Mage);
            var lists = specs.ToDictionary(spec => spec.Id, _ => (IReadOnlyList<Recommendation>)new[] { first, second });
            var equipped = specs.ToDictionary(spec => spec.Id, _ => new Dictionary<EquipmentSlot, string>());
            var loadouts = new CharacterLoadouts(CharacterClass.Mage, lists, [], equipped);
            loadouts.SetEquipped("fire", first.Id, true);
            loadouts.SetEquipped("fire", second.Id, true);
            Assert(!loadouts.IsEquipped("fire", first.Id) && loadouts.IsEquipped("fire", second.Id) && loadouts.OwnedItemKeys.Length == 1,
                "Samma registrerade exemplar flyttas mellan platserna, utan att hitta på kvantitet.");
            equipped["fire"].Add(EquipmentSlot.Finger1, first.Id);
            equipped["fire"].Add(EquipmentSlot.Finger2, second.Id);
            await Throws<ArgumentException>(() => { _ = new CharacterLoadouts(CharacterClass.Mage, lists, [CharacterLoadouts.ItemKey(first)], equipped); return Task.CompletedTask; });
        });
        await checks.RunAsync("Alternativ grupperas till ett mål per slot; Rogue 39 rader blir aldrig 39 slots", async () =>
        {
            var provider = new CharacterCatalog();
            foreach (var characterClass in Enum.GetValues<CharacterClass>())
                foreach (var spec in CharacterDefinition.Specializations(characterClass))
                {
                    var catalog = await provider.LoadAsync(GameVersion.Forever, characterClass, spec.Id);
                    var plan = catalog.EquipmentPlan;
                    Assert(plan.Goals.Count <= 17 && plan.Goals.Select(goal => goal.Slot).Distinct().Count() == plan.Goals.Count, "Slotmål är unika.");
                    Assert(plan.Goals.Sum(goal => goal.Alternatives.Count) == catalog.Items.Count, "Alla sourcealternativ bevaras inom rätt slot.");
                    Assert(plan.Goals.All(goal => goal.Alternatives.All(item => item.Slot == goal.Slot)), "En annan slots item får inte fylla ett hål.");
                }
            var rogue = await provider.LoadAsync(GameVersion.Forever, CharacterClass.Rogue, "combat");
            Assert(rogue.EquipmentPlan.Goals.Count == 17, "Combat har 17 slotmål även om data saknas för några.");
        });
        await checks.RunAsync("Synliga slotrader bevisar inte ett komplett set när samma enda trinket förekommer två gånger", () =>
        {
            var items = FullSet();
            items[(int)EquipmentSlot.Trinket2] = Item("trinket-second-placement", EquipmentSlot.Trinket2, items[(int)EquipmentSlot.Trinket1].Details!.ClassicItemId);
            var plan = new EquipmentPlan(items);
            Assert(plan.Goals.All(goal => goal.Alternatives.Count > 0), "Alla 17 positioner har en rad.");
            Assert(!plan.CatalogCoverage.IsComplete && plan.CatalogCoverage.MissingSlots.SequenceEqual([EquipmentSlot.Trinket2]), "Andra trinketen saknar en separat fysisk identitet.");
            items.Add(Item("second-trinket", EquipmentSlot.Trinket2, 100000));
            Assert(new EquipmentPlan(items).CatalogCoverage.IsComplete, "Ett distinkt verifierat alternativ löser konflikten.");
            return Task.CompletedTask;
        });
        await checks.RunAsync("Flera ägda alternativ fyller en slot; en ring fyller inte två, och matching väljer en laglig kombination", () =>
        {
            var items = FullSet();
            var firstRing = items[(int)EquipmentSlot.Finger1];
            items.Add(Item("same-ring-second-slot", EquipmentSlot.Finger2, firstRing.Details!.ClassicItemId));
            var extraHead = Item("extra-head", EquipmentSlot.Head, 100001);
            items.Add(extraHead);
            var plan = new EquipmentPlan(items);
            var keys = new[] { CharacterLoadouts.ItemKey(items[0]), CharacterLoadouts.ItemKey(extraHead), CharacterLoadouts.ItemKey(firstRing) };
            Assert(plan.OwnedCoverage(keys).CoveredSlots.Count == 2, "Två head-alternativ och en ring ger två fyllda slots.");
            Assert(!plan.OwnedCoverage(keys).CoveredSlots.Contains(EquipmentSlot.Finger2), "Samma ring räknas inte igen.");
            Assert(plan.OwnedCoverage(keys, preferredSlots: [EquipmentSlot.Finger2]).CoveredSlots.Contains(EquipmentSlot.Finger2) &&
                !plan.OwnedCoverage(keys, preferredSlots: [EquipmentSlot.Finger2]).CoveredSlots.Contains(EquipmentSlot.Finger1),
                "Den utrustade ringplatsen ska visas som täckt; den andra förblir saknad.");
            var secondRing = items[(int)EquipmentSlot.Finger2];
            Assert(plan.OwnedCoverage(keys.Append(CharacterLoadouts.ItemKey(secondRing))).CoveredSlots.Count == 3, "Ett separat andra ringitem fyller nästa plats.");
            return Task.CompletedTask;
        });
        await checks.RunAsync("Unique över suffix och inkompatibla händer kan inte ge ett falskt fullständigt set", () =>
        {
            var items = FullSet();
            items[(int)EquipmentSlot.Finger1] = Item("unique-one", EquipmentSlot.Finger1, 100002, unique: true, suffix: "of Healing");
            items[(int)EquipmentSlot.Finger2] = Item("unique-two", EquipmentSlot.Finger2, 100002, unique: true, suffix: "of Intellect");
            Assert(!new EquipmentPlan(items).CatalogCoverage.IsComplete, "Unique-basitem får inte räknas två gånger med olika suffix.");
            items = FullSet();
            items[(int)EquipmentSlot.MainHand] = Item("two-hand", EquipmentSlot.MainHand, 100003, WeaponKind.TwoHanded);
            var flexible = new EquipmentPlan(items).CatalogCoverage;
            Assert(flexible.IsComplete && flexible.RequiredSlots.Count == 16 && !flexible.RequiredSlots.Contains(EquipmentSlot.OffHand), "Tvåhandsval behöver inte offhand.");
            var keys = items.Select(CharacterLoadouts.ItemKey).ToArray();
            Assert(new EquipmentPlan(items).OwnedCoverage(keys).IsComplete &&
                new EquipmentPlan(items).OwnedCoverage(keys).RequiredSlots.Count == 16,
                "Ägt tvåhandsval ger samma handplan före och efter utrustning.");
            Assert(new EquipmentPlan(items).OwnedCoverage(keys, useTwoHandedWeapon: false).RequiredSlots.Count == 17 &&
                !new EquipmentPlan(items).OwnedCoverage(keys, useTwoHandedWeapon: false).IsComplete,
                "En vald enhandsuppsättning får inte fyllas av ett ägt tvåhandsvapen.");
            Assert(!new EquipmentPlan(items, weaponSetup: WeaponSetup.OneHandAndOffHand).CatalogCoverage.IsComplete,
                "Ett tvåhandsalternativ ersätter inte guidens uttryckliga dual-wield-mål.");
            var dualWieldOwned = new EquipmentPlan(items, weaponSetup: WeaponSetup.OneHandAndOffHand)
                .OwnedCoverage(items.Select(CharacterLoadouts.ItemKey), useTwoHandedWeapon: true);
            Assert(dualWieldOwned.RequiredSlots.Count == 17 && dualWieldOwned.MissingSlots.Contains(EquipmentSlot.MainHand),
                "Ett utrustat tvåhandsalternativ får inte ta bort offhand från ett uttryckligt dual-wield-mål.");
            return Task.CompletedTask;
        });
        await checks.RunAsync("Saknade slots förblir synliga mål; källbelagda undantag får inte motsäga katalogitems", () =>
        {
            var items = FullSet();
            items.RemoveAll(item => item.Slot == EquipmentSlot.Ranged);
            Assert(new EquipmentPlan(items).Goals.Single(goal => goal.Slot == EquipmentSlot.Ranged).Alternatives.Count == 0 &&
                !new EquipmentPlan(items).CatalogCoverage.IsComplete, "Saknad ranged får inte tyst försvinna.");
            var exemption = new SlotExemption(EquipmentSlot.Ranged, "TEST ONLY: slot unavailable at this level", "https://example.com/test-only-source");
            Assert(new EquipmentPlan(items, [exemption]).CatalogCoverage.IsComplete, "Ett uttryckligt granskningstillstånd skiljer en otillgänglig slot från saknad data.");
            return Throws<ArgumentException>(() => { _ = new EquipmentPlan(FullSet(), [exemption]); return Task.CompletedTask; });
        });
        await checks.RunAsync("Alliance- och Horde-belöningar får inte kombineras till ett falskt komplett fraktionsset", () =>
        {
            var items = FullSet();
            var head = items[0];
            items[0] = head with { Details = head.Details! with { AvailableFactions = [CharacterFaction.Alliance] } };
            var hands = items[(int)EquipmentSlot.Hands];
            items[(int)EquipmentSlot.Hands] = hands with { Details = hands.Details! with { AvailableFactions = [CharacterFaction.Horde] } };
            var plan = new EquipmentPlan(items);
            Assert(plan.CatalogCoverage.IsComplete, "Unionen kan ha alla slots.");
            Assert(plan.CatalogCoverageFor(CharacterFaction.Alliance).MissingSlots.Contains(EquipmentSlot.Hands) &&
                plan.CatalogCoverageFor(CharacterFaction.Horde).MissingSlots.Contains(EquipmentSlot.Head), "Varje fraktion måste ha en egen laglig kombination.");
            Assert(!plan.OwnedCoverage(items.Select(CharacterLoadouts.ItemKey)).IsComplete,
                "Gemensamt ägande får inte kombinera båda fraktionerna till falskt fullständigt framsteg.");
            return Task.CompletedTask;
        });
    }
    private static List<Recommendation> FullSet() => Enum.GetValues<EquipmentSlot>().Select(slot =>
        Item("test-" + slot, slot, 99000 + (int)slot, slot == EquipmentSlot.MainHand ? WeaponKind.OneHanded : slot == EquipmentSlot.OffHand ? WeaponKind.OffHand : WeaponKind.None)).ToList();
    private static Recommendation Item(string id, EquipmentSlot slot, int itemId, WeaponKind weapon = WeaponKind.None, bool unique = false, string? suffix = null) =>
        new(id, slot, "TEST ONLY " + id, "TEST ONLY source", "TEST ONLY", new ItemDetails(itemId, suffix, AcquisitionType.Dungeon,
            "", "https://example.com/test-only-item", "https://example.com/test-only-guide", weapon, unique));
}
