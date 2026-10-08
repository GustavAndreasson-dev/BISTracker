using BISTracker.Application;

namespace BISTracker.Infrastructure;

public sealed class CatalogPackImporter : ICatalogPackImporter
{
    private readonly string _directory;
    private readonly SemaphoreSlim _gate = new(1, 1);
    public CatalogPackImporter(string directory) => _directory = Path.GetFullPath(directory);

    public async Task ImportAsync(string sourceDirectory, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? staging = null;
        try
        {
            var sources = Directory.GetFiles(Path.GetFullPath(sourceDirectory), "*.json", SearchOption.AllDirectories);
            if (sources.Length == 0) throw new InvalidDataException("The selected folder contains no catalog JSON files.");
            var existingIds = new CharacterCatalog(_directory).CatalogIds().ToHashSet(StringComparer.Ordinal);
            var batch = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            foreach (var source in sources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var bytes = await File.ReadAllBytesAsync(source, cancellationToken).ConfigureAwait(false);
                using var stream = new MemoryStream(bytes, writable: false);
                var pack = ForeverCatalogReader.Read(stream, includeOtherSources: true);
                if (existingIds.Contains(pack.CatalogId) || !batch.TryAdd(pack.CatalogId, bytes))
                    throw new InvalidDataException("A catalog with this ID already exists. The existing catalog has been preserved.");
            }
            // Stage beside the destination so a directory move publishes the entire validated batch at once.
            var parent = Path.GetDirectoryName(_directory)!;
            Directory.CreateDirectory(parent);
            staging = Path.Combine(parent, $".catalog-import-{Guid.NewGuid():N}");
            Directory.CreateDirectory(staging);
            foreach (var pair in batch)
                await File.WriteAllBytesAsync(Path.Combine(staging, pair.Key + ".json"), pair.Value, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(_directory);
            Directory.Move(staging, Path.Combine(_directory, "import-" + Guid.NewGuid().ToString("N")));
            staging = null;
        }
        finally
        {
            // This unique staging child was created by this operation, never a source/user directory.
            if (staging is not null)
            {
                try { Directory.Delete(staging, recursive: true); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            _gate.Release();
        }
    }
}
