using BISTracker.Application;
using BISTracker.Domain;

namespace BISTracker.Infrastructure;

/// <summary>Fictional placeholders for the draft UI, never a verified BiS list.</summary>
public sealed class SampleBisCatalog : IBisCatalog
{
    private static readonly BisCatalog Catalog = new(
        new GameContext("WoW Classic (Vanilla)", "Holy Priest", "Pre-raid · fas ej beslutad"),
        Array.AsReadOnly(new[]
        {
            Example("head", EquipmentSlot.Head, "huvud"),
            Example("neck", EquipmentSlot.Neck, "hals"),
            Example("shoulder", EquipmentSlot.Shoulder, "axlar"),
            Example("back", EquipmentSlot.Back, "rygg"),
            Example("chest", EquipmentSlot.Chest, "bröst"),
            Example("wrist", EquipmentSlot.Wrist, "handleder"),
            Example("hands", EquipmentSlot.Hands, "händer"),
            Example("waist", EquipmentSlot.Waist, "midja"),
            Example("legs", EquipmentSlot.Legs, "ben"),
            Example("feet", EquipmentSlot.Feet, "fötter"),
            Example("finger1", EquipmentSlot.Finger1, "ringplats 1"),
            Example("finger2", EquipmentSlot.Finger2, "ringplats 2"),
            Example("trinket1", EquipmentSlot.Trinket1, "smyckesplats 1"),
            Example("trinket2", EquipmentSlot.Trinket2, "smyckesplats 2"),
            Example("mainhand", EquipmentSlot.MainHand, "huvudhand"),
            Example("offhand", EquipmentSlot.OffHand, "andra handen"),
            Example("ranged", EquipmentSlot.Ranged, "distansplats")
        }),
        IsSample: true);

    public Task<BisCatalog> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Catalog);
    }

    private static Recommendation Example(string id, EquipmentSlot slot, string name) =>
        new($"demo-{id}", slot, $"Exempel: {name}", "Fiktiv exempeldata för apputkastet",
            "Platshållare, inte ett WoW-item eller en BiS-rekommendation. Källa och urval återstår.");
}
