using System.Text.Json;
using System.Text.Json.Serialization;
using BISTracker.Application;
using BISTracker.Domain;

namespace BISTracker.Infrastructure;

public sealed class JsonWorkspaceRepository : IWorkspaceRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };
    private readonly string _path;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonWorkspaceRepository(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
    }

    public async Task<WorkspaceState?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try { return await ReadAsync(cancellationToken).ConfigureAwait(false); }
        finally { _gate.Release(); }
    }

    public async Task SaveAsync(WorkspaceState state, CancellationToken cancellationToken = default)
    {
        if (!HasValidShape(state)) throw new ArgumentException("Invalid character workspace.", nameof(state));
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? temporary = null;
        try
        {
            // A direct save must also preserve unreadable files and unsupported future schemas.
            await ReadAsync(cancellationToken).ConfigureAwait(false);
            var directory = Path.GetDirectoryName(_path)!;
            Directory.CreateDirectory(directory);
            temporary = Path.Combine(directory, $".{Path.GetFileName(_path)}.{Guid.NewGuid():N}.tmp");
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, state, Options, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                stream.Flush(flushToDisk: true);
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(_path)) File.Replace(temporary, _path, destinationBackupFileName: null);
            else File.Move(temporary, _path);
            temporary = null;
        }
        finally
        {
            if (temporary is not null)
            {
                try { File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            _gate.Release();
        }
    }

    private async Task<WorkspaceState?> ReadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FileStream stream;
        try { stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous); }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
        await using (stream)
        {
            try
            {
                var state = await JsonSerializer.DeserializeAsync<WorkspaceState>(stream, Options, cancellationToken).ConfigureAwait(false);
                if (!HasValidShape(state)) throw new JsonException("Unsupported schema or invalid character workspace.");
                return state;
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("Invalid character workspace. The existing file has been preserved.", exception);
            }
        }
    }

    // Catalog-dependent relationships are validated by the application and domain before saving.
    private static bool HasValidShape(WorkspaceState? state) =>
        state is { SchemaVersion: 1, Characters.Length: > 0 } &&
        state.Characters.All(character => character is not null && character.Id != Guid.Empty &&
            !string.IsNullOrWhiteSpace(character.Name) && character.Name.Trim().Length <= 40 && !character.Name.Any(char.IsControl) &&
            Enum.IsDefined(character.Version) && Enum.IsDefined(character.Class) &&
            CharacterDefinition.Specializations(character.Class).Any(spec => spec.Id == character.SelectedSpecialization) &&
            character.OwnedItemKeys is not null && character.OwnedItemKeys.All(key => !string.IsNullOrWhiteSpace(key)) &&
            character.OwnedItemKeys.Distinct(StringComparer.Ordinal).Count() == character.OwnedItemKeys.Length &&
            ValidEquipment(character.Class, character.EquippedBySpec) &&
            (character.CatalogSetId is null || !string.IsNullOrWhiteSpace(character.CatalogSetId)) &&
            (character.ArchivedLoadouts is null || character.ArchivedLoadouts.All(pair => !string.IsNullOrWhiteSpace(pair.Key) && ValidEquipment(character.Class, pair.Value)))) &&
        state.Characters.Select(character => character.Id).Distinct().Count() == state.Characters.Length &&
        state.Characters.Any(character => character.Id == state.ActiveCharacterId);

    private static bool ValidEquipment(CharacterClass characterClass, Dictionary<string, Dictionary<EquipmentSlot, string>>? lists) =>
        lists is { Count: 3 } && CharacterDefinition.Specializations(characterClass).All(spec =>
            lists.TryGetValue(spec.Id, out var equipment) && equipment is not null &&
            equipment.All(pair => Enum.IsDefined(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value)));
}
