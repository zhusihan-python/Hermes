using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Hermes.TokenGen.Views;

public partial class MainView : UserControl
{
    public MainView()
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