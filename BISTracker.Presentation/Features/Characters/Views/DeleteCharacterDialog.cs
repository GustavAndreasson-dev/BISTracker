using BISTracker.Application;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BISTracker.Presentation.Features.Characters.Views;

public sealed class DeleteCharacterDialog : ContentDialog
{
    public DeleteCharacterDialog(XamlRoot root, CharacterOption character)
    {
        XamlRoot = root;
        RequestedTheme = ElementTheme.Dark;
        Title = $"Delete {character.DisplayName}?";
        Content = new TextBlock { Text = "This character and all of its progress will be permanently deleted.", TextWrapping = TextWrapping.Wrap, MaxWidth = 360 };
        PrimaryButtonText = "Delete";
        CloseButtonText = "Cancel";
        DefaultButton = ContentDialogButton.Close;
    }
}
