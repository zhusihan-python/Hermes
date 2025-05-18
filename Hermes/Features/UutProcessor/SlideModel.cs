using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Features.UutProcessor;
public partial class SlideModel: ObservableObject
{
    [ObservableProperty] public Color backColor = Colors.White;
    [ObservableProperty]
    public string _slideLocation;
    public Slide Slide { get; set; }
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
        this._slideLocation = "0-0";
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
}