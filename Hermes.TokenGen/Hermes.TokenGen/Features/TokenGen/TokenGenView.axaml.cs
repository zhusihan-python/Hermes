using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Hermes.TokenGen.Features.TokenGen;

public partial class TokenGenView : UserControl
{
    public TokenGenView()
    {
        InitializeComponent();
    }

    private void InputElement_OnTapped(object? sender, TappedEventArgs _)
    {
        var tokenTextBox = sender as TextBox;
        var token = tokenTextBox?.Text;
        if (string.IsNullOrWhiteSpace(token) || tokenTextBox == null) return;
        TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(token);
        FlyoutBase.ShowAttachedFlyout(tokenTextBox);
    }
}