using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R3;
using ObservableCollections;
using Hermes.Repositories;
using System.Collections.ObjectModel;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoardViewModel : ViewModelBase
{
    private ObservableCollection<SlideBoxViewModel> _slideBoxes;
    public ObservableCollection<SlideBoxViewModel> SlideBoxes
    {
        get { return _slideBoxes; }
        set { SetProperty(ref _slideBoxes, value); }
    }
    private int _rowCount = 5;
    public int RowCount
    {
        get => _rowCount;
    }
    private int _columnCount = 15;
    public int ColumnCount
    {
        get => _columnCount;
    }

    public SlideBoardViewModel(SlideRepository slideRepository)
    {
        SlideBoxes = new ObservableCollection<SlideBoxViewModel>();

        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                SlideBoxes.Add(new SlideBoxViewModel(slideRepository) { RowIndex = i, ColumnIndex = j });
            }
        }
        Console.WriteLine("SlideBoxes", SlideBoxes);
    }

    protected override void SetupReactiveExtensions()
    {

    }
}

