using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Cipher.Types;
using Hermes.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
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
    private readonly ISettingsRepository _settingsRepository;


    public TokenViewModel(UserRemoteRepository userRemoteRepository, ISettingsRepository settingsRepository)
    {
        this._userRemoteRepository = userRemoteRepository;
        this._settingsRepository = settingsRepository;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteUnlock))]
    private async Task Unlock()
    {
#if DEBUG
        var user = await this._userRemoteRepository.FindUser(this.UserName, this.Password);
        var validation = this.Validate(user);
        if (validation != null)
        {
            ShowErrorToast(validation);
            return;
        }
#endif
        this.IsUnlocked = true;
        this.UserName = user.Name;
        this.Password = "";
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

        if (
            user.Department != DepartmentType.Admin &&
            !this._departments.Contains(DepartmentType.All) && !this._departments.Contains(user.Department))
        {
            return Resources.msg_invalid_department;
        }

        if (!user.HasPermission(FeatureTypeExtensions.GetFeatureType(_settingsRepository.Settings.Station)))
        {
            return Resources.msg_user_without_permission;
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

    public void Reset()
    {
        this.IsUnlocked = false;
        this.UserName = "";
        this.Password = "";
    }

    public TokenViewModel Clone()
    {
        return new TokenViewModel(this._userRemoteRepository, this._settingsRepository)
        {
            ToastManager = this.ToastManager
        };
    }
}