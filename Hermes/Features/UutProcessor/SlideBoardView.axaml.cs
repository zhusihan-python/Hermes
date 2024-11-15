using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoardView : UserControl
{
    SlideBoardViewModel viewModel = new SlideBoardViewModel();
    public SlideBoardView()
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}