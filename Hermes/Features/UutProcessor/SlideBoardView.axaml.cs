using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
//using Hermes.Features.About;
using Microsoft.Extensions.DependencyInjection;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoardView : UserControl
{
    public SlideBoardView()
    {
        InitializeComponent();
        var serviceProvider = ((App)Application.Current).GetSingleServiceProvider();
        var slideBoardViewModel = serviceProvider.GetService<SlideBoardViewModel>()!;
        this.DataContext = slideBoardViewModel;
    }
}