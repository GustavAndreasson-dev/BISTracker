using System;
using System.Linq;
using BISTracker.Application;
using BISTracker.Domain;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BISTracker.Presentation.Features.Characters.Views;

public sealed class CharacterDialog : ContentDialog
{
    private sealed record VersionOption(GameVersion Version, string Name);
    private readonly TextBox _name = new() { Header = "Name", MaxLength = 40 };
    private readonly ComboBox _version = new() { Header = "Game version", HorizontalAlignment = HorizontalAlignment.Stretch, DisplayMemberPath = "Name" };
    private readonly ComboBox _class = new() { Header = "Class", HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly InfoBar _error = new() { IsClosable = false, Severity = InfoBarSeverity.Error };

    public CharacterDialog(XamlRoot root)
    {
        XamlRoot = root;
        RequestedTheme = ElementTheme.Dark;
        Title = "New character";
        PrimaryButtonText = "Create";
        CloseButtonText = "Cancel";
        DefaultButton = ContentDialogButton.Primary;
        _version.ItemsSource = new[] { new VersionOption(GameVersion.Classic, "WoW Classic (Vanilla)"), new VersionOption(GameVersion.Forever, "WoW Forever") };
        _version.SelectedIndex = 0;
        _class.ItemsSource = Enum.GetValues<CharacterClass>();
        _class.SelectedItem = CharacterClass.Priest;
        var panel = new StackPanel { Spacing = 16, MinWidth = 320 };
        panel.Children.Add(_name);
        panel.Children.Add(_version);
        panel.Children.Add(_class);
        panel.Children.Add(_error);
        Content = panel;
        PrimaryButtonClick += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(_name.Text) && !Array.Exists(_name.Text.ToCharArray(), char.IsControl)) return;
            args.Cancel = true;
            _error.Message = "Enter a character name.";
            _error.IsOpen = true;
        };
    }

    // Rename mode: only the name is editable; version and class stay locked.
    public CharacterDialog(XamlRoot root, CharacterOption character) : this(root)
    {
        Title = "Rename character";
        PrimaryButtonText = "Save";
        _name.Text = character.Name;
        _version.SelectedItem = ((VersionOption[])_version.ItemsSource).Single(option => option.Version == character.Version);
        _class.SelectedItem = character.Class;
        _version.IsEnabled = false;
        _class.IsEnabled = false;
    }

    public string CharacterName => _name.Text.Trim();
    public GameVersion Version => ((VersionOption)_version.SelectedItem).Version;
    public CharacterClass Class => (CharacterClass)_class.SelectedItem;
}
