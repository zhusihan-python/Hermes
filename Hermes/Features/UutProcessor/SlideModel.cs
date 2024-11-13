using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Models;
using Hermes.Types;
using R3;

namespace Hermes.Features.UutProcessor;
public partial class SlideModel : ViewModelBase
{
    [ObservableProperty] private string _statusText = Resources.enum_empty;
    [ObservableProperty] public Color backColor = Colors.White;
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

    // 根据 SlideState 更新状态文本和背景颜色
    private void UpdateStatusAndColor()
    {
        switch (State)
        {
            case SlideState.Empty:
                StatusText = Resources.enum_empty;
                BackColor = Colors.White;
                break;
            case SlideState.NotSorted:
                StatusText = Resources.enum_not_sorted;
                BackColor = Colors.Cyan;
                break;
            case SlideState.Sorted:
                StatusText = Resources.enum_sorted;
                BackColor = Colors.Green;
                break;
            case SlideState.Recognized:
                StatusText = Resources.enum_recognized;
                BackColor = Colors.Blue;
                break;
            case SlideState.NotRecognized:
                StatusText = Resources.enum_not_recognized;
                BackColor = Colors.Yellow;
                break;
            case SlideState.SlideBlocked:
                StatusText = Resources.enum_slide_blocked;
                BackColor = Colors.Red;
                break;
            default:
                StatusText = Resources.enum_empty;
                BackColor = Colors.Transparent;
                break;
        }
    }

    protected override void SetupReactiveExtensions()
    {

    }
}