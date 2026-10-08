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
    private static string ImportedCatalogDirectory => Path.Combine(Path.GetFullPath(Arguments[FlagIndex + 1]), "verification-catalog-imports");
    public static CharacterCatalog Catalog { get; } = new(IsRequested ? ImportedCatalogDirectory : null);
    public static ICatalogPackImporter Importer => new CatalogPackImporter(ImportedCatalogDirectory);

    public static async Task RunAsync(FrameworkElement root, TrackerViewModel model)
    {
        var directory = Path.GetFullPath(Arguments[FlagIndex + 1]);
        Directory.CreateDirectory(directory);
        try
        {
            Require(model.TotalCount == 17 && model.OwnedCount == 0, "Initial state");
            Require(model.CatalogContext == "Phase 1 · pre-raid", "Real catalog context");
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
            Require(model.Characters.Count == 2 && model.Specializations.Count == 3 && model.TotalCount > 0 && model.CatalogLabel.StartsWith("WoW Forever") &&
                model.SelectedCatalogSet?.LevelCap == 30 && model.CatalogContext.Contains("beta", StringComparison.OrdinalIgnoreCase), "Forever Mage level 30 selection");
            picker.SelectedItem = model.Specializations.Single(spec => spec.Id == "fire");
            await WaitForAsync(() => model.IsInteractive && model.SelectedSpecialization?.Id == "fire", "Forever spec selection");
            Require(model.VisibleItems.All(item => item.ItemUri?.AbsoluteUri.Contains("/forever/", StringComparison.Ordinal) == true), "Forever item links");
            var mageHead = model.VisibleItems.First(item => item.SlotName == "Head");
            model.VisibleSlots.Single(slot => slot.Slot == EquipmentSlot.Head).IsExpanded = true;
            await model.SetEquippedAsync(mageHead.Id, true);
            await WaitForAsync(() => IconMatches(root, mageHead), "Forever icon");
            Require(model.VisibleSlots.Count == 17 && model.TotalCount == 17 && model.OwnedCount == 1, "Mage alternatives count as slot goals");
            await SaveImageAsync(root, Path.Combine(directory, "slot-alternatives.png"));
            model.VisibleSlots.Single(slot => slot.Slot == EquipmentSlot.Head).IsExpanded = false;
            await SaveImageAsync(root, Path.Combine(directory, "character-forever.png"));
            var catalogPicker = (ComboBox)root.FindName("CatalogPicker");
            catalogPicker.SelectedItem = model.CatalogSets.Single(set => set.LevelCap == 60);
            await WaitForAsync(() => model.IsInteractive && model.SelectedCatalogSet?.LevelCap == 60, "Level picker event");
            Require(model.TotalCount == 0 && model.EmptyMessage == "No reviewed BiS list available yet.", "Level 60 pending catalog");
            await SaveImageAsync(root, Path.Combine(directory, "forever-level60-pending.png"));
            catalogPicker.SelectedItem = model.CatalogSets.Single(set => set.LevelCap == 30);
            await WaitForAsync(() => model.IsInteractive && model.SelectedCatalogSet?.LevelCap == 30, "Return to beta catalog");
            Require(model.VisibleItems.Single(item => item.Id == mageHead.Id).IsEquipped, "Level switch preserves equipment");
            Workspace.FailNextSave = true;
            catalogPicker.SelectedItem = model.CatalogSets.Single(set => set.LevelCap == 60);
            await WaitForAsync(() => model.IsInteractive && model.HasError && model.SelectedCatalogSet?.LevelCap == 30 &&
                catalogPicker.SelectedItem is CatalogSet { LevelCap: 30 }, "Failed level change restores picker");
            catalogPicker.SelectedItem = model.CatalogSets.Single(set => set.LevelCap == 60);
            await WaitForAsync(() => model.IsInteractive && model.SelectedCatalogSet?.LevelCap == 60, "Select pending import context");
            var thirtyCatalog = await Catalog.LoadAsync(GameVersion.Forever, CharacterClass.Mage, "fire");
            var physicalHead = thirtyCatalog.Items.First(item => item.Slot == EquipmentSlot.Head);
            var fixtureDirectory = Path.Combine(directory, "test-only-pack-source-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(fixtureDirectory);
            var fixture = new
            {
                schemaVersion = 1, catalogId = "forever-launch-level60-mage-fire", gameVersion = "Forever", characterClass = "Mage",
                specializationId = "fire", levelCap = 60, releaseStage = "Launch", phase = "TEST ONLY import context",
                sourceUrl = "https://example.com/test-only-import-fixture", reviewedOn = "2026-10-08",
                selectionMethod = "TEST ONLY: UI import regression, not a level 60 recommendation", status = "Partial",
                items = new[] { new { id = "test-only-imported-head", slot = "Head", itemId = physicalHead.Details!.ClassicItemId,
                    name = "TEST ONLY import option", requiredSuffix = physicalHead.Details.RequiredSuffix, requiredLevel = 0,
                    acquisitionType = "Dungeon", source = "TEST ONLY fixture", note = "TEST ONLY: no level 60 recommendation is implied.",
                    iconUrl = physicalHead.Details.IconUrl, itemUrl = physicalHead.Details.ItemUrl, recommendationUrl = "https://example.com/test-only-import-fixture",
                    weaponKind = "None", uniqueEquipped = false } }
            };
            await File.WriteAllTextAsync(Path.Combine(fixtureDirectory, "TEST-ONLY-mage-fire-60.json"), System.Text.Json.JsonSerializer.Serialize(fixture));
            await model.ImportCatalogAsync(fixtureDirectory);
            Require(!model.HasError && model.TotalCount == 17 && model.VisibleSlots.Count == 17 && model.HasCatalogGaps && model.VisibleItems[0].Name == "TEST ONLY import option" &&
                model.VisibleItems[0].IsOwned, "Import refreshes an already selected empty context and retains shared inventory");
            var importedFixturePath = Directory.GetFiles(ImportedCatalogDirectory, "*.json", SearchOption.AllDirectories).Single();
            var updatedFixture = System.Text.Json.Nodes.JsonNode.Parse(await File.ReadAllTextAsync(importedFixturePath))!;
            updatedFixture["items"]![0]!["name"] = "TEST ONLY changed metadata";
            await File.WriteAllTextAsync(importedFixturePath, updatedFixture.ToJsonString());
            await model.LoadAsync();
            Require(model.VisibleItems.Single().Name == "TEST ONLY changed metadata" && model.HasCatalogGaps,
                "Same recommendation IDs refresh changed metadata and slot plan");
            catalogPicker.SelectedItem = model.CatalogSets.Single(set => set.LevelCap == 30);
            await WaitForAsync(() => model.IsInteractive && model.SelectedCatalogSet?.LevelCap == 30, "Restore beta after import regression");
            Require(model.VisibleItems.Single(item => item.Id == mageHead.Id).IsEquipped, "Import preserves beta equipment");
            var reloaded = new TrackerViewModel(new CharacterTrackerService(Catalog, Workspace, new PreviewProgressRepository()));
            await reloaded.LoadAsync();
            Require(reloaded.SelectedCharacter?.Name == "Forever Mage" && reloaded.SelectedSpecialization?.Id == "fire" && reloaded.SelectedCatalogSet?.LevelCap == 30 &&
                reloaded.VisibleItems.Single(item => item.Id == mageHead.Id).IsEquipped, "Restart restores character, spec, level and equipment");
            await model.CreateCharacterAsync("Forever Rogue", GameVersion.Forever, CharacterClass.Rogue);
            picker.SelectedItem = model.Specializations.Single(spec => spec.Id == "combat");
            await WaitForAsync(() => model.IsInteractive && model.SelectedSpecialization?.Id == "combat", "Rogue combat selector");
            Require(model.TotalCount == 17 && model.VisibleSlots.Count == 17 && model.VisibleSlots.Select(slot => slot.Slot).Distinct().Count() == 17,
                "Rogue has 17 unique slot rows, not an alternative-row count");
            var rogueAlternatives = model.VisibleSlots.First(slot => slot.Alternatives.Length > 1 && slot.Slot is not
                (EquipmentSlot.MainHand or EquipmentSlot.OffHand or EquipmentSlot.Finger1 or EquipmentSlot.Finger2 or EquipmentSlot.Trinket1 or EquipmentSlot.Trinket2));
            await model.SetOwnedAsync(rogueAlternatives.Alternatives[0].Id, true);
            await model.SetOwnedAsync(rogueAlternatives.Alternatives[1].Id, true);
            Require(model.OwnedCount == 1 && model.RemainingCount == 16, "Owning two Rogue alternatives covers one slot");
            model.SearchText = rogueAlternatives.SlotName;
            Require(model.VisibleSlots.Count == 1 && model.VisibleItems.Count == rogueAlternatives.Alternatives.Length, "Slot search retains grouped alternatives");
            await SaveImageAsync(root, Path.Combine(directory, "rogue-slot-alternatives.png"));
            model.SearchText = "";
            rogueAlternatives.IsExpanded = false;
            await SaveImageAsync(root, Path.Combine(directory, "rogue-slot-overview.png"));
            await model.CreateCharacterAsync("Forever Hunter", GameVersion.Forever, CharacterClass.Hunter);
            var hunterCatalog = await Catalog.LoadAsync(GameVersion.Forever, CharacterClass.Hunter, model.SelectedSpecialization!.Id);
            var hunterTwoHand = hunterCatalog.Items.First(item => item.Slot == EquipmentSlot.MainHand && item.Details?.WeaponKind == WeaponKind.TwoHanded);
            await model.SetOwnedAsync(hunterTwoHand.Id, true);
            Require(model.OwnedCount == 1 && model.TotalCount == 16 && model.VisibleSlots.All(slot => slot.Slot != EquipmentSlot.OffHand),
                "Owned two-hand setup uses 16 consistent slot goals before equipping");
            var hunterOneHand = hunterCatalog.Items.First(item => item.Slot == EquipmentSlot.MainHand && item.Details?.WeaponKind == WeaponKind.OneHanded);
            await model.SetEquippedAsync(hunterOneHand.Id, true);
            Require(model.TotalCount == 17 && model.VisibleSlots.Any(slot => slot.Slot == EquipmentSlot.OffHand),
                "Equipped one-hand setup retains offhand even when a two-hand alternative is owned");
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
                "PASS: Classic 17-slot catalog, tracking, grouped slot filters/search, icons/fallback, suffix/quest conditions, character/spec picker events, failed-save/selection rollback; real Forever level30 Mage and Rogue Combat each show 17 unique slot goals, owning two alternatives covers only one slot, grouped alternatives survive search; Hunter owned two-hand setup shows16 before equipping, equipped one-hand retains17 despite owned two-hand alternative; level30/60 picker events, preserved beta equipment, failed-level rollback, restored character/spec/level/equipment after reload, pending60 data, new-character dialog, render, same-context import refresh with TEST ONLY sparse level60 fixture that remains visibly incomplete, metadata refresh with identical recommendation IDs. All progress isolated in memory; imported fixture exists only in this temporary verification directory and is not product data.\n");
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
