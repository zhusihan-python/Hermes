using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Features.UutProcessor;

public partial class DummyViewModel : ViewModelBase
{
    [ObservableProperty] private string _statusText = Resources.enum_idle;
    [ObservableProperty] private bool _isWaitingForDummy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(WaitForDummyCommand))]
    [NotifyCanExecuteChangedFor(nameof(this.CancelWaitForDummyCommand))]
    private bool _canChangeStatus;

    private readonly Session _session;

    public DummyViewModel(Session session)
    {
        this._session = session;
        this.IsActive = true;
    }

    protected override void OnActivated()
    {
        this._session.UutProcessorStateChanged += OnUutProcessorStateChanged;
        base.OnActivated();
    }

    private void OnUutProcessorStateChanged(UutProcessorState state)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            CanChangeStatus = state == UutProcessorState.Idle;
            if (state != UutProcessorState.Idle)
            {
                this.IsWaitingForDummy = false;
            }
        });
    }

    partial void OnIsWaitingForDummyChanged(bool oldValue, bool newValue)
    {
        this.StatusText = newValue
            ? Resources.msg_waiting_dummy_board
            : Resources.enum_idle;
    }

    [RelayCommand(CanExecute = nameof(CanChangeStatus))]
    private void WaitForDummy()
    {
        this.IsWaitingForDummy = true;
        Messenger.Send(new WaitForDummyMessage(this.IsWaitingForDummy));
    }

    [RelayCommand(CanExecute = nameof(CanChangeStatus))]
    private void CancelWaitForDummy()
    {
        this.IsWaitingForDummy = false;
        Messenger.Send(new WaitForDummyMessage(this.IsWaitingForDummy));
    }
}