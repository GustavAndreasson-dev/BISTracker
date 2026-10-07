using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Infrastructure;
using static BISTracker.Checks.Scenarios.CheckAssertions;

namespace BISTracker.Checks.Scenarios;
internal static class TrackingScenarios
{
    public static async Task RunAsync(CheckRun checks)
    {
        await checks.RunAsync("Utrustad innebär erhållen och avmarkering behåller ägande", async () =>
        {
            var service = checks.Service("equip.json");
            var equipped = Entry(await service.SetEquippedAsync("demo-head", true), "demo-head");
            Assert(equipped.IsOwned && equipped.IsEquipped, "Utrustad ska också bli erhållen.");
            var unequipped = Entry(await service.SetEquippedAsync("demo-head", false), "demo-head");
            Assert(unequipped.IsOwned && !unequipped.IsEquipped, "Avmarkera utrustad ska behålla erhållen.");
        });

        await checks.RunAsync("Avmarkera erhållen tar även bort utrustning", async () =>
        {
            var service = checks.Service("unown.json");
            await service.SetEquippedAsync("demo-neck", true);
            var entry = Entry(await service.SetOwnedAsync("demo-neck", false), "demo-neck");
            Assert(!entry.IsOwned && !entry.IsEquipped, "Avmarkerat ägande ska även avmarkera utrustning.");
        });

        await checks.RunAsync("Erhållen innebär inte automatiskt utrustad", async () =>
        {
            var entry = Entry(await checks.Service("owned.json").SetOwnedAsync("demo-chest", true), "demo-chest");
            Assert(entry.IsOwned && !entry.IsEquipped, "Erhållen ska kunna registreras separat.");
        });

        await checks.RunAsync("Byte inom samma slot behåller föregående item som erhållet", async () =>
        {
            var catalog = await new SampleBisCatalog().LoadAsync();
            var alternate = new Recommendation("test-head-alternative", EquipmentSlot.Head,
                "Exempel: alternativt huvud", "Fiktiv testdata", "Endast för kontroll av slotbyte.");
            var service = new TrackerService(new TestCatalog(catalog with { Items = catalog.Items.Append(alternate).ToArray() }),
                checks.Repository("swap.json"));
            await service.SetEquippedAsync("demo-head", true);
            await service.SetEquippedAsync("demo-neck", true);
            var snapshot = await service.SetEquippedAsync(alternate.Id, true);
            Assert(Entry(snapshot, "demo-head").IsOwned && !Entry(snapshot, "demo-head").IsEquipped,
                "Föregående huvud ska fortfarande vara erhållet, men inte utrustat.");
            Assert(Entry(snapshot, alternate.Id).IsOwned && Entry(snapshot, alternate.Id).IsEquipped,
                "Alternativet ska vara erhållet och utrustat.");
            Assert(Entry(snapshot, "demo-neck").IsEquipped, "Byte ska inte ändra andra platser.");
            Assert(snapshot.Entries.Count(entry => entry.Item.Slot == EquipmentSlot.Head && entry.IsEquipped) == 1,
                "Endast ett item per slot får vara utrustat.");
            snapshot = await service.SetEquippedAsync("demo-head", false);
            Assert(Entry(snapshot, alternate.Id).IsEquipped,
                "Avmarkering av ett annat item i samma slot får inte ta bort aktiv utrustning.");
            snapshot = await service.SetOwnedAsync("demo-head", false);
            Assert(Entry(snapshot, alternate.Id).IsEquipped,
                "Avmarkering av tidigare items ägande får inte ta bort aktiv utrustning.");
            var state = await checks.Repository("swap.json").LoadAsync();
            Assert(state.EquippedItems[EquipmentSlot.Head] == alternate.Id, "Slotbytet ska vara sparat.");
        });

        await checks.RunAsync("Okända ID:n avvisas utan att sparat tillstånd ändras", async () =>
        {
            var service = checks.Service("unknown.json");
            await service.SetOwnedAsync("demo-head", true);
            var original = await File.ReadAllTextAsync(checks.PathFor("unknown.json"));
            await Throws<ArgumentException>(() => service.SetOwnedAsync("unknown-item", true));
            await Throws<ArgumentException>(() => service.SetOwnedAsync("unknown-item", false));
            await Throws<ArgumentException>(() => service.SetEquippedAsync("unknown-item", true));
            await Throws<ArgumentException>(() => service.SetEquippedAsync("unknown-item", false));
            Assert(await File.ReadAllTextAsync(checks.PathFor("unknown.json")) == original, "Felaktiga mutationer får inte ändra filen.");
        });

        await checks.RunAsync("Skrivfel når användningsfallet och rapporteras inte som framgång", async () =>
        {
            var service = new TrackerService(new SampleBisCatalog(), new FailingRepository());
            await Throws<IOException>(() => service.SetOwnedAsync("demo-head", true));
            await Throws<IOException>(() => service.SetEquippedAsync("demo-head", true));
            var snapshot = await service.LoadAsync();
            Assert(snapshot.Entries.All(entry => !entry.IsOwned && !entry.IsEquipped),
                "Misslyckade mutationer får inte synas som sparade framsteg.");
        });
    }
}

sealed class TestCatalog(BisCatalog catalog) : IBisCatalog
{
    public Task<BisCatalog> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(catalog);
    }
}

sealed class FailingRepository : IProgressRepository
{
    public Task<ProgressState> LoadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new ProgressState([], []));

    public Task SaveAsync(ProgressState state, CancellationToken cancellationToken = default) =>
        throw new IOException("Simulerat skrivfel.");
}
