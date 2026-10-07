using BISTracker.Application;

namespace BISTracker.Checks.Scenarios;

internal static class CheckAssertions
{
    public static TrackerEntry Entry(TrackerSnapshot snapshot, string itemId) =>
        snapshot.Entries.Single(entry => entry.Item.Id == itemId);

    public static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static async Task Throws<TException>(Func<Task> action) where TException : Exception
    {
        try { await action(); }
        catch (TException) { return; }
        throw new InvalidOperationException($"Förväntat fel: {typeof(TException).Name}.");
    }
}
