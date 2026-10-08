using BISTracker.Domain;

namespace BISTracker.Application;

public interface ICharacterCatalog
{
    Task<BisCatalog> LoadAsync(GameVersion version, CharacterClass characterClass, string specializationId, CancellationToken cancellationToken = default);
}
