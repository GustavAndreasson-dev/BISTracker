namespace BISTracker.Domain;

public sealed record Recommendation(string Id, EquipmentSlot Slot, string Name, string Source, string Note,
    ItemDetails? Details = null);
