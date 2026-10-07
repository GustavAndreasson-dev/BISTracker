using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BISTracker.Presentation.Features.Tracking.ViewModels;

namespace BISTracker.Presentation
{
    public sealed partial class MainWindow : Window
    {
        private readonly TrackerViewModel _viewModel;

        public MainWindow(TrackerViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            Root.DataContext = viewModel;
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1240, 900));
            Root.Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Root.Loaded -= OnLoaded;
            await _viewModel.LoadAsync();
#if DRAFT_PREVIEW
            if (Common.DraftPreview.IsRequested)
            {
                await Common.DraftPreview.RunAsync(Root, _viewModel);
                Close();
            }
#endif
        }

        private async void OwnedClicked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox { DataContext: TrackerItemViewModel item } checkBox)
            {
                await _viewModel.SetOwnedAsync(item.Id, checkBox.IsChecked == true);
            }
        }

        private async void EquippedClicked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox { DataContext: TrackerItemViewModel item } checkBox)
            {
                await _viewModel.SetEquippedAsync(item.Id, checkBox.IsChecked == true);
            }
        }

        private void ItemIconOpened(object sender, RoutedEventArgs e)
        {
            if (sender is Image { DataContext: TrackerItemViewModel item }) item.SetIconLoaded(true);
        }

        private void ItemIconFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (sender is Image { DataContext: TrackerItemViewModel item }) item.SetIconLoaded(false);
        }
    }
}
