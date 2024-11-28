using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Hermes.Features.Bender;

public partial class OtherTabView : UserControl
{
    OtherTabViewModel viewModel = new OtherTabViewModel();
    public OtherTabView()
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}