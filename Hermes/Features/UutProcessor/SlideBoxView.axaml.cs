using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoxView : UserControl
{
    private readonly SlideBoxViewModel viewModel;
    public int Row;
    public int Column;
    public SlideBoxView()
    {
        InitializeComponent();
        var serviceProvider = ((App)Application.Current).GetServiceProvider();
        var slideBoxViewModel = serviceProvider.GetService<SlideBoxViewModel>()!;
        this.viewModel = slideBoxViewModel;
        this.DataContext = slideBoxViewModel;
    }

    public void Border_PointerPressed(object sender, PointerPressedEventArgs args)
    {
        viewModel.ExecuteLoadSlides();
        var ctl = sender as Control;
        if (ctl != null)
        {
            FlyoutBase.ShowAttachedFlyout(ctl);
        }
    }
}