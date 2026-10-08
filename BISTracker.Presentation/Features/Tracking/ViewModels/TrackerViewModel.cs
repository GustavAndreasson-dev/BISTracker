using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Presentation.Common;

namespace BISTracker.Presentation.Features.Tracking.ViewModels;

public sealed class TrackerViewModel : ObservableObject
{
    private readonly ICharacterTrackerService _service;
    private readonly ICatalogPackImporter? _catalogImporter;
    private readonly List<TrackerItemViewModel> _items = new();
    private readonly List<TrackerSlotViewModel> _slots = new();
    private EquipmentPlan? _plan;
    private BisCatalog? _catalogSnapshot;
    private EquipmentCoverage? _catalogCoverage;
    private EquipmentCoverage? _allianceCoverage;
    private EquipmentCoverage? _hordeCoverage;
    private EquipmentCoverage? _ownedCoverage;
    private bool _usesTwoHandedWeapon;
    private bool _isBusy;
    private bool _loaded;
    private string _searchText = "";
    private int _filterIndex;
    private string _errorMessage = "";
    private string _saveStatus = "Loading your progress…";
    private string? _unavailableReason;
    private CharacterOption? _selectedCharacter;
    private Specialization? _selectedSpecialization;
    private CatalogSet? _selectedCatalogSet;
    public TrackerViewModel(ICharacterTrackerService service, bool hasDraftProgress = false, ICatalogPackImporter? catalogImporter = null)
    {
        _service = service;
        _catalogImporter = catalogImporter;
        HasDraftProgress = hasDraftProgress;
    }
    public bool HasDraftProgress { get; }
    public string CatalogContext { get; private set; } = "";
    public string CatalogLabel { get; private set; } = "";
    public string ListTitle => IsWorkspaceEmpty ? "No characters" : SelectedCharacter?.Version == GameVersion.Forever ? $"Your level {SelectedCatalogSet?.LevelCap ?? 30} gear" : "Your pre-raid gear";
    public string NavigationLabel => SelectedCharacter?.Version == GameVersion.Forever ? $"Level {SelectedCatalogSet?.LevelCap ?? 30} gear" : "Pre-raid BiS";
    public string SelectionMethod { get; private set; } = "";
    public bool ShowClassicContext => HasReviewedCatalog && SelectedCharacter?.Version == GameVersion.Classic;
    public bool CanImportCatalog => IsInteractive && _catalogImporter is not null;
    public bool IsWorkspaceEmpty => _loaded && _selectedCharacter is null;
    public bool IsCharacterInteractive => IsInteractive && _selectedCharacter is not null;
    public string CharacterContentVisibility => IsWorkspaceEmpty ? "Collapsed" : "Visible";
    public string NoCharactersVisibility => IsWorkspaceEmpty ? "Visible" : "Collapsed";
    public bool HasReviewedCatalog => _loaded && _unavailableReason is null && TotalCount > 0;
    public ObservableCollection<CharacterOption> Characters { get; } = new();
    public ObservableCollection<Specialization> Specializations { get; } = new();
    public ObservableCollection<CatalogSet> CatalogSets { get; } = new();
    public CharacterOption? SelectedCharacter => _selectedCharacter;
    public Specialization? SelectedSpecialization => _selectedSpecialization;
    public CatalogSet? SelectedCatalogSet => _selectedCatalogSet;
    public ObservableCollection<TrackerItemViewModel> VisibleItems { get; } = new();
    public ObservableCollection<TrackerSlotViewModel> VisibleSlots { get; } = new();
    private bool IsRequired(TrackerSlotViewModel slot) => slot.Slot != EquipmentSlot.OffHand || !_usesTwoHandedWeapon;
    public int TotalCount => _slots.Count(IsRequired);
    public int OwnedCount => _ownedCoverage?.CoveredSlots.Count ?? 0;
    public int RemainingCount => TotalCount - OwnedCount;
    public string OwnedSummary => $"{OwnedCount} / {TotalCount}";
    public string EquippedSummary => $"{_slots.Count(slot => IsRequired(slot) && slot.IsEquipped)} / {TotalCount}";
    public string VisibleSummary => $"Showing {VisibleSlots.Count} of {TotalCount} slots";
    public bool HasCatalogGaps => _catalogCoverage is { IsComplete: false } || _allianceCoverage is { IsComplete: false } || _hordeCoverage is { IsComplete: false };
    public string CatalogStatus => !HasCatalogGaps ? "" : string.Join(" · ", new[] { CoverageStatus("Alliance", _allianceCoverage), CoverageStatus("Horde", _hordeCoverage) }.Where(value => value.Length > 0));
    private static string CoverageStatus(string faction, EquipmentCoverage? coverage) => coverage is not { IsComplete: false } ? "" :
        $"{faction}: " + string.Join(", ", coverage.MissingSlots.Select(TrackerItemViewModel.TranslateSlot));
    public string EmptyMessage => _loaded && VisibleSlots.Count == 0 ? _unavailableReason ?? "No items match your search or filter." : "";
    public string EmptyStateVisibility => EmptyMessage.Length > 0 ? "Visible" : "Collapsed";
    public bool IsInteractive => _loaded && !_isBusy;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ErrorMessage { get => _errorMessage; private set { if (Set(ref _errorMessage, value)) Notify(nameof(HasError)); } }
    public string SaveStatus { get => _saveStatus; private set => Set(ref _saveStatus, value); }
    public string SearchText { get => _searchText; set { if (Set(ref _searchText, value)) ApplyFilter(); } }
    public int FilterIndex { get => _filterIndex; set { if (Set(ref _filterIndex, value)) ApplyFilter(); } }
    public Task LoadAsync() => ExecuteAsync(() => _service.LoadAsync(), false);
    public Task SetOwnedAsync(string id, bool owned) => ExecuteAsync(() => _service.SetOwnedAsync(id, owned), true);
    public Task SetEquippedAsync(string id, bool equipped) => ExecuteAsync(() => _service.SetEquippedAsync(id, equipped), true);
    public Task SelectCharacterAsync(CharacterOption character) => ExecuteAsync(() => _service.SelectAsync(character.Id, character.SelectedSpecialization), true);
    public Task SelectSpecializationAsync(Specialization spec) => ExecuteAsync(() => _service.SelectAsync(SelectedCharacter!.Id, spec.Id), true);
    public Task CreateCharacterAsync(string name, GameVersion version, CharacterClass characterClass) =>
        ExecuteAsync(() => _service.CreateAsync(name, version, characterClass), true);
    public Task RenameCharacterAsync(CharacterOption character, string name) => ExecuteAsync(() => _service.RenameAsync(character.Id, name), true);
    public Task DeleteCharacterAsync(CharacterOption character) => ExecuteAsync(() => _service.DeleteAsync(character.Id), true);
    public Task SelectCatalogAsync(CatalogSet set) => ExecuteAsync(() => _service.SelectCatalogAsync(set.Id), true);
    public Task ImportCatalogAsync(string directory) => ExecuteAsync(async () =>
    {
        await (_catalogImporter ?? throw new InvalidOperationException("Catalog import is unavailable.")).ImportAsync(directory);
        return await _service.LoadAsync();
    }, false, importsCatalog: true);
    private async Task ExecuteAsync(Func<Task<TrackerSnapshot>> operation, bool saves, bool importsCatalog = false)
    {
        if (_isBusy) return;
        _isBusy = true;
        Notify(nameof(IsInteractive));
        Notify(nameof(CanImportCatalog));
        Notify(nameof(IsCharacterInteractive));
        ErrorMessage = "";
        SaveStatus = saves ? "Saving progress…" : "Loading your progress…";
        try
        {
            var snapshot = await operation();
            var contextChanged = !_loaded || !Equals(_selectedCharacter?.Id, snapshot.Selection?.ActiveCharacter.Id) ||
                _selectedSpecialization?.Id != snapshot.Selection?.ActiveSpecialization.Id || _selectedCatalogSet?.Id != snapshot.Selection?.ActiveCatalogSet?.Id;
            var itemsChanged = !_items.Select(item => item.Id).SequenceEqual(snapshot.Entries.Select(entry => entry.Item.Id));
            if (contextChanged || itemsChanged || importsCatalog || CatalogChanged(snapshot.Catalog))
            {
                _catalogSnapshot = snapshot.Catalog;
                _items.Clear();
                _items.AddRange(snapshot.Entries.Select(entry => new TrackerItemViewModel(entry)));
                _plan = snapshot.Catalog.UnavailableReason is null ? snapshot.Catalog.EquipmentPlan : null;
                _catalogCoverage = _plan?.CatalogCoverage;
                _allianceCoverage = _plan?.CatalogCoverageFor(CharacterFaction.Alliance);
                _hordeCoverage = _plan?.CatalogCoverageFor(CharacterFaction.Horde);
                _slots.Clear();
                if (_plan is not null)
                    _slots.AddRange(_plan.Goals.Select(goal => new TrackerSlotViewModel(goal.Slot, _items.Where(item => item.Slot == goal.Slot).ToArray())));
                if (contextChanged) { SearchText = ""; FilterIndex = 0; }
                _loaded = true;
            }
            else
            {
                foreach (var entry in snapshot.Entries) _items.Single(item => item.Id == entry.Item.Id).Update(entry);
            }
            var equippedMain = snapshot.Entries.SingleOrDefault(entry => entry.IsEquipped && entry.Item.Slot == EquipmentSlot.MainHand);
            bool? handChoice = equippedMain is not null ? equippedMain.Item.Details?.WeaponKind == WeaponKind.TwoHanded :
                snapshot.Entries.Any(entry => entry.IsEquipped && entry.Item.Slot == EquipmentSlot.OffHand) ? false : null;
            _ownedCoverage = _plan?.OwnedCoverage(snapshot.Entries.Where(entry => entry.IsOwned).Select(entry => CharacterLoadouts.ItemKey(entry.Item)),
                handChoice, snapshot.Entries.Where(entry => entry.IsEquipped).Select(entry => entry.Item.Slot));
            _usesTwoHandedWeapon = _ownedCoverage is not null && !_ownedCoverage.RequiredSlots.Contains(EquipmentSlot.OffHand);
            foreach (var slot in _slots)
                slot.Update(_ownedCoverage?.CoveredSlots.Contains(slot.Slot) == true,
                    _catalogCoverage?.MissingSlots.Contains(slot.Slot) == true || _allianceCoverage?.MissingSlots.Contains(slot.Slot) == true || _hordeCoverage?.MissingSlots.Contains(slot.Slot) == true);
            CatalogContext = snapshot.Catalog.Context.Phase;
            CatalogLabel = snapshot.Selection is null ? "" : $"{snapshot.Catalog.Context.Version} / {snapshot.Catalog.Context.Specialization}";
            SelectionMethod = snapshot.Catalog.SelectionMethod ?? "";
            _unavailableReason = snapshot.Catalog.UnavailableReason;
            if (snapshot.Selection is { } selection)
            {
                Characters.Clear();
                foreach (var character in selection.Characters) Characters.Add(character);
                _selectedCharacter = Characters.Single(character => character.Id == selection.ActiveCharacter.Id);
                Specializations.Clear();
                foreach (var spec in CharacterDefinition.Specializations(selection.ActiveCharacter.Class)) Specializations.Add(spec);
                _selectedSpecialization = Specializations.Single(spec => spec.Id == selection.ActiveSpecialization.Id);
                CatalogSets.Clear();
                foreach (var set in selection.CatalogSets ?? Array.Empty<CatalogSet>()) CatalogSets.Add(set);
                _selectedCatalogSet = CatalogSets.SingleOrDefault(set => set.Id == selection.ActiveCatalogSet?.Id);
            }
            else
            {
                // A character snapshot without a selection means that the workspace has no characters.
                Characters.Clear();
                Specializations.Clear();
                CatalogSets.Clear();
                _selectedCharacter = null;
                _selectedSpecialization = null;
                _selectedCatalogSet = null;
            }
            foreach (var property in new[] { nameof(CatalogContext), nameof(CatalogLabel), nameof(HasReviewedCatalog), nameof(ShowClassicContext), nameof(ListTitle), nameof(NavigationLabel), nameof(SelectionMethod), nameof(HasCatalogGaps), nameof(CatalogStatus),
                nameof(IsWorkspaceEmpty), nameof(CharacterContentVisibility), nameof(NoCharactersVisibility) }) Notify(property);
            foreach (var property in new[] { nameof(TotalCount), nameof(OwnedCount), nameof(RemainingCount), nameof(OwnedSummary), nameof(EquippedSummary) }) Notify(property);
            ApplyFilter();
            SaveStatus = importsCatalog ? "Catalogs imported locally." : saves ? $"Saved locally · {DateTime.Now:HH:mm}" : "Your progress is saved locally on this computer.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            ErrorMessage = $"{(importsCatalog ? ex.Message : DescribeError(ex, saves))} Your progress in this view has not changed.";
            SaveStatus = importsCatalog ? "Catalog import or refresh failed." : saves
                ? "Changes were not saved. Check the progress file and try again."
                : "Could not load progress. Check the progress file and restart the app.";
        }
        finally
        {
            foreach (var item in _items) item.RefreshTrackingState();
            Notify(nameof(SelectedCharacter));
            Notify(nameof(SelectedSpecialization));
            Notify(nameof(SelectedCatalogSet));
            _isBusy = false;
            Notify(nameof(IsInteractive));
            Notify(nameof(CanImportCatalog));
            Notify(nameof(IsCharacterInteractive));
        }
    }
    private static string DescribeError(Exception exception, bool saves) => exception switch
    {
        InvalidDataException => "The progress file contains invalid data.",
        ArgumentException { ParamName: "name" } => "Enter a character name with 1–40 characters.",
        UnauthorizedAccessException => "Access to the progress file was denied.",
        IOException => saves
            ? "The progress file could not be saved. Make sure it is accessible and is not in use by another app."
            : "The progress file could not be read. Make sure it is accessible and is not in use by another app.",
        ArgumentException => "The saved progress or item catalog contains invalid item data.",
        OperationCanceledException => "The update was canceled.",
        _ => saves ? "Your progress could not be saved." : "Your progress could not be loaded."
    };
    private bool CatalogChanged(BisCatalog catalog) => _catalogSnapshot is not { } previous ||
        previous.Context != catalog.Context || previous.Set != catalog.Set || previous.IsSample != catalog.IsSample ||
        previous.UnavailableReason != catalog.UnavailableReason || previous.SelectionMethod != catalog.SelectionMethod ||
        previous.WeaponSetup != catalog.WeaponSetup || previous.WeaponSetupSourceUrl != catalog.WeaponSetupSourceUrl ||
        !previous.Items.SequenceEqual(catalog.Items) ||
        !(previous.SlotExemptions ?? Array.Empty<SlotExemption>()).SequenceEqual(catalog.SlotExemptions ?? Array.Empty<SlotExemption>());
    private void ApplyFilter()
    {
        var term = SearchText.Trim();
        var slots = _slots.Where(slot => IsRequired(slot) && slot.ApplyFilter(term, FilterIndex)).ToArray();
        var matches = slots.SelectMany(slot => slot.VisibleAlternatives).ToArray();
        if (!VisibleSlots.SequenceEqual(slots))
        {
            VisibleSlots.Clear();
            foreach (var slot in slots) VisibleSlots.Add(slot);
        }
        if (!VisibleItems.SequenceEqual(matches))
        {
            VisibleItems.Clear();
            foreach (var item in matches) VisibleItems.Add(item);
        }
        Notify(nameof(VisibleSummary));
        Notify(nameof(EmptyMessage));
        Notify(nameof(EmptyStateVisibility));
    }
}
