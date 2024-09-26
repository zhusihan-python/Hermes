using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Hermes.Common.Messages;
using Hermes.Features.Login;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using SukiUI.Controls;
using SukiUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hermes.Cipher.Types;
using Hermes.Types;
using SukiUI.Dialogs;
using SukiUI.Toasts;

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
        private readonly string _baseTitle;

        private readonly SukiTheme _theme;
        private readonly Session _session;
        private readonly ISettingsRepository _settingsRepository;

        public MainWindowViewModel(
            Session session,
            IEnumerable<PageBase> pages,
            ISettingsRepository settingsRepository,
            ISukiToastManager toastManager,
            ISukiDialogManager dialogManager)
        {
            this._session = session;
            this._session.UserChanged += this.OnUserChanged;
            this._theme = SukiTheme.GetInstance();
            this._theme.ChangeBaseTheme(ThemeVariant.Light);
            this._settingsRepository = settingsRepository;
            this.Pages = pages.ToList();
            this.TitleBarVisible = false;
            this.ToastManager = toastManager;
            this.DialogManager = dialogManager;
            this.ConfigureBasedOnSession();
            this.UpdateBaseTheme();
            this._baseTitle =
                $"{Resources.txt_hermes} - {_settingsRepository.Settings.Station} - {_settingsRepository.Settings.Line}";
            Title = this._baseTitle;
            if (settingsRepository.Settings.AutostartUutProcessor)
            {
                Messenger.Send(new StartUutProcessorMessage());
            }
        }

        private void OnUserChanged(User user)
        {
            this.ConfigureBasedOnSession();
            if (!user.IsNull)
            {
                Title = $"{this._baseTitle}     (👤{user.Name})";
            }
            else
            {
                Title = this._baseTitle;
            }
        }

        private void ConfigureBasedOnSession()
        {
            var visiblePages = Pages
                .Where(x =>
                    _session.UserDepartmentType == DepartmentType.Admin ||
                    x.FeatureType == FeatureType.FreeAccess ||
                    _session.HasUserPermission(x.FeatureType))
                .Where(x =>
                    _session.UserDepartmentType == DepartmentType.Admin ||
                    x.StationFilter == null ||
                    x.StationFilter.Contains(_settingsRepository.Settings.Station))
                .OrderBy(x => x.Index)
                .ThenBy(x => x.DisplayName)
                .ToList();
            this.ShownPages.Clear();
            this.ShownPages.AddRange(visiblePages);
            this.AreSettingsVisible = _session.HasUserPermission(FeatureType.SettingsConfig);
            this.CanExit = _session.CanUserExit();
            this.IsLoggedIn = _session.IsLoggedIn;
        }

        [RelayCommand]
        private void Logout()
        {
            _session.Logout();
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
            Messenger.Send(new ShowToastMessage(
                TitleBarVisible ? Resources.c_main_window_title_bar_vissible : Resources.c_main_window_title_bar_hidden,
                TitleBarVisible
                    ? Resources.c_main_window_title_bar_visible_msg
                    : Resources.c_main_window_title_bar_hidden_msg
            ));
        }

        [RelayCommand]
        private void Exit(SukiWindow window)
        {
            Messenger.Send(new ExitMessage());
            window.Close();
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