using Avalonia.Controls;

namespace Hermes.Features.Logs;

public partial class UnitUnderTestLogView : UserControl
{
    public UnitUnderTestLogView()
    {
        InitializeComponent();
        this.DataContext = new UnitUnderTestLogViewModel();
    }
}