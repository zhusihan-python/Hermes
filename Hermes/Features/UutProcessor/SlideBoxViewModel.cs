using System;
using CommunityToolkit.Mvvm.ComponentModel;
//using System.Linq;
using System.Threading.Tasks;
using Hermes.Types;
using Hermes.Repositories;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Hermes.Models;

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

    [ObservableProperty] private bool _boxInPlace;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private SlideBoxActionType _actionType;
    [ObservableProperty] private bool _isBusy;
    public ObservableCollection<SlideModel> ItemList { get; set; } = new ObservableCollection<SlideModel>();
    //public RangeObservableCollection<Slide> Slides { get; set; } = [];
    //private readonly SlideRepository _slideRepository;
    public SlideBoxViewModel(SlideRepository slideRepository)
    {
        for (int i = 0; i < 20; i++)
        {
            ItemList.Add(new SlideModel
            {
                State = SlideState.Empty
            });
        }
    }

    protected override void SetupReactiveExtensions()
    {

    }

    public void Refresh()
    {
        foreach (var model in ItemList)
        {
            model.Slide = Slide.Null;
        }
    }

    [RelayCommand]
    private async Task Search()
    {

    }

    public void UpdateSlide(int slideIndex, Slide slide)
    {
        if (slideIndex < 0 ||  slideIndex >= ItemList.Count)
        {
            return;
        }
        ItemList[slideIndex].Slide = slide;
    }

    public void UpdateSlideLocation()
    {
        var startChar = 65;
        var boxLocation = $"{(char)(RowIndex + startChar)}{ColumnIndex+1}";
        for (int i = 0; i < ItemList.Count; i++)
        {
            ItemList[i].SlideLocation = boxLocation + $"-{i+1}";
        }
    }
}
