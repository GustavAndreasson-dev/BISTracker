using BISTracker.Domain;

namespace BISTracker.Application;

public sealed record TrackerEntry(Recommendation Item, bool IsOwned, bool IsEquipped);
