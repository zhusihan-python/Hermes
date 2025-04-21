using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hermes.Features.UutProcessor;

public partial class BoxSelectDialogViewModel : ViewModelBase
{
    private readonly ISukiDialog _dialog;
    public ObservableCollection<Crockery> CrockeryList { get; set; }
    public BoxSelectDialogViewModel()
    {
        //this._dialog = dialog;
        CrockeryList = new ObservableCollection<Crockery>(new List<Crockery>
            {
                new Crockery("1行-1列", 12),
                new Crockery("1行-2列", 12),
                new Crockery("1行-3列", 6),
                new Crockery("1行-4列", 10),
                new Crockery("1行-5列", 10),
                new Crockery("1行-6列", 6),
                new Crockery("1行-7列", 1)
            });
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

public class Crockery
{
    public string Title { get; set; }
    public int Number { get; set; }

    public Crockery(string title, int number)
    {
        Title = title;
        Number = number;
    }
}
