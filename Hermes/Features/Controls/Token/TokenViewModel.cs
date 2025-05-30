using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Cipher.Types;
using Hermes.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
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
    private readonly UserRemoteRepository _userRemoteRepository;
    public event EventHandler? Unlocked;

    private readonly List<DepartmentType> _departments = [DepartmentType.All];
    private readonly Settings _settings;

    public TokenViewModel(
        Settings settings,
        UserRemoteRepository userRemoteRepository)
    {
        this._userRemoteRepository = userRemoteRepository;
        this._settings = settings;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteUnlock))]
    private async Task Unlock()
    {
        await Task.Run(async () =>
        {
            try
            {
                var user = await this._userRemoteRepository.FindUser(this.UserName, this.Password);
                var validation = this.Validate(user);
                if (validation != null)
                {
                    ShowErrorToast(validation);
                    return;
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.UserName = user.Name;
                    this.IsUnlocked = true;
                    this.Password = "";
                    this.User = user;
                    this.Unlocked?.Invoke(this, EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
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
        if (user.Department == DepartmentType.Admin) return null;

        if (user.IsNull)
        {
            return Resources.msg_invalid_user_password;
        }

        if (!this._departments.Contains(DepartmentType.All) && !this._departments.Contains(user.Department))
        {
            return Resources.msg_invalid_department;
        }

        return null;
    }

    public void ClearDepartments()
    {
        this._departments.Clear();
    }

    public void Add(DepartmentType department)
    {
        this._departments.Add(department);
        this.Watermark = $"{string.Join(", ", _departments)} {Resources.txt_employee.ToLower()}";
    }

    private bool CanExecuteUnlock =>
        this.CanUnlock && !string.IsNullOrEmpty(this.UserName) && !string.IsNullOrEmpty(this.Password);

    public User User { get; private set; } = User.Null;

    public void Reset()
    {
        this.IsUnlocked = false;
        this.UserName = "";
        this.Password = "";
    }

    public TokenViewModel Clone()
    {
        return new TokenViewModel(this._settings, this._userRemoteRepository)
        {
            ToastManager = this.ToastManager
        };
    }
}