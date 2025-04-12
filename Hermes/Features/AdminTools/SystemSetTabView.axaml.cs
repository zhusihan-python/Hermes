using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Hermes.Features.AdminTools;

public partial class SystemSetTabView : UserControl
{
    public SystemSetTabView()
    {
        InitializeComponent();
        var serviceProvider = ((App)Application.Current).GetSingleServiceProvider();
        var viewModel = serviceProvider.GetService<SystemSetTabViewModel>()!;
        this.DataContext = viewModel;
        cameras.ItemsSource = new string[]
            {"HKVision", "Nikon" }
        .OrderBy(x => x);
    }
}