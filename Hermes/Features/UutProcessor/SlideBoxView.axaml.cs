using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoxView : UserControl
{
    SlideBoxViewModel viewModel = new SlideBoxViewModel();
    public SlideBoxView()
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}