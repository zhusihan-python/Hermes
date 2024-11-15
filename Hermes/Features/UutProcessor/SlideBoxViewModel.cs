using System.Collections.Generic;
using Hermes.Types;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoxViewModel : ViewModelBase
{
    private int _rowIndex = 1;
    private int _columnIndex = 1;
    public int RowIndex 
    { 
        get { return _rowIndex; }
        set { SetProperty(ref _rowIndex, value); } 
    }
    public int ColumnIndex
    { 
        get { return _columnIndex; }
        set { SetProperty(ref _columnIndex, value); }
    }
    public List<SlideModel> ItemList { get; set; } = new List<SlideModel>();
    public SlideBoxViewModel()
    {
        ItemList.Add(new SlideModel { State = SlideState.Empty });
        ItemList.Add(new SlideModel { State = SlideState.NotSorted });
        ItemList.Add(new SlideModel { State = SlideState.Sorted });
        ItemList.Add(new SlideModel { State = SlideState.Recognized });
        ItemList.Add(new SlideModel { State = SlideState.NotRecognized });
        ItemList.Add(new SlideModel { State = SlideState.SlideBlocked });
        ItemList.Add(new SlideModel { State = SlideState.NotSorted });
        ItemList.Add(new SlideModel { State = SlideState.Sorted });
        ItemList.Add(new SlideModel { State = SlideState.Recognized });
        ItemList.Add(new SlideModel { State = SlideState.NotRecognized });
    }
}



