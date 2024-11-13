using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using DynamicData;
using Hermes.Types;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoxViewModel : ViewModelBase
{
    public List<SlideState> SlideStates { get; }
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



