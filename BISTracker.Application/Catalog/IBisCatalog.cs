namespace BISTracker.Application;

public interface IBisCatalog
{
    Task<BisCatalog> LoadAsync(CancellationToken cancellationToken = default);
}
