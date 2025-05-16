using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using System.Collections.ObjectModel;

namespace Hermes.Features.UutProcessor;

public partial class SlideDetailsViewModel : ViewModelBase
{
    private readonly ISukiDialog _dialog;
    public ObservableCollection<SlideModel> SlideModels { get; set; } = new ObservableCollection<SlideModel>();

    public SlideDetailsViewModel(ISukiDialog dialog)
    {
        this._dialog = dialog;
    }

    [RelayCommand]
    private void CloseDialog()
    {
        Dispatcher.UIThread.InvokeAsync(() => _dialog.Dismiss());
    }
}
