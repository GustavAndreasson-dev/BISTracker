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
            Assert(catalog.Items.All(item => item.Name.StartsWith("Exempel:") && item.Source.Contains("Fiktiv")),
                "Namnen och källan ska uttryckligen vara exempeldata.");
            Assert(catalog.Context.Version.Contains("Vanilla") && catalog.Context.Specialization == "Holy Priest",
                "Fel spelkontext.");
        });
    }
}
