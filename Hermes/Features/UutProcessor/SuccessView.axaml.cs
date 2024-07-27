using Avalonia.Controls;
using Avalonia.Input;

namespace Hermes.Features.UutProcessor;

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