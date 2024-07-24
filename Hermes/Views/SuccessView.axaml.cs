using Avalonia.Controls;
using Avalonia.Input;

namespace Hermes.Views;

public partial class SuccessView : Window
{
    public SuccessView()
    {
        InitializeComponent();
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        this.Hide();
    }
}