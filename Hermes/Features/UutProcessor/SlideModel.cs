using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Models;
using Hermes.Types;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Hermes.Features.UutProcessor;
public partial class SlideModel : ViewModelBase
{
    [ObservableProperty] private int _rowIndex = 0;
    [ObservableProperty] private bool _isSelected = false;
    [ObservableProperty] public Color backColor = Colors.White;

    private Slide Slide { get; set; }
    private SlideState _state;
    public SlideState State
    {
        get => _state;
        set
        {
            if (SetProperty(ref _state, value))
            {
                // 根据 SlideState 更新 StatusText 和 BackColor
                UpdateStatusAndColor();
            }
        }
    }
    public SlideModel()
    {
        this.IsActive = true;
    }

    public SlideModel(int SlideId)
    {
        //Slide.Initialize(SlideId);
    }

    // 根据 SlideState 更新状态文本和背景颜色
    private void UpdateStatusAndColor()
    {
        switch (State)
        {
            case SlideState.Empty:
                BackColor = Colors.White;
                break;
            case SlideState.NotSorted:
                BackColor = Colors.Cyan;
                break;
            case SlideState.Sorted:
                BackColor = Colors.Green;
                break;
            case SlideState.NotRecognized:
            case SlideState.SlideBlocked:
                BackColor = Colors.Red;
                break;
            default:
                BackColor = Colors.Transparent;
                break;
        }
    }

    protected override void SetupReactiveExtensions()
    {

    }
}