using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace Hermes.Features.UutProcessor;

public partial class BoxSelectDialogView : UserControl
{
    private readonly BoxSelectDialogViewModel _viewModel;
    public BoxSelectDialogView()
    {
        InitializeComponent();
        _viewModel = new BoxSelectDialogViewModel();
        DataContext = _viewModel;
    }
}