using BISTracker.Domain;

namespace BISTracker.Application;

public sealed class CharacterTrackerService : ICharacterTrackerService
{
    private readonly ICharacterCatalog _catalogs;
    private readonly IWorkspaceRepository _repository;
    private readonly IProgressRepository _legacyProgress;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public CharacterTrackerService(ICharacterCatalog catalogs, IWorkspaceRepository repository, IProgressRepository legacyProgress)
    {
        ArgumentNullException.ThrowIfNull(catalogs);
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(legacyProgress);
        _catalogs = catalogs;
        _repository = repository;
        _legacyProgress = legacyProgress;
    }

    public Task<TrackerSnapshot> LoadAsync(CancellationToken cancellationToken = default) => ExecuteAsync(null, cancellationToken);
    public Task<TrackerSnapshot> SelectAsync(Guid characterId, string specializationId, CancellationToken cancellationToken = default) =>
        ExecuteAsync((state, validated) =>
        {
            var character = state.Characters.SingleOrDefault(item => item.Id == characterId)
                ?? throw new ArgumentException("Unknown character.");
            _ = CharacterDefinition.Specialization(character.Class, specializationId);
            return state with { ActiveCharacterId = characterId, Characters = state.Characters.Select(item => item.Id == characterId
                ? item with { SelectedSpecialization = specializationId } : item).ToArray() };
        }, cancellationToken);

    public Task<TrackerSnapshot> CreateAsync(string name, GameVersion version, CharacterClass characterClass, CancellationToken cancellationToken = default) =>
        ExecuteAsync((state, validated) =>
        {
            ValidateName(name);
            _ = CharacterDefinition.VersionName(version);
            var spec = CharacterDefinition.Specializations(characterClass)[0].Id;
            var character = NewCharacter(name.Trim(), version, characterClass, spec);
            return state with { ActiveCharacterId = character.Id, Characters = [.. state.Characters, character] };
        }, cancellationToken);

    public Task<TrackerSnapshot> SetOwnedAsync(string itemId, bool owned, CancellationToken cancellationToken = default) =>
        ChangeProgressAsync((progress, spec) => progress.SetOwned(spec, itemId, owned), cancellationToken);
    public Task<TrackerSnapshot> SetEquippedAsync(string itemId, bool equipped, CancellationToken cancellationToken = default) =>
        ChangeProgressAsync((progress, spec) => progress.SetEquipped(spec, itemId, equipped), cancellationToken);

    private Task<TrackerSnapshot> ChangeProgressAsync(Action<CharacterLoadouts, string> change, CancellationToken cancellationToken) =>
        ExecuteAsync((state, validated) =>
        {
            var character = state.Characters.Single(item => item.Id == state.ActiveCharacterId);
            var progress = validated[character.Id].Progress;
            change(progress, character.SelectedSpecialization);
            return state with { Characters = state.Characters.Select(item => item.Id == character.Id
                ? item with { OwnedItemKeys = progress.OwnedItemKeys, EquippedBySpec = progress.EquippedBySpec } : item).ToArray() };
        }, cancellationToken);

    private async Task<TrackerSnapshot> ExecuteAsync(
        Func<WorkspaceState, Dictionary<Guid, ValidatedCharacter>, WorkspaceState>? change, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var state = await _repository.LoadAsync(cancellationToken).ConfigureAwait(false);
            var isNew = state is null;
            state ??= await ImportLegacyAsync(cancellationToken).ConfigureAwait(false);
            var validated = await ValidateAsync(state, cancellationToken).ConfigureAwait(false);
            if (change is not null)
            {
                state = change(state, validated);
                validated = await ValidateAsync(state, cancellationToken).ConfigureAwait(false);
            }
            if (isNew || change is not null) await _repository.SaveAsync(state, cancellationToken).ConfigureAwait(false);
            var active = state.Characters.Single(item => item.Id == state.ActiveCharacterId);
            var catalog = validated[active.Id].Catalogs[active.SelectedSpecialization];
            var progress = validated[active.Id].Progress;
            var entries = catalog.Items.Select(item => new TrackerEntry(item,
                progress.IsOwned(active.SelectedSpecialization, item.Id), progress.IsEquipped(active.SelectedSpecialization, item.Id))).ToArray();
            var options = state.Characters.Select(item => new CharacterOption(item.Id, item.Name, item.Version, item.Class, item.SelectedSpecialization)).ToArray();
            return new TrackerSnapshot(catalog, Array.AsReadOnly(entries), new CharacterSelection(Array.AsReadOnly(options),
                options.Single(item => item.Id == active.Id), CharacterDefinition.Specialization(active.Class, active.SelectedSpecialization)));
        }
        finally { _gate.Release(); }
    }

    private async Task<WorkspaceState> ImportLegacyAsync(CancellationToken cancellationToken)
    {
        var catalog = await _catalogs.LoadAsync(GameVersion.Classic, CharacterClass.Priest, "holy", cancellationToken).ConfigureAwait(false);
        var legacy = await _legacyProgress.LoadAsync(cancellationToken).ConfigureAwait(false);
        var oldProgress = new CharacterProgress(catalog.Items, legacy.OwnedItemIds, legacy.EquippedItems);
        var character = NewCharacter("My Priest", GameVersion.Classic, CharacterClass.Priest, "holy");
        character.EquippedBySpec["holy"] = new(oldProgress.EquippedItems);
        character = character with { OwnedItemKeys = oldProgress.OwnedItemIds.Select(id => CharacterLoadouts.ItemKey(catalog.Items.Single(item => item.Id == id))).Distinct().ToArray() };
        return new WorkspaceState(1, character.Id, [character]);
    }

    private async Task<Dictionary<Guid, ValidatedCharacter>> ValidateAsync(WorkspaceState state, CancellationToken cancellationToken)
    {
        if (state.SchemaVersion != 1 || state.Characters is null || state.Characters.Length == 0 ||
            state.Characters.Any(item => item is null || item.Id == Guid.Empty) ||
            state.Characters.Select(item => item.Id).Distinct().Count() != state.Characters.Length ||
            !state.Characters.Any(item => item.Id == state.ActiveCharacterId))
            throw new InvalidDataException("Invalid workspace or unsupported schema. Existing progress has been preserved.");
        var result = new Dictionary<Guid, ValidatedCharacter>();
        foreach (var character in state.Characters)
        {
            ValidateName(character.Name);
            _ = CharacterDefinition.VersionName(character.Version);
            _ = CharacterDefinition.Specialization(character.Class, character.SelectedSpecialization);
            var catalogs = new Dictionary<string, BisCatalog>();
            foreach (var spec in CharacterDefinition.Specializations(character.Class))
            {
                var catalog = await _catalogs.LoadAsync(character.Version, character.Class, spec.Id, cancellationToken).ConfigureAwait(false);
                catalogs.Add(spec.Id, catalog with { Items = Array.AsReadOnly(catalog.Items.ToArray()) });
            }
            var progress = new CharacterLoadouts(character.Class, catalogs.ToDictionary(pair => pair.Key, pair => pair.Value.Items),
                character.OwnedItemKeys, character.EquippedBySpec);
            result.Add(character.Id, new ValidatedCharacter(catalogs, progress));
        }
        return result;
    }

    private static CharacterState NewCharacter(string name, GameVersion version, CharacterClass characterClass, string spec) =>
        new(Guid.NewGuid(), name, version, characterClass, spec, [], CharacterDefinition.Specializations(characterClass)
            .ToDictionary(item => item.Id, _ => new Dictionary<EquipmentSlot, string>()));

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 40 || name.Any(char.IsControl))
            throw new ArgumentException("Enter a character name with 1–40 characters.", nameof(name));
    }

    private sealed record ValidatedCharacter(Dictionary<string, BisCatalog> Catalogs, CharacterLoadouts Progress);
}
