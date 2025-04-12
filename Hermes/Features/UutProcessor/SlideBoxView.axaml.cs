using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoxView : UserControl
{
    public int Row;
    public int Column;
    public SlideBoxView()
    {
        InitializeComponent();
    }

    public void Border_PointerPressed(object sender, PointerPressedEventArgs args)
    {
        //viewModel.ExecuteLoadSlides();
        var ctl = sender as Control;
        if (ctl != null)
        {
            FlyoutBase.ShowAttachedFlyout(ctl);
        }
    }
}