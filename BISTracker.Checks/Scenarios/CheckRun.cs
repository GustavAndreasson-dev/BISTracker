using BISTracker.Application;
using BISTracker.Infrastructure;

namespace BISTracker.Checks.Scenarios;

/// <summary>Runs checks against a uniquely named, disposable temporary directory.</summary>
internal sealed class CheckRun : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"BISTracker.Checks-{Guid.NewGuid():N}");

    public CheckRun() => Directory.CreateDirectory(_temporaryDirectory);

    public int Failures { get; private set; }

    public async Task RunAsync(string name, Func<Task> action)
    {
        try
        {
            await action();
            Console.WriteLine($"PASS: {name}");
        }
        catch (Exception exception)
        {
            Failures++;
            Console.Error.WriteLine($"FAIL: {name}\n{exception}");
        }
    }

    public string PathFor(string relativePath) => Path.Combine(_temporaryDirectory, relativePath);
    public JsonProgressRepository Repository(string relativePath) => new(PathFor(relativePath));
    public TrackerService Service(string relativePath) => new(new SampleBisCatalog(), Repository(relativePath));

    public void Dispose()
    {
        // This directory is a freshly created, unique child of the OS temporary directory.
        Directory.Delete(_temporaryDirectory, recursive: true);
    }
}
