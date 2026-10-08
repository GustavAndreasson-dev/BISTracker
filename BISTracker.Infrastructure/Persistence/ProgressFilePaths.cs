namespace BISTracker.Infrastructure;

/// <summary>Separate progress profiles prevent fictional draft IDs from becoming real item ownership.</summary>
public static class ProgressFilePaths
{
    public static string Characters(string directory) => Path.Combine(directory, "characters-v1.json");
    public static string ClassicPhaseOne(string directory) => Path.Combine(directory, "holy-priest-classic-phase1-progress.json");
    public static string Draft(string directory) => Path.Combine(directory, "draft-progress.json");
}
