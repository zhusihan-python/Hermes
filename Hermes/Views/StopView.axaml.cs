using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Hermes.Views;

public partial class StopView : Window
{
    public bool CanClose { get; set; }

    public StopView()
    {
        InitializeComponent();
        Closing += (_, args) => args.Cancel = !CanClose;
#if DEBUG
        this.Topmost = false;
        this.CanClose = true;
#endif
    }
}