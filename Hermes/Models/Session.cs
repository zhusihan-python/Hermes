using System;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Cipher.Types;
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
            this.UserChanged?.Invoke(value);
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
    public DepartmentType UserDepartmentType => _user.Department;
    public UserLevel UserLevel => _user.Level;

    public void ResetStop()
    {
        Stop = Stop.Null;
    }

    public bool HasUserPermission(FeatureType featureType)
    {
        return this.User.HasPermission(featureType);
    }

    public void UpdateUser(User user)
    {
        this.User = user;
    }

    public bool CanUserExit()
    {
        return this.HasUserPermission(FeatureType.Exit);
    }

    public void Logout()
    {
        this.User = User.Null;
    }
}