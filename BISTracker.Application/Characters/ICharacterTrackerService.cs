using BISTracker.Domain;

namespace BISTracker.Application;

public interface ICharacterTrackerService : ITrackerService
{
    Task<TrackerSnapshot> SelectAsync(Guid characterId, string specializationId, CancellationToken cancellationToken = default);
    Task<TrackerSnapshot> CreateAsync(string name, GameVersion version, CharacterClass characterClass, CancellationToken cancellationToken = default);
}
