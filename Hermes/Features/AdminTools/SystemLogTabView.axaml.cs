using Avalonia.Controls;

namespace Hermes.Features.AdminTools;

public partial class SystemLogTabView : UserControl
{
    public SystemLogTabViewModel viewModel = new SystemLogTabViewModel();
    public SystemLogTabView()
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}