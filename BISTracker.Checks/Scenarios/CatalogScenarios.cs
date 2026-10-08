using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Infrastructure;
using static BISTracker.Checks.Scenarios.CheckAssertions;

namespace BISTracker.Checks.Scenarios;
internal static class CatalogScenarios
{
    public static async Task RunAsync(CheckRun checks)
    {
        await checks.RunAsync("Exempelkatalog: 17 platser, stabila ID:n och fiktiv data", async () =>
        {
            var catalog = await new SampleBisCatalog().LoadAsync();
            Assert(catalog.IsSample, "Katalogen ska vara markerad som exempeldata.");
            Assert(catalog.Items.Count == 17 && catalog.Items.Select(item => item.Slot).Distinct().Count() == 17,
                "Exakt en rekommendation per slot krävs.");
            var ids = new[] { "head", "neck", "shoulder", "back", "chest", "wrist", "hands", "waist", "legs",
                "feet", "finger1", "finger2", "trinket1", "trinket2", "mainhand", "offhand", "ranged" };
            Assert(catalog.Items.Select(item => item.Id).SequenceEqual(ids.Select(id => $"demo-{id}")), "Fel exempel-ID:n.");
            Assert(catalog.Items.All(item => item.Name.StartsWith("Example:") && item.Source.Contains("Fictional")),
                "Namnen och källan ska uttryckligen vara exempeldata.");
            Assert(catalog.Context.Version.Contains("Vanilla") && catalog.Context.Specialization == "Holy Priest",
                "Fel spelkontext.");
        });

        await checks.RunAsync("Riktig fas 1-katalog: 1.0.0-målen täcker alla slots med källor, ikonlänkar och rätt anskaffning", async () =>
        {
            var catalog = await new ClassicPhaseOneBisCatalog().LoadAsync();
            Assert(!catalog.IsSample && catalog.Items.Count == 17, "Den riktiga katalogen ska vara aktiv med 17 mål.");
            Assert(catalog.Items.Select(item => item.Slot).Order().SequenceEqual(Enum.GetValues<EquipmentSlot>()),
                "Samtliga utrustningsplatser ska täckas exakt en gång.");
            Assert(catalog.Context.Phase == ReviewedCatalogReader.ClassicPhase && catalog.Set == CatalogSet.ClassicPhaseOne, "Fel fastillgänglighet.");
            Assert(catalog.Items.All(item => item.Details is { ClassicItemId: > 0 } details &&
                Enum.IsDefined(details.AcquisitionType) && details.IconUrl.StartsWith("https://") &&
                details.ItemUrl.Contains($"item={details.ClassicItemId}") && details.RecommendationUrl.StartsWith("https://") &&
                !string.IsNullOrWhiteSpace(item.Source)), "Itemfakta, anskaffning och källänkar måste bevaras.");
            Assert(!catalog.Items.Any(item => item.Details!.ClassicItemId is 13102 or 14154),
                "De 17 målen från 1.0.0 innehåller inte originalguidens world drop eller crafting.");
            Assert(catalog.Items.Single(item => item.Slot == EquipmentSlot.Waist).Note.Contains("10 players") &&
                catalog.Items.Single(item => item.Slot == EquipmentSlot.Ranged).Note.Contains("Raid quest"),
                "UBRS- och questvillkor får inte döljas.");
        });

        await checks.RunAsync("Of Healing är ett eget trackingmål; bas-ID och demo-ID avvisas", async () =>
        {
            var service = new TrackerService(new ClassicPhaseOneBisCatalog(), checks.Repository("real-variants.json"));
            var catalog = (await service.LoadAsync()).Catalog;
            var variants = catalog.Items.Where(item => item.Details!.RequiredSuffix is not null).ToArray();
            Assert(variants.Length == 3 && variants.All(item => item.Name.EndsWith(" of Healing") &&
                item.Id.EndsWith("-of-healing")), "Suffixet måste finnas i både namn och trackingidentitet.");
            var cape = variants.Single(item => item.Slot == EquipmentSlot.Back);
            await service.SetEquippedAsync(cape.Id, true);
            await Throws<ArgumentException>(() => service.SetOwnedAsync("classic-p1-item-13386", true));
            await Throws<ArgumentException>(() => service.SetOwnedAsync("demo-back", true));
            var restored = Entry(await service.LoadAsync(), cape.Id);
            Assert(restored.IsOwned && restored.IsEquipped, "Avvisade markeringar får inte ändra suffixmålet.");
        });

        await checks.RunAsync("Fas 1-framsteg återläses separat och gamla demomarkeringar bevaras", async () =>
        {
            var directory = checks.PathFor("profiles");
            var draftPath = ProgressFilePaths.Draft(directory);
            var phaseOnePath = ProgressFilePaths.ClassicPhaseOne(directory);
            var draftService = new TrackerService(new SampleBisCatalog(), new JsonProgressRepository(draftPath));
            await draftService.SetEquippedAsync("demo-head", true);
            var originalDraft = await File.ReadAllTextAsync(draftPath);
            var service = new TrackerService(new ClassicPhaseOneBisCatalog(), new JsonProgressRepository(phaseOnePath));
            var initial = await service.LoadAsync();
            Assert(initial.Entries.All(entry => !entry.IsOwned && !entry.IsEquipped),
                "Fiktiva items ska inte bli verkligt ägande.");
            var hat = initial.Catalog.Items.Single(item => item.Slot == EquipmentSlot.Head);
            await service.SetEquippedAsync(hat.Id, true);
            var restarted = new TrackerService(new ClassicPhaseOneBisCatalog(), new JsonProgressRepository(phaseOnePath));
            var restored = Entry(await restarted.LoadAsync(), hat.Id);
            Assert(restored.IsOwned && restored.IsEquipped, "Riktiga item-ID:n ska återläsas efter omstart.");
            Assert(await File.ReadAllTextAsync(draftPath) == originalDraft, "Demofilen får inte ändras.");
        });
    }
}
