namespace BISTracker.Application;

public interface ICatalogPackImporter
{
    Task ImportAsync(string sourceDirectory, CancellationToken cancellationToken = default);
}
