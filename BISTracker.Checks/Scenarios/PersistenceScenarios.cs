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
    }
}
