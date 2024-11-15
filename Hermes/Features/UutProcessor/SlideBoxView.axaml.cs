using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoxView : UserControl
{
    SlideBoxViewModel viewModel = new SlideBoxViewModel();
    public int Row;
    public int Column;
    public SlideBoxView()
    {
        InitializeComponent();
        this.DataContext = viewModel;
        Row = viewModel.RowIndex;
        Column = viewModel.ColumnIndex;
    }
}