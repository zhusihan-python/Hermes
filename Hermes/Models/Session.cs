using System;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Types;

namespace Hermes.Models;

public partial class Session : ObservableObject
{
    private User User
    {
        get => _user;
        set
        {
            _user = value;
            this.UserChanged?.Invoke(User.Null);
        }
    }

    [ObservableProperty] private string _path = string.Empty;
    public Stop Stop { get; set; } = Stop.Null;

    public UutProcessorState UutProcessorState
    {
        get => _uutProcessorState;
        set
        {
            lock (_lock)
            {
                _uutProcessorState = value;
            }

            UutProcessorStateChanged?.Invoke(value);
        }
    }

    public event Action<User>? UserChanged;
    public event Action<UutProcessorState>? UutProcessorStateChanged;

    private readonly object _lock = new object();
    private UutProcessorState _uutProcessorState;
    private User _user = User.Null;

    public bool IsUutProcessorIdle => UutProcessorState == UutProcessorState.Idle;
    public bool IsUutProcessorBlocked => UutProcessorState == UutProcessorState.Blocked;
    public bool IsLoggedIn => !_user.IsNull;

    public void ResetStop()
    {
        Stop = Stop.Null;
    }

    public bool CanUserView(int level)
    {
        return this.User.ViewLevel >= level;
    }

    public bool CanUserUpdate(int level)
    {
        return this.User.UpdateLevel >= level;
    }

    public void UpdateUser(User user)
    {
        this.User = user;
    }

    public bool CanUserExit()
    {
        return this.User.CanExit;
    }

    public void Logout()
    {
        this.User = User.Null;
    }
}