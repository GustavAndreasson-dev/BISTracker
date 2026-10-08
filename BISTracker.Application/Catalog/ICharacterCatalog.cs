using BISTracker.Domain;

namespace BISTracker.Application;

public interface ICharacterCatalog
{
    Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string specializationId, CancellationToken cancellationToken = default);
    IReadOnlyList<CatalogSet> CatalogSets(GameVersion version) => CatalogSet.Defaults(version);
    Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string specializationId, string catalogSetId, CancellationToken cancellationToken = default)
    {
        var set = CatalogSets(version).SingleOrDefault(item => item.Id == catalogSetId)
            ?? throw new ArgumentException("Unknown catalog set.", nameof(catalogSetId));
        if (set.Id == CatalogSet.Default(version).Id) return LoadAsync(version, characterClass, specializationId, cancellationToken);
        var spec = CharacterDefinition.Specialization(characterClass, specializationId);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new BisCatalog(new GameContext(CharacterDefinition.VersionName(version), $"{spec.Name} {characterClass}", set.Name),
            Array.Empty<Recommendation>(), false, "No reviewed BiS list available yet.", set));
    }
}
