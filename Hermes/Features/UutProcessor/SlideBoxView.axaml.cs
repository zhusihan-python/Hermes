using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoxView : UserControl
{
    SlideBoxViewModel? ViewModel => DataContext as SlideBoxViewModel;
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

    void SlideBox_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (ViewModel is null)
            return;

        if (ViewModel.BoxInPlace)
        {
            ViewModel.IsSelected = !ViewModel.IsSelected;
        }
    }
}