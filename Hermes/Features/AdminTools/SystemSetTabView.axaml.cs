using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Hermes.Features.Bender;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.Features.AdminTools;

public partial class SystemSetTabView : UserControl
{
    public SystemSetTabViewModel viewModel = new SystemSetTabViewModel();
    public SystemSetTabView()
    {
        InitializeComponent();
        cameras.ItemsSource = new string[]
            {"HKVision", "Nikon" }
        .OrderBy(x => x);
        this.DataContext = viewModel;
    }
}