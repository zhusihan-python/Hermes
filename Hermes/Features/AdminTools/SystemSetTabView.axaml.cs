using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;

namespace Hermes.Features.AdminTools;

public partial class SystemSetTabView : UserControl
{
    public SystemSetTabView()
    {
        InitializeComponent();
        cameras.ItemsSource = new string[]
            {"HKVision", "Nikon" }
        .OrderBy(x => x);
    }
}