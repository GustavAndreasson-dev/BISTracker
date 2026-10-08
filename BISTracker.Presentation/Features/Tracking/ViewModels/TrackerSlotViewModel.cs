using System;
using System.Collections.ObjectModel;
using System.Linq;
using BISTracker.Domain;
using BISTracker.Presentation.Common;

namespace BISTracker.Presentation.Features.Tracking.ViewModels;

public sealed class TrackerSlotViewModel : ObservableObject
{
    private bool _isExpanded;
    private string _status = "";
    private bool _isOwned;
    public TrackerSlotViewModel(EquipmentSlot slot, TrackerItemViewModel[] alternatives)
    {
        Slot = slot;
        Alternatives = alternatives;
        foreach (var item in alternatives) VisibleAlternatives.Add(item);
        _isExpanded = alternatives.Length == 1;
    }
    public EquipmentSlot Slot { get; }
    public string SlotName => TrackerItemViewModel.TranslateSlot(Slot);
    public TrackerItemViewModel[] Alternatives { get; }
    public ObservableCollection<TrackerItemViewModel> VisibleAlternatives { get; } = new();
    public bool IsExpanded { get => _isExpanded; set => Set(ref _isExpanded, value); }
    public string Status { get => _status; private set => Set(ref _status, value); }
    public bool IsOwned => _isOwned;
    public bool IsEquipped => Alternatives.Any(item => item.IsEquipped);
    public void Update(bool owned, bool hasCatalogGap)
    {
        _isOwned = owned;
        Status = Alternatives.Length == 0 ? "No reviewed item available" : hasCatalogGap ? "No complete item combination" :
            IsEquipped ? Alternatives.Single(item => item.IsEquipped).Name :
            $"{Alternatives.Length} {(Alternatives.Length == 1 ? "option" : "alternatives")}";
        Notify(nameof(IsOwned));
        Notify(nameof(IsEquipped));
    }
    public bool ApplyFilter(string term, int filter)
    {
        var matchesSlot = SlotName.Contains(term, StringComparison.OrdinalIgnoreCase);
        var matches = Alternatives.Where(item => matchesSlot || item.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            item.Source.Contains(term, StringComparison.OrdinalIgnoreCase)).ToArray();
        if ((filter == 1 && IsOwned) || (filter == 2 && !IsOwned) || (filter == 3 && !IsEquipped)) return false;
        if (term.Length > 0 && !matchesSlot && matches.Length == 0) return false;
        if (!VisibleAlternatives.SequenceEqual(matches))
        {
            VisibleAlternatives.Clear();
            foreach (var item in matches) VisibleAlternatives.Add(item);
        }
        if (term.Length > 0 && matches.Length > 0) IsExpanded = true;
        return true;
    }
}
