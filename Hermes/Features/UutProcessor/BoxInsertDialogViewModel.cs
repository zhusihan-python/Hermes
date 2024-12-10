using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Hermes.Language;
using Hermes.Models;
using SukiUI.Dialogs;
using System;

namespace Hermes.Features.UutProcessor;

public partial class BoxInsertDialogViewModel: ViewModelBase
{
    //public event Action<User>? Accepted;
    private readonly ISukiDialog _dialog;
    public BoxInsertDialogViewModel(ISukiDialog dialog)
    {
        this._dialog = dialog;
    }

    [RelayCommand]
    private void Accept()
    {
        this.ShowSuccessToast("流程开始");
    }


    [RelayCommand]
    public void CloseDialog()
    {
        Dispatcher.UIThread.InvokeAsync(() => _dialog.Dismiss());
    }
}
