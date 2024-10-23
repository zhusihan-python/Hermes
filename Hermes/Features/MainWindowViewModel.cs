using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Hermes.Cipher.Types;
using Hermes.Common.Messages;
using Hermes.Features.Login;
using Hermes.Language;
using Hermes.Models;
using Hermes.Types;
using R3;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using SukiUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hermes.Features
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NavigateMessage>
    {
        public List<PageBase> Pages { get; }
        public ISukiToastManager ToastManager { get; }
        public ISukiDialogManager DialogManager { get; }

        public ObservableCollection<PageBase> ShownPages { get; set; } = [];
        [ObservableProperty] private ThemeVariant? _baseTheme;
        [ObservableProperty] private string _baseThemeText = "";
        [ObservableProperty] private string _title;
        [ObservableProperty] private bool _titleBarVisible;
        [ObservableProperty] private PageBase? _activePage;
        [ObservableProperty] private bool _areSettingsVisible;
        [ObservableProperty] private bool _canExit;
        [ObservableProperty] private bool _isLoggedIn;

        private readonly Session _session;
        private readonly Settings _settings;
        private readonly SukiTheme _theme;

        public MainWindowViewModel(
            IEnumerable<PageBase> pages,
            ISukiDialogManager dialogManager,
            ISukiToastManager toastManager,
            Session session,
            Settings settings)
        {
            this._settings = settings;
            this._session = session;
            this._theme = SukiTheme.GetInstance();
            this._theme.ChangeBaseTheme(ThemeVariant.Light);
            this.Pages = pages.ToList();
            this.TitleBarVisible = false;
            this.ToastManager = toastManager;
            this.DialogManager = dialogManager;
            this.ConfigurePages();
            this.UpdateBaseTheme();
            this.UpdateTitle(this._session.LoggedUser.Value);
            this.IsActive = true;
            if (settings.AutostartUutProcessor)
            {
                Messenger.Send(new StartUutProcessorMessage());
            }
        }

        protected override void SetupReactiveExtensions()
        {
            var userChangedDisposable = this._session
                .LoggedUser
                .Do(this.UpdateTitle)
                .Do(_ => this.ConfigurePages())
                .Subscribe()
                .AddTo(ref Disposables);
        }

        private void UpdateTitle(User user)
        {
            var baseTitle = $"{Resources.txt_hermes} - {_settings.Station} - {_settings.Line}";
            if (!user.IsNull)
            {
                Title = $"{baseTitle}     (👤{user.Name})";
            }
            else
            {
                Title = baseTitle;
            }
        }

        private void ConfigurePages()
        {
            var visiblePages = Pages
                .Where(x =>
                    this._session.UserDepartment == DepartmentType.Admin ||
                    x.PermissionType == PermissionType.FreeAccess ||
                    this._session.HasUserPermission(x.PermissionType))
                .Where(x =>
                    this._session.UserDepartment == DepartmentType.Admin ||
                    x.StationFilter == null ||
                    x.StationFilter.Contains(_settings.Station))
                .OrderBy(x => x.Index)
                .ThenBy(x => x.DisplayName)
                .ToList();
            this.ShownPages.Clear();
            this.ShownPages.AddRange(visiblePages);
            this.AreSettingsVisible = this._session.HasUserPermission(PermissionType.OpenSettingsConfig);
            this.CanExit = this._session.CanUserExit();
            this.IsLoggedIn = this._session.IsLoggedIn;
        }

        [RelayCommand]
        private void Logout()
        {
            this._session.Logout();
            this.OpenLogin();
        }

        [RelayCommand]
        private void OpenLogin()
        {
            var loginViewModel = this.Pages.FirstOrDefault(x => x is LoginViewModel);
            if (loginViewModel is null) return;
            this.ActivePage = loginViewModel;
        }

        [RelayCommand]
        private void ToggleBaseTheme()
        {
            this._theme.SwitchBaseTheme();
            this.UpdateBaseTheme();
        }

        private void UpdateBaseTheme()
        {
            this.BaseTheme = _theme.ActiveBaseTheme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
            this.BaseThemeText = BaseTheme == ThemeVariant.Dark ? Resources.txt_dark_theme : Resources.txt_light_theme;
        }

        [RelayCommand]
        private void ToggleTitleBar()
        {
            TitleBarVisible = !TitleBarVisible;
            this.ShowInfoToast(
                TitleBarVisible
                    ? Resources.c_main_window_title_bar_vissible
                    : Resources.c_main_window_title_bar_hidden,
                TitleBarVisible
                    ? Resources.c_main_window_title_bar_visible_msg
                    : Resources.c_main_window_title_bar_hidden_msg
            );
        }

        [RelayCommand]
        private void Exit(SukiWindow window)
        {
            window.Close();
        }

        [RelayCommand]
        private void Close(SukiWindow window)
        {
            Messenger.Send(new ExitMessage());
        }

        [RelayCommand]
        private void ShowSettings()
        {
            Messenger.Send(new ShowSettingsMessage());
        }

        public void Receive(NavigateMessage message)
        {
            var pageType = message.Value.GetType();
            var page = Pages.FirstOrDefault(x => x.GetType() == pageType);
            if (page is null || this.ActivePage?.GetType() == pageType) return;
            this.ActivePage = page;
        }
    }
}