using Avalonia;
using Avalonia.Controls;

namespace Hermes.Features.AdminTools;

public partial class SystemAlarmTabView : UserControl
{
    public SystemAlarmTabViewModel viewModel = new SystemAlarmTabViewModel();
    public SystemAlarmTabView()
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}