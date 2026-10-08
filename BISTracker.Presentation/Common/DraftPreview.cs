#if DRAFT_PREVIEW
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BISTracker.Application;
using BISTracker.Domain;
using BISTracker.Infrastructure;
using BISTracker.Presentation.Features.Characters.Views;
using BISTracker.Presentation.Features.Tracking.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BISTracker.Presentation.Common;

// Compiled only for a requested verification build; never accesses real progress.
internal static class DraftPreview
{
    private static readonly string[] Arguments = Environment.GetCommandLineArgs();
    private static readonly int FlagIndex = Array.IndexOf(Arguments, "--draft-preview");
    public static bool IsRequested => FlagIndex >= 0 && FlagIndex + 1 < Arguments.Length;
    public static PreviewWorkspaceRepository Workspace { get; } = new();

    public static async Task RunAsync(FrameworkElement root, TrackerViewModel model)
    {
        var directory = Path.GetFullPath(Arguments[FlagIndex + 1]);
        Directory.CreateDirectory(directory);
        try
        {
            Require(model.TotalCount == 17 && model.OwnedCount == 0, "Initial state");
            Require(model.CatalogContext == "Phase 1 · dungeons and quests", "Real catalog context");
            Require(((ComboBox)root.FindName("CharacterPicker")).SelectedItem is CharacterOption &&
                ((ComboBox)root.FindName("SpecializationPicker")).SelectedItem is Specialization { Id: "holy" }, "Initial picker selection");
            var hat = model.VisibleItems.Single(item => item.Name == "Crimson Felt Hat");
            var neck = model.VisibleItems.Single(item => item.Name == "Animated Chain Necklace");
            var chest = model.VisibleItems.Single(item => item.Name == "Robes of the Exalted");
            var shoulders = model.VisibleItems.Single(item => item.Name == "Burial Shawl");
            var cape = model.VisibleItems.Single(item => item.Name == "Archivist Cape of Healing");
            var cuffs = model.VisibleItems.Single(item => item.Name == "Flameweave Cuffs of Healing");
            Require(model.VisibleItems.All(item => item.ItemUri is not null && item.RecommendationUri is not null), "Source links");
            await model.SetEquippedAsync(hat.Id, true);
            Require(model.OwnedCount == 1 && model.EquippedSummary == "1 / 17", "Equip updates summary");
            await model.SetOwnedAsync(hat.Id, false);
            Require(model.OwnedCount == 0 && model.EquippedSummary == "0 / 17", "Unown clears equipment");

            foreach (var id in new[] { hat.Id, neck.Id, chest.Id }) await model.SetEquippedAsync(id, true);
            foreach (var id in new[] { shoulders.Id, cape.Id, cuffs.Id }) await model.SetOwnedAsync(id, true);
            Require(model.OwnedCount == 6 && model.RemainingCount == 11, "Owned summary");
            model.FilterIndex = 3;
            Require(model.VisibleItems.Count == 3, "Equipped filter");
            model.FilterIndex = 1;
            Require(model.VisibleItems.Count == 11, "Missing filter");
            model.FilterIndex = 0;
            model.SearchText = "mAiN HaNd";
            Require(model.VisibleItems.Count == 1 && model.VisibleItems[0].Name == "The Hammer of Grace", "Case-insensitive search");
            model.SearchText = "Scholomance";
            Require(model.VisibleItems.Count == 1 && model.VisibleItems[0].Id == shoulders.Id, "Acquisition search");
            model.SearchText = "no-matching-item";
            Require(model.VisibleItems.Count == 0 && model.EmptyMessage.Length > 0, "Empty state");
            model.SearchText = "";
            await WaitForAsync(() => IconMatches(root, hat) && IconMatches(root, neck), "Visible remote item icons");
            await SaveImageAsync(root, Path.Combine(directory, "draft-overview.png"));

            // Exercise the actual ImageFailed event and the visible fallback without changing real progress.
            var hatImage = FindImage(root, hat);
            hatImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/missing-verification-icon.png"));
            await WaitForAsync(() => !hat.IsIconLoaded, "Failed image fallback");
            Require(hat.IconPlaceholderVisibility == "Visible", "Offline icon placeholder");
            await SaveImageAsync(root, Path.Combine(directory, "phase1-icon-fallback.png"));
            hatImage.SetBinding(Image.SourceProperty, new Microsoft.UI.Xaml.Data.Binding
            {
                Path = new PropertyPath(nameof(TrackerItemViewModel.IconUrl))
            });
            await WaitForAsync(() => IconMatches(root, hat), "Restored image");

            model.SearchText = "of Healing";
            Require(model.VisibleItems.Count == 3 && model.VisibleItems.All(item => item.Note.Contains("Other variants")), "Suffix conditions");
            await WaitForAsync(() => IconMatches(root, cape) && IconMatches(root, cuffs), "Suffix item icons");
            Require(IconMatches(root, cape) && IconMatches(root, cuffs), "Recycled rows keep correct icons");
            await SaveImageAsync(root, Path.Combine(directory, "phase1-suffix.png"));
            model.SearchText = "Stormrager";
            Require(model.VisibleItems.Count == 1 && model.VisibleItems[0].Note.Contains("Raid quest") &&
                model.VisibleItems[0].Note.Contains("Bonecreeper Stylus"), "Faction quest conditions and alternative");
            var wand = model.VisibleItems[0];
            await WaitForAsync(() => IconMatches(root, wand), "Quest item icon");
            Require(IconMatches(root, wand), "Quest item has correct icon");
            await SaveImageAsync(root, Path.Combine(directory, "phase1-quest.png"));
            var priest = model.SelectedCharacter!;
            var picker = (ComboBox)root.FindName("SpecializationPicker");
            picker.SelectedItem = model.Specializations.Single(spec => spec.Id == "shadow");
            await WaitForAsync(() => model.IsInteractive && model.SelectedSpecialization?.Id == "shadow", "Spec picker event");
            Require(model.TotalCount == 0 && model.EmptyMessage == "No reviewed BiS list available yet.", "Unavailable spec is explicit");
            await SaveImageAsync(root, Path.Combine(directory, "character-shadow.png"));
            picker.SelectedItem = model.Specializations.Single(spec => spec.Id == "holy");
            await WaitForAsync(() => model.IsInteractive && model.SelectedSpecialization?.Id == "holy", "Return to Holy");
            Require(model.OwnedCount == 6 && model.EquippedSummary == "3 / 17" && model.VisibleItems.Count == 17, "Spec switch preserves progress and resets search");
            Workspace.FailNextSave = true;
            await model.SetOwnedAsync(hat.Id, false);
            Require(model.HasError && model.OwnedCount == 6 && model.EquippedSummary == "3 / 17", "Failed save leaves view unchanged");
            Workspace.FailNextSave = true;
            picker.SelectedItem = model.Specializations.Single(spec => spec.Id == "shadow");
            await WaitForAsync(() => model.IsInteractive && model.HasError && model.SelectedSpecialization?.Id == "holy" &&
                picker.SelectedItem is Specialization { Id: "holy" }, "Failed selection restores picker");
            await model.CreateCharacterAsync("Forever Mage", GameVersion.Forever, CharacterClass.Mage);
            Require(model.Characters.Count == 2 && model.Specializations.Count == 3 && model.TotalCount == 0 && model.CatalogLabel.StartsWith("WoW Forever"), "Forever Mage selection");
            picker.SelectedItem = model.Specializations.Single(spec => spec.Id == "fire");
            await WaitForAsync(() => model.IsInteractive && model.SelectedSpecialization?.Id == "fire", "Forever spec selection");
            await SaveImageAsync(root, Path.Combine(directory, "character-forever.png"));
            var reloaded = new TrackerViewModel(new CharacterTrackerService(new CharacterCatalog(), Workspace, new PreviewProgressRepository()));
            await reloaded.LoadAsync();
            Require(reloaded.SelectedCharacter?.Name == "Forever Mage" && reloaded.SelectedSpecialization?.Id == "fire", "Restart restores character and spec");
            var characterPicker = (ComboBox)root.FindName("CharacterPicker");
            characterPicker.SelectedItem = model.Characters.Single(character => character.Id == priest.Id);
            await WaitForAsync(() => model.IsInteractive && model.SelectedCharacter?.Id == priest.Id, "Character picker event");
            Require(model.OwnedCount == 6 && model.EquippedSummary == "3 / 17", "Character switch preserves separate collection");
            var dialog = new CharacterDialog(root.XamlRoot);
            var dialogResult = dialog.ShowAsync();
            await SaveImageAsync(dialog, Path.Combine(directory, "character-create.png"));
            dialog.Hide();
            await dialogResult;
            await File.WriteAllTextAsync(Path.Combine(directory, "ui-checks.txt"),
                "PASS: real 17-item catalog, source links, equip, unown, summaries, filters, search, icons and fallback, suffix/quest conditions, character/spec picker events, separate lists, unavailable catalogs, failed-save rollback, failed-selection rollback, Forever Mage, restored selection after reload, new-character dialog, render. All progress isolated in memory.\n");
        }
        catch (Exception exception)
        {
            await File.WriteAllTextAsync(Path.Combine(directory, "ui-checks.txt"), "FAIL: " + exception);
            Environment.ExitCode = 1;
        }
    }

    private static async Task WaitForAsync(Func<bool> condition, string check)
    {
        for (var attempt = 0; attempt < 50 && !condition(); attempt++) await Task.Delay(200);
        Require(condition(), check);
    }

    private static Image FindImage(DependencyObject parent, TrackerItemViewModel item) =>
        FindImageOrNull(parent, item) ?? throw new InvalidOperationException("Visible item icon was not found.");

    // Filtering updates the visual tree asynchronously; an earlier load flag can remain true.
    private static bool IconMatches(DependencyObject root, TrackerItemViewModel item) =>
        item.IsIconLoaded &&
        FindImageOrNull(root, item)?.Source is BitmapImage { PixelWidth: > 0, PixelHeight: > 0 } bitmap &&
        bitmap.UriSource.AbsoluteUri == item.IconUrl;

    private static Image? FindImageOrNull(DependencyObject parent, TrackerItemViewModel item)
    {
        if (parent is Image image && ReferenceEquals(image.DataContext, item)) return image;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var found = FindImageOrNull(VisualTreeHelper.GetChild(parent, index), item);
            if (found is not null) return found;
        }
        return null;
    }

    private static async Task SaveImageAsync(FrameworkElement root, string filePath)
    {
        root.UpdateLayout();
        await Task.Delay(300);
        var bitmap = new RenderTargetBitmap();
        await bitmap.RenderAsync(root);
        var pixels = await bitmap.GetPixelsAsync();
        Require(bitmap.PixelWidth > 0 && bitmap.PixelHeight > 0 && pixels.Length > 0, "Nonempty render");
        var file = await StorageFile.GetFileFromPathAsync(await CreateEmptyFileAsync(filePath));
        using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
        using var reader = DataReader.FromBuffer(pixels);
        var bytes = new byte[pixels.Length];
        reader.ReadBytes(bytes);
        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
            (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes);
        await encoder.FlushAsync();
    }

    private static async Task<string> CreateEmptyFileAsync(string path)
    {
        await File.WriteAllBytesAsync(path, Array.Empty<byte>());
        return path;
    }

    private static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("UI check failed: " + check);
    }
}

internal sealed class PreviewProgressRepository : IProgressRepository
{
    private ProgressState _state = new(Array.Empty<string>(), new());
    public Task<ProgressState> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(_state);
    public Task SaveAsync(ProgressState state, CancellationToken cancellationToken = default)
    {
        _state = state;
        return Task.CompletedTask;
    }
}

internal sealed class PreviewWorkspaceRepository : IWorkspaceRepository
{
    private string? _json;
    public bool FailNextSave { get; set; }
    public Task<WorkspaceState?> LoadAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_json is null ? null : System.Text.Json.JsonSerializer.Deserialize<WorkspaceState>(_json));
    public Task SaveAsync(WorkspaceState state, CancellationToken cancellationToken = default)
    {
        if (FailNextSave)
        {
            FailNextSave = false;
            throw new IOException("Simulated verification failure.");
        }
        _json = System.Text.Json.JsonSerializer.Serialize(state);
        return Task.CompletedTask;
    }
}
#endif
