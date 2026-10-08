using BISTracker.Application;
using BISTracker.Domain;

namespace BISTracker.Infrastructure;

public sealed class CharacterCatalog : ICharacterCatalog
{
    private readonly ClassicPhaseOneBisCatalog _classicHoly = new();

    public Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string specializationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var versionName = CharacterDefinition.VersionName(version);
        var spec = CharacterDefinition.Specialization(characterClass, specializationId);
        if (version == GameVersion.Classic && characterClass == CharacterClass.Priest && spec.Id == "holy")
            return _classicHoly.LoadAsync(cancellationToken);

        // An unavailable catalog is explicit: Classic recommendations are never presented as Forever data.
        return Task.FromResult(new BisCatalog(new GameContext(versionName, $"{spec.Name} {characterClass}", "Pre-raid"),
            Array.Empty<Recommendation>(), false, "No reviewed BiS list available yet."));
    }
}
