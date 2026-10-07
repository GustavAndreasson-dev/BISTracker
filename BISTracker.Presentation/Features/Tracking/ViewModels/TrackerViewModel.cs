using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BISTracker.Application;
using BISTracker.Presentation.Common;

namespace BISTracker.Presentation.Features.Tracking.ViewModels;

public sealed class TrackerViewModel : ObservableObject
{
    private readonly TrackerService _service;
    private readonly List<TrackerItemViewModel> _items = new();
    private bool _isBusy;
    private bool _loaded;
    private string _searchText = "";
    private int _filterIndex;
    private string _errorMessage = "";
    private string _saveStatus = "Loading your progress…";
    public TrackerViewModel(TrackerService service) => _service = service;
    public ObservableCollection<TrackerItemViewModel> VisibleItems { get; } = new();
    public int TotalCount => _items.Count;
    public int OwnedCount => _items.Count(item => item.IsOwned);
    public int RemainingCount => TotalCount - OwnedCount;
    public string OwnedSummary => $"{OwnedCount} / {TotalCount}";
    public string EquippedSummary => $"{_items.Count(item => item.IsEquipped)} / {TotalCount}";
    public string VisibleSummary => $"Showing {VisibleItems.Count} of {TotalCount} items";
    public string EmptyMessage => _loaded && VisibleItems.Count == 0 ? "No items match your search or filter." : "";
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
    private async Task ExecuteAsync(Func<Task<TrackerSnapshot>> operation, bool saves)
    {
        if (_isBusy) return;
        _isBusy = true;
        Notify(nameof(IsInteractive));
        ErrorMessage = "";
        SaveStatus = saves ? "Saving progress…" : "Loading your progress…";
        try
        {
            var snapshot = await operation();
            if (!_loaded)
            {
                _items.AddRange(snapshot.Entries.Select(entry => new TrackerItemViewModel(entry)));
                _loaded = true;
            }
            else
            {
                foreach (var entry in snapshot.Entries) _items.Single(item => item.Id == entry.Item.Id).Update(entry);
            }
            foreach (var property in new[] { nameof(TotalCount), nameof(OwnedCount), nameof(RemainingCount), nameof(OwnedSummary), nameof(EquippedSummary) }) Notify(property);
            ApplyFilter();
            SaveStatus = saves ? $"Saved locally · {DateTime.Now:HH:mm}" : "Your progress is saved locally on this computer.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            ErrorMessage = $"{DescribeError(ex, saves)} Your progress in this view has not changed.";
            SaveStatus = saves
                ? "Changes were not saved. Check the progress file and try again."
                : "Could not load progress. Check the progress file and restart the app.";
        }
        finally
        {
            foreach (var item in _items) item.RefreshTrackingState();
            _isBusy = false;
            Notify(nameof(IsInteractive));
        }
    }
    private static string DescribeError(Exception exception, bool saves) => exception switch
    {
        InvalidDataException => "The progress file contains invalid data.",
        UnauthorizedAccessException => "Access to the progress file was denied.",
        IOException => saves
            ? "The progress file could not be saved. Make sure it is accessible and is not in use by another app."
            : "The progress file could not be read. Make sure it is accessible and is not in use by another app.",
        ArgumentException => "The saved progress or item catalog contains invalid item data.",
        OperationCanceledException => "The update was canceled.",
        _ => saves ? "Your progress could not be saved." : "Your progress could not be loaded."
    };
    private void ApplyFilter()
    {
        var term = SearchText.Trim();
        var matches = _items.Where(item =>
            (term.Length == 0 || item.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || item.SlotName.Contains(term, StringComparison.OrdinalIgnoreCase)) &&
            (FilterIndex switch { 1 => !item.IsOwned, 2 => item.IsOwned, 3 => item.IsEquipped, _ => true })).ToArray();
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
