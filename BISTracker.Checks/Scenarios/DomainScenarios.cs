using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Infrastructure;
using static BISTracker.Checks.Scenarios.CheckAssertions;

namespace BISTracker.Checks.Scenarios;
internal static class DomainScenarios
{
    public static async Task RunAsync(CheckRun checks)
    {
        await checks.RunAsync("Återställt framsteg avvisar ogiltiga domänrelationer", async () =>
        {
            var catalog = await new SampleBisCatalog().LoadAsync();
            await Throws<ArgumentException>(() => Task.FromResult(new CharacterProgress(catalog.Items,
                [], new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Head] = "demo-head" })));
            await Throws<ArgumentException>(() => Task.FromResult(new CharacterProgress(catalog.Items,
                ["demo-head"], new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Neck] = "demo-head" })));
            await Throws<ArgumentException>(() => Task.FromResult(new CharacterProgress(catalog.Items, ["unknown-item"])));
        });
    }
}
