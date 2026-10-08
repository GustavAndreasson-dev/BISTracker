using BISTracker.Checks.Scenarios;

try
{
    if (args is ["--catalog-release-audit", var auditPath]) return await CatalogReleaseAudit.RunAsync(auditPath);
    if (args is ["--catalog-release-audit", var versionAuditPath, var version] && Enum.TryParse<BISTracker.Domain.GameVersion>(version, false, out var gameVersion) &&
        Enum.IsDefined(gameVersion) && !int.TryParse(version, out _))
        return await CatalogReleaseAudit.RunAsync(versionAuditPath, gameVersion);
    using var checks = new CheckRun();
    await CatalogScenarios.RunAsync(checks);
    await DomainScenarios.RunAsync(checks);
    await TrackingScenarios.RunAsync(checks);
    await PersistenceScenarios.RunAsync(checks);
    await CharacterScenarios.RunAsync(checks);
    await CatalogContextScenarios.RunAsync(checks);
    await ForeverCatalogScenarios.RunAsync(checks);
    await ClassicCatalogScenarios.RunAsync(checks);
    await EquipmentPlanScenarios.RunAsync(checks);
    await DistributionScenarios.RunAsync(checks);
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
