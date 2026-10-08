using System;
using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Presentation.Common;

namespace BISTracker.Presentation.Features.Tracking.ViewModels;

public sealed class TrackerItemViewModel : ObservableObject
{
    private bool _isOwned;
    private bool _isEquipped;
    private bool _isIconLoaded;
    public TrackerItemViewModel(TrackerEntry entry)
    {
        Id = entry.Item.Id;
        Name = entry.Item.Name;
        Source = entry.Item.Source;
        Note = entry.Item.Note;
        IconUrl = entry.Item.Details?.IconUrl;
        ItemUri = entry.Item.Details is null ? null : new Uri(entry.Item.Details.ItemUrl);
        RecommendationUri = entry.Item.Details is null ? null : new Uri(entry.Item.Details.RecommendationUrl);
        SlotName = TranslateSlot(entry.Item.Slot);
        Update(entry);
    }
    public string Id { get; }
    public string Name { get; }
    public string Source { get; }
    public string Note { get; }
    public string NoteVisibility => string.IsNullOrEmpty(Note) ? "Collapsed" : "Visible";
    public string? IconUrl { get; }
    public Uri? ItemUri { get; }
    public Uri? RecommendationUri { get; }
    public bool IsIconLoaded => _isIconLoaded;
    public string IconPlaceholderVisibility => _isIconLoaded ? "Collapsed" : "Visible";
    public string SlotName { get; }
    public string OwnedAccessibleName => $"{Name}, owned";
    public string EquippedAccessibleName => $"{Name}, equipped";
    public bool IsOwned { get => _isOwned; private set => Set(ref _isOwned, value); }
    public bool IsEquipped { get => _isEquipped; private set => Set(ref _isEquipped, value); }
    public void Update(TrackerEntry entry)
    {
        IsOwned = entry.IsOwned;
        IsEquipped = entry.IsEquipped;
    }
    public void RefreshTrackingState()
    {
        Notify(nameof(IsOwned));
        Notify(nameof(IsEquipped));
    }
    public void SetIconLoaded(bool loaded)
    {
        if (Set(ref _isIconLoaded, loaded))
        {
            Notify(nameof(IsIconLoaded));
            Notify(nameof(IconPlaceholderVisibility));
        }
    }
    private static string TranslateSlot(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Head => "Head", EquipmentSlot.Neck => "Neck",
        EquipmentSlot.Shoulder => "Shoulders", EquipmentSlot.Back => "Back",
        EquipmentSlot.Chest => "Chest", EquipmentSlot.Wrist => "Wrists",
        EquipmentSlot.Hands => "Hands", EquipmentSlot.Waist => "Waist",
        EquipmentSlot.Legs => "Legs", EquipmentSlot.Feet => "Feet",
        EquipmentSlot.Finger1 => "Ring 1", EquipmentSlot.Finger2 => "Ring 2",
        EquipmentSlot.Trinket1 => "Trinket 1", EquipmentSlot.Trinket2 => "Trinket 2",
        EquipmentSlot.MainHand => "Main hand", EquipmentSlot.OffHand => "Off hand",
        EquipmentSlot.Ranged => "Ranged", _ => slot.ToString()
    };
}
