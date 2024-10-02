using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Cipher.Types;
using Hermes.Language;
using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Models;
using Hermes.Repositories;
using SukiUI.Toasts;

namespace Hermes.Features.Controls.Token;

public partial class TokenViewModel : ViewModelBase, ITokenViewModel
{
    public ISukiToastManager? ToastManager { get; set; }

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(UnlockCommand))]
    private bool _canUnlock = true;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(UnlockCommand))]
    private string _userName = "";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(UnlockCommand))]
    private string _password = "";

    [ObservableProperty] private bool _isUnlocked;
    [ObservableProperty] private string _watermark = Resources.txt_employee;
    private readonly ISfcRepository _sfcRepository;
    public event EventHandler? Unlocked;

    private DepartmentType _department = DepartmentType.All;

    public DepartmentType Department
    {
        get => _department;
        set
        {
            _department = value;
            this.Watermark = $"{value.ToString().ToUpper()} {Resources.txt_employee.ToLower()}";
        }
    }

    public TokenViewModel(ISfcRepository sfcRepository)
    {
        this._sfcRepository = sfcRepository;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteUnlock))]
    private async Task Unlock()
    {
#if DEBUG
        var user = await this._sfcRepository.FindUser(this.UserName, this.Password);
        var validation = this.Validate(user);
        if (validation != null)
        {
            ShowErrorToast(validation);
            return;
        }
#endif
        this.IsUnlocked = true;
        this.Unlocked?.Invoke(this, EventArgs.Empty);
    }

    private void ShowErrorToast(string message)
    {
        if (ToastManager == null)
        {
            return;
        }

        ToastManager.CreateToast()
            .OfType(NotificationType.Error)
            .WithTitle(Resources.txt_error)
            .WithContent(message)
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Dismiss().ByClicking()
            .Queue();
    }

    private string? Validate(User user)
    {
        if (user.IsNull)
        {
            return Resources.msg_invalid_user_password;
        }

        if (user.Department != this.Department)
        {
            return Resources.msg_invalid_department;
        }

        return null;
    }

    private bool CanExecuteUnlock =>
        this.CanUnlock && !string.IsNullOrEmpty(this.UserName) && !string.IsNullOrEmpty(this.Password);

    public void Reset()
    {
        this.IsUnlocked = false;
    }

    public TokenViewModel Clone()
    {
        return new TokenViewModel(this._sfcRepository)
        {
            ToastManager = this.ToastManager
        };
    }
}