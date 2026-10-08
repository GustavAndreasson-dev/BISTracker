using BISTracker.Domain;

namespace BISTracker.Application;

public sealed record BisCatalog(GameContext Context, IReadOnlyList<Recommendation> Items, bool IsSample, string? UnavailableReason = null,
    CatalogSet? Set = null, string? SelectionMethod = null, IReadOnlyList<SlotExemption>? SlotExemptions = null,
    WeaponSetup WeaponSetup = WeaponSetup.Flexible, string? WeaponSetupSourceUrl = null)
{
    public EquipmentPlan EquipmentPlan => new(Items, SlotExemptions, WeaponSetup);
}
