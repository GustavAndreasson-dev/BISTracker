namespace BISTracker.Domain;

public enum AcquisitionType { Dungeon, Quest }

/// <summary>Item identity and reference facts, independent of any database API.</summary>
public sealed record ItemDetails(int ClassicItemId, string? RequiredSuffix, AcquisitionType AcquisitionType,
    string IconUrl, string ItemUrl, string RecommendationUrl);
