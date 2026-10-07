using BISTracker.Application;
using BISTracker.Domain;

namespace BISTracker.Infrastructure;

/// <summary>Fictional placeholders for the draft UI, never a verified BiS list.</summary>
public sealed class SampleBisCatalog : IBisCatalog
{
    private static readonly BisCatalog Catalog = new(
        new GameContext("WoW Classic (Vanilla)", "Holy Priest", "Phase 1 · dungeons and quests"),
        Array.AsReadOnly(new[]
        {
            Example("head", EquipmentSlot.Head, "head"),
            Example("neck", EquipmentSlot.Neck, "neck"),
            Example("shoulder", EquipmentSlot.Shoulder, "shoulders"),
            Example("back", EquipmentSlot.Back, "back"),
            Example("chest", EquipmentSlot.Chest, "chest"),
            Example("wrist", EquipmentSlot.Wrist, "wrists"),
            Example("hands", EquipmentSlot.Hands, "hands"),
            Example("waist", EquipmentSlot.Waist, "waist"),
            Example("legs", EquipmentSlot.Legs, "legs"),
            Example("feet", EquipmentSlot.Feet, "feet"),
            Example("finger1", EquipmentSlot.Finger1, "ring 1"),
            Example("finger2", EquipmentSlot.Finger2, "ring 2"),
            Example("trinket1", EquipmentSlot.Trinket1, "trinket 1"),
            Example("trinket2", EquipmentSlot.Trinket2, "trinket 2"),
            Example("mainhand", EquipmentSlot.MainHand, "main hand"),
            Example("offhand", EquipmentSlot.OffHand, "off hand"),
            Example("ranged", EquipmentSlot.Ranged, "wand")
        }),
        IsSample: true);

    public Task<BisCatalog> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Catalog);
    }

    private static Recommendation Example(string id, EquipmentSlot slot, string name) =>
        new($"demo-{id}", slot, $"Example: {name}", "Fictional sample data for the app draft",
            "Placeholder, not a WoW item or a BiS recommendation. Sources and selection are still being reviewed.");
}
