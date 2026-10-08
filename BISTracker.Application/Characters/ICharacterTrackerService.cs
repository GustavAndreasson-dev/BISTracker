using BISTracker.Domain;

namespace BISTracker.Application;

/// <summary>
/// Character use cases. Every operation returns a snapshot only after a successful save.
/// A snapshot whose <see cref="TrackerSnapshot.Selection"/> is null means the workspace has no characters;
/// its catalog is then <see cref="TrackerSnapshot.NoCharacterCatalog"/> with no entries.
/// </summary>
public interface ICharacterTrackerService : ITrackerService
{
    Task<TrackerSnapshot> SelectAsync(Guid characterId, string specializationId, CancellationToken cancellationToken = default);
    Task<TrackerSnapshot> CreateAsync(string name, GameVersion version, CharacterClass characterClass, CancellationToken cancellationToken = default);
    Task<TrackerSnapshot> SelectCatalogAsync(string catalogSetId, CancellationToken cancellationToken = default);
    /// <summary>Changes only the name; version, class, ownership and equipment are unchanged.</summary>
    Task<TrackerSnapshot> RenameAsync(Guid characterId, string name, CancellationToken cancellationToken = default);
    /// <summary>Permanently removes a character, including the last one. A deleted active character is replaced by the first remaining one.</summary>
    Task<TrackerSnapshot> DeleteAsync(Guid characterId, CancellationToken cancellationToken = default);
}
