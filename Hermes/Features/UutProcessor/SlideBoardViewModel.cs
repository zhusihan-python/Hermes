using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R3;
using ObservableCollections;
using Hermes.Repositories;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoardViewModel : ViewModelBase
{
    private ObservableList<SlideBoxViewModel> _slideBoxList;
    public ObservableList<SlideBoxViewModel> SlideBoxList
    {
        get { return _slideBoxList; }
        set { SetProperty(ref _slideBoxList, value); }
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
        _slideBoxList = new ObservableList<SlideBoxViewModel>();

        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                _slideBoxList.Add(new SlideBoxViewModel(slideRepository) { RowIndex = i, ColumnIndex = j });
            }
        }
        Console.WriteLine("SlideBoxList", SlideBoxList);
    }

    protected override void SetupReactiveExtensions()
    {

    }
}

