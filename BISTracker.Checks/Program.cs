using BISTracker.Checks.Scenarios;

try
{
    if (args.Length == 2 && args[0] == "--catalog-release-audit") return await CatalogReleaseAudit.RunAsync(args[1]);
    using var checks = new CheckRun();
    await CatalogScenarios.RunAsync(checks);
    await DomainScenarios.RunAsync(checks);
    await TrackingScenarios.RunAsync(checks);
    await PersistenceScenarios.RunAsync(checks);
    await CharacterScenarios.RunAsync(checks);
    await CatalogContextScenarios.RunAsync(checks);
    await ForeverCatalogScenarios.RunAsync(checks);
    await EquipmentPlanScenarios.RunAsync(checks);
    Console.WriteLine(checks.Failures == 0
        ? "Alla kontroller godkända."
        : $"{checks.Failures} kontroll(er) misslyckades.");
    return checks.Failures == 0 ? 0 : 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Kontrollerna kunde inte slutföras: {exception}");
    return 1;
}
