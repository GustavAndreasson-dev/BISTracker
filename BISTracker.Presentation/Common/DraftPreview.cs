#if DRAFT_PREVIEW
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BISTracker.Application;
using BISTracker.Presentation.Features.Tracking.ViewModels;
using Microsoft.UI.Xaml;
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

    public static async Task RunAsync(FrameworkElement root, TrackerViewModel model)
    {
        var directory = Path.GetFullPath(Arguments[FlagIndex + 1]);
        Directory.CreateDirectory(directory);
        try
        {
            Require(model.TotalCount == 17 && model.OwnedCount == 0, "Initial state");
            await model.SetEquippedAsync("demo-head", true);
            Require(model.OwnedCount == 1 && model.EquippedSummary == "1 / 17", "Equip updates summary");
            await model.SetOwnedAsync("demo-head", false);
            Require(model.OwnedCount == 0 && model.EquippedSummary == "0 / 17", "Unown clears equipment");

            foreach (var id in new[] { "demo-head", "demo-neck", "demo-chest" }) await model.SetEquippedAsync(id, true);
            foreach (var id in new[] { "demo-shoulder", "demo-back", "demo-wrist" }) await model.SetOwnedAsync(id, true);
            Require(model.OwnedCount == 6 && model.RemainingCount == 11, "Owned summary");
            model.FilterIndex = 3;
            Require(model.VisibleItems.Count == 3, "Equipped filter");
            model.FilterIndex = 1;
            Require(model.VisibleItems.Count == 11, "Missing filter");
            model.FilterIndex = 0;
            model.SearchText = "mAiN HaNd";
            Require(model.VisibleItems.Count == 1 && model.VisibleItems[0].Id == "demo-mainhand", "Case-insensitive search");
            model.SearchText = "no-matching-item";
            Require(model.VisibleItems.Count == 0 && model.EmptyMessage.Length > 0, "Empty state");
            model.SearchText = "";
            await SaveImageAsync(root, Path.Combine(directory, "draft-overview.png"));
            await File.WriteAllTextAsync(Path.Combine(directory, "ui-checks.txt"),
                "PASS: initial state, equip, unown, summaries, equipped/missing filters, search, empty state, render.\n");
        }
        catch (Exception exception)
        {
            await File.WriteAllTextAsync(Path.Combine(directory, "ui-checks.txt"), "FAIL: " + exception);
            Environment.ExitCode = 1;
        }
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
#endif
