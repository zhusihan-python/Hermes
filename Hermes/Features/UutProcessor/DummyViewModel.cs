using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Models;
using Hermes.Types;
using Reactive.Bindings.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System;

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
    private readonly CompositeDisposable _disposables = [];

    public DummyViewModel(Session session)
    {
        this._session = session;
        this.IsActive = true;
    }

    protected override void OnActivated()
    {
        this.SetupReactiveObservers();
        base.OnActivated();
    }

    private void SetupReactiveObservers()
    {
        var uutProcessorStateChangedDisposable = this._session
            .UutProcessorCurrentState
            .ObserveOn(SynchronizationContext.Current!)
            .Do(uutProcessorState =>
            {
                if (uutProcessorState != UutProcessorState.Idle)
                {
                    this.IsWaitingForDummy = false;
                }
            })
            .Do(uutProcessorState => CanChangeStatus = uutProcessorState == UutProcessorState.Idle)
            .Subscribe();

        this._disposables.Add(uutProcessorStateChangedDisposable);
    }

    protected override void OnDeactivated()
    {
        this._disposables.DisposeItems();
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

    partial void OnIsWaitingForDummyChanged(bool oldValue, bool newValue)
    {
        this.StatusText = newValue
            ? Resources.msg_waiting_dummy_board
            : Resources.enum_idle;
    }
}