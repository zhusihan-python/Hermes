using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hermes.Features.UutProcessor;

public partial class SlideDetailsViewModel : ViewModelBase
{
    private readonly ISukiDialog _dialog;
    public ObservableCollection<SlideModel> SlideModels { get; set; } = new ObservableCollection<SlideModel>();

    public SlideDetailsViewModel(
        ISukiDialog dialog,
        ObservableCollection<SlideModel> slideModels
        )
    {
        this._dialog = dialog;
        this.SlideModels = slideModels;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        Dispatcher.UIThread.InvokeAsync(() => _dialog.Dismiss());
    }
}
