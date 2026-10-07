using BISTracker.Domain;

namespace BISTracker.Application;

public sealed record ProgressState(string[] OwnedItemIds, Dictionary<EquipmentSlot, string> EquippedItems);
