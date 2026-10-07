using System.Text.Json;
using System.Text.Json.Serialization;
using BISTracker.Application;

namespace BISTracker.Infrastructure;

public sealed class JsonProgressRepository : IProgressRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonProgressRepository(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = Path.GetFullPath(filePath);
    }

    public async Task<ProgressState> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await ReadAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(ProgressState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (!HasValidShape(state))
            throw new ArgumentException("Progress must contain valid collections and item IDs.", nameof(state));

        await _gate.WaitAsync(cancellationToken);
        string? temporaryPath = null;
        try
        {
            // Refuse to overwrite unreadable data, including direct calls to SaveAsync.
            await ReadAsync(cancellationToken);
            var directory = Path.GetDirectoryName(_filePath)!;
            Directory.CreateDirectory(directory);
            temporaryPath = Path.Combine(directory, $".{Path.GetFileName(_filePath)}.{Guid.NewGuid():N}.tmp");
            await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write,
                FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
                stream.Flush(flushToDisk: true);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(_filePath))
                File.Replace(temporaryPath, _filePath, destinationBackupFileName: null);
            else
                File.Move(temporaryPath, _filePath);
            temporaryPath = null;
        }
        finally
        {
            if (temporaryPath is not null)
            {
                // A cleanup failure must not hide the original persistence/cancellation error.
                try { File.Delete(temporaryPath); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            _gate.Release();
        }
    }

    private async Task<ProgressState> ReadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FileStream stream;
        try
        {
            stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                4096, FileOptions.Asynchronous);
        }
        catch (FileNotFoundException)
        {
            return Empty();
        }
        catch (DirectoryNotFoundException)
        {
            return Empty();
        }

        await using (stream)
        {
            try
            {
                var state = await JsonSerializer.DeserializeAsync<ProgressState>(stream, JsonOptions, cancellationToken);
                if (state is null || !HasValidShape(state))
                    throw new JsonException("The progress file is missing valid collections or item IDs.");
                return state;
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The progress file contains invalid JSON. The existing file has been preserved.", exception);
            }
        }
    }

    // Domain relationships (ownership, equipment and catalogue IDs) belong to the aggregate.
    private static bool HasValidShape(ProgressState state) =>
        state.OwnedItemIds is not null && state.EquippedItems is not null &&
        state.OwnedItemIds.All(id => !string.IsNullOrWhiteSpace(id)) &&
        state.EquippedItems.All(entry => Enum.IsDefined(entry.Key) && !string.IsNullOrWhiteSpace(entry.Value));

    private static ProgressState Empty() => new([], []);
}
