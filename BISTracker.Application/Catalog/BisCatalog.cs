using BISTracker.Domain;

namespace BISTracker.Application;

public sealed record BisCatalog(GameContext Context, IReadOnlyList<Recommendation> Items, bool IsSample, string? UnavailableReason = null);
