using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BISTracker.Presentation.Features.Tracking.ViewModels;
using BISTracker.Presentation.Features.Characters.Views;
using BISTracker.Application;
using BISTracker.Domain;

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
            viewModel.PropertyChanged += (_, change) =>
            {
                // Collection refreshes can clear ComboBox selection even when the model value is unchanged.
                if (change.PropertyName == nameof(TrackerViewModel.SelectedCharacter)) CharacterPicker.SelectedItem = viewModel.SelectedCharacter;
                if (change.PropertyName == nameof(TrackerViewModel.SelectedSpecialization)) SpecializationPicker.SelectedItem = viewModel.SelectedSpecialization;
                if (change.PropertyName == nameof(TrackerViewModel.SelectedCatalogSet)) CatalogPicker.SelectedItem = viewModel.SelectedCatalogSet;
            };
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

        private async void CharacterChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.IsInteractive == true && CharacterPicker.SelectedItem is CharacterOption character && character.Id != _viewModel.SelectedCharacter?.Id)
            {
                await _viewModel.SelectCharacterAsync(character);
                CharacterPicker.SelectedItem = _viewModel.SelectedCharacter;
            }
        }

        private async void SpecializationChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.IsInteractive == true && SpecializationPicker.SelectedItem is Specialization spec && spec.Id != _viewModel.SelectedSpecialization?.Id)
            {
                await _viewModel.SelectSpecializationAsync(spec);
                // A rejected save leaves the model's selected value unchanged; reset the control explicitly.
                SpecializationPicker.SelectedItem = _viewModel.SelectedSpecialization;
            }
        }

        private async void NewCharacterClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new CharacterDialog(Root.XamlRoot);
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                await _viewModel.CreateCharacterAsync(dialog.CharacterName, dialog.Version, dialog.Class);
        }

        private async void RenameCharacterClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedCharacter is not { } character) return;
            var dialog = new CharacterDialog(Root.XamlRoot, character);
            if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.CharacterName != character.Name)
                await _viewModel.RenameCharacterAsync(character, dialog.CharacterName);
            // A rejected save leaves the model unchanged; reset the picker to the model's character.
            CharacterPicker.SelectedItem = _viewModel.SelectedCharacter;
        }

        private async void DeleteCharacterClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedCharacter is not { } character) return;
            if (await new DeleteCharacterDialog(Root.XamlRoot, character).ShowAsync() == ContentDialogResult.Primary)
                await _viewModel.DeleteCharacterAsync(character);
            CharacterPicker.SelectedItem = _viewModel.SelectedCharacter;
        }

        private async void CatalogChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.IsInteractive == true && CatalogPicker.SelectedItem is CatalogSet set && set.Id != _viewModel.SelectedCatalogSet?.Id)
            {
                await _viewModel.SelectCatalogAsync(set);
                CatalogPicker.SelectedItem = _viewModel.SelectedCatalogSet;
            }
        }

        private async void ImportCatalogClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new Microsoft.Windows.Storage.Pickers.FolderPicker(AppWindow.Id);
                var folder = await picker.PickSingleFolderAsync();
                if (folder is not null) await _viewModel.ImportCatalogAsync(folder.Path);
            }
            catch (Exception exception)
            {
                var dialog = new ContentDialog { XamlRoot = Root.XamlRoot, RequestedTheme = ElementTheme.Dark,
                    Title = "Could not open catalog folder", Content = exception.Message, CloseButtonText = "Close" };
                await dialog.ShowAsync();
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
