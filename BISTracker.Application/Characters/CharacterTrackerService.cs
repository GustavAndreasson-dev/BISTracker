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

    public Task<TrackerSnapshot> SelectCatalogAsync(string catalogSetId, CancellationToken cancellationToken = default) =>
        ExecuteAsync((state, validated) =>
        {
            var active = state.Characters.Single(item => item.Id == state.ActiveCharacterId);
            if (!_catalogs.CatalogSets(active.Version).Any(set => set.Id == catalogSetId)) throw new ArgumentException("Unknown catalog set.");
            var previousId = SetId(active);
            if (previousId == catalogSetId) return state;
            var archived = active.ArchivedLoadouts is null ? new Dictionary<string, Dictionary<string, Dictionary<EquipmentSlot, string>>>() : new(active.ArchivedLoadouts);
            archived[previousId] = active.EquippedBySpec;
            var restored = archived.Remove(catalogSetId, out var equipment) ? equipment : EmptyEquipment(active.Class);
            var changed = active with { CatalogSetId = catalogSetId, EquippedBySpec = restored, ArchivedLoadouts = archived };
            return state with { Characters = state.Characters.Select(item => item.Id == active.Id ? changed : item).ToArray() };
        }, cancellationToken);

    private Task<TrackerSnapshot> ChangeProgressAsync(Action<CharacterLoadouts, string> change, CancellationToken cancellationToken) =>
        ExecuteAsync((state, validated) =>
        {
            var character = state.Characters.Single(item => item.Id == state.ActiveCharacterId);
            var validation = validated[character.Id];
            var progress = validation.ProgressBySet[SetId(character)];
            var previouslyOwned = progress.OwnedItemKeys.ToHashSet(StringComparer.Ordinal);
            change(progress, character.SelectedSpecialization);
            foreach (var removed in previouslyOwned.Except(progress.OwnedItemKeys))
                foreach (var other in validation.ProgressBySet.Values) other.SetItemOwnership(removed, false);
            var archives = validation.ProgressBySet.Where(pair => pair.Key != SetId(character)).ToDictionary(pair => pair.Key, pair => pair.Value.EquippedBySpec);
            return state with { Characters = state.Characters.Select(item => item.Id == character.Id
                ? item with { OwnedItemKeys = progress.OwnedItemKeys, EquippedBySpec = progress.EquippedBySpec, ArchivedLoadouts = archives } : item).ToArray() };
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
            var setId = SetId(active);
            var catalog = validated[active.Id].CatalogsBySet[setId][active.SelectedSpecialization];
            var progress = validated[active.Id].ProgressBySet[setId];
            var entries = catalog.Items.Select(item => new TrackerEntry(item,
                progress.IsOwned(active.SelectedSpecialization, item.Id), progress.IsEquipped(active.SelectedSpecialization, item.Id))).ToArray();
            var options = state.Characters.Select(item => new CharacterOption(item.Id, item.Name, item.Version, item.Class, item.SelectedSpecialization)).ToArray();
            return new TrackerSnapshot(catalog, Array.AsReadOnly(entries), new CharacterSelection(Array.AsReadOnly(options),
                options.Single(item => item.Id == active.Id), CharacterDefinition.Specialization(active.Class, active.SelectedSpecialization),
                _catalogs.CatalogSets(active.Version), _catalogs.CatalogSets(active.Version).Single(set => set.Id == setId)));
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
            var sets = _catalogs.CatalogSets(character.Version);
            if (!sets.Any(set => set.Id == SetId(character)) || character.ArchivedLoadouts?.Any(pair => pair.Key == SetId(character) || !sets.Any(set => set.Id == pair.Key)) == true)
                throw new InvalidDataException("Saved catalog context is unavailable. The existing progress has been preserved.");
            var catalogsBySet = new Dictionary<string, Dictionary<string, BisCatalog>>();
            foreach (var set in sets)
            {
                var catalogs = new Dictionary<string, BisCatalog>();
                foreach (var spec in CharacterDefinition.Specializations(character.Class))
                {
                    var catalog = await _catalogs.LoadAsync(character.Version, character.Class, spec.Id, set.Id, cancellationToken).ConfigureAwait(false);
                    catalogs.Add(spec.Id, catalog with { Items = Array.AsReadOnly(catalog.Items.ToArray()) });
                }
                catalogsBySet.Add(set.Id, catalogs);
            }
            var inventory = catalogsBySet.Values.SelectMany(catalogs => catalogs.Values).SelectMany(catalog => catalog.Items).ToArray();
            var equipmentBySet = character.ArchivedLoadouts is null ? new Dictionary<string, Dictionary<string, Dictionary<EquipmentSlot, string>>>() : new(character.ArchivedLoadouts);
            equipmentBySet.Add(SetId(character), character.EquippedBySpec);
            var progressBySet = equipmentBySet.ToDictionary(pair => pair.Key, pair => new CharacterLoadouts(character.Class,
                catalogsBySet[pair.Key].ToDictionary(item => item.Key, item => item.Value.Items), character.OwnedItemKeys, pair.Value, inventory));
            result.Add(character.Id, new ValidatedCharacter(catalogsBySet, progressBySet));
        }
        return result;
    }

    private static CharacterState NewCharacter(string name, GameVersion version, CharacterClass characterClass, string spec) =>
        new(Guid.NewGuid(), name, version, characterClass, spec, [], EmptyEquipment(characterClass), CatalogSet.Default(version).Id, new());

    private static Dictionary<string, Dictionary<EquipmentSlot, string>> EmptyEquipment(CharacterClass characterClass) =>
        CharacterDefinition.Specializations(characterClass).ToDictionary(item => item.Id, _ => new Dictionary<EquipmentSlot, string>());
    private static string SetId(CharacterState character) => character.CatalogSetId ?? CatalogSet.Default(character.Version).Id;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 40 || name.Any(char.IsControl))
            throw new ArgumentException("Enter a character name with 1–40 characters.", nameof(name));
    }

    private sealed record ValidatedCharacter(Dictionary<string, Dictionary<string, BisCatalog>> CatalogsBySet, Dictionary<string, CharacterLoadouts> ProgressBySet);
}
