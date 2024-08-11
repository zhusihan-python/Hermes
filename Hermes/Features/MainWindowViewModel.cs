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

namespace Hermes.Features
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NavigateMessage>
    {
        public List<PageBase> Pages { get; }
        public ObservableCollection<PageBase> ShownPages { get; set; } = [];
        [ObservableProperty] private ThemeVariant? _baseTheme;
        [ObservableProperty] private string _baseThemeText = "";
        [ObservableProperty] private bool _titleBarVisible;
        [ObservableProperty] private PageBase? _activePage;
        [ObservableProperty] private bool _areSettingsVisible;
        [ObservableProperty] private bool _canExit;
        [ObservableProperty] private bool _isLoggedIn;

        private readonly SukiTheme _theme;
        private readonly Session _session;

        public MainWindowViewModel(
            Session session,
            IEnumerable<PageBase> pages,
            ISettingsRepository settingsRepository)
        {
            this._session = session;
            this._session.UserChanged += this.OnUserChanged;
            this._theme = SukiTheme.GetInstance();
            this._theme.ChangeBaseTheme(ThemeVariant.Light);
            this.Pages = pages.ToList();
            this.ConfigureBasedOnSession();
            this.UpdateBaseTheme();
            if (settingsRepository.Settings.AutostartUutProcessor)
            {
                Messenger.Send(new StartUutProcessorMessage());
            }
        }

        private void OnUserChanged(User user)
        {
            this.ConfigureBasedOnSession();
        }

        private void ConfigureBasedOnSession()
        {
            var visiblePages = Pages
                .Where(x => _session.CanUserView(x.RequiredViewLevel))
                .OrderBy(x => x.Index)
                .ThenBy(x => x.DisplayName)
                .ToList();
            this.ShownPages.RemoveMany(ShownPages.Except(visiblePages));
            this.ShownPages.AddRange(visiblePages.Except(ShownPages));
            this.AreSettingsVisible = _session.CanUserUpdate(1);
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