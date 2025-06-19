using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
using Hermes.Common;

namespace Hermes.Features
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NavigateMessage>
    {
        //private PageFactory _pageFactory;

        [ObservableProperty]
        private bool _sideMenuExpanded = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HomePageIsActive))]
        [NotifyPropertyChangedFor(nameof(HistoryPageIsActive))]
        [NotifyPropertyChangedFor(nameof(SettingsPageIsActive))]
        [NotifyPropertyChangedFor(nameof(LoginPageIsActive))]
        private PageBase _currentPage;

        public bool HomePageIsActive => CurrentPage!.DisplayName == PageNames.Home.GetPageDescription();
        public bool HistoryPageIsActive => CurrentPage!.DisplayName == PageNames.History.GetPageDescription();
        public bool SettingsPageIsActive => CurrentPage!.DisplayName == PageNames.Settings.GetPageDescription();
        public bool LoginPageIsActive => CurrentPage!.DisplayName == PageNames.Login.GetPageDescription();
        public List<PageBase> Pages { get; }
        public ISukiToastManager ToastManager { get; }
        public ISukiDialogManager DialogManager { get; }
        public bool CanClose { get; private set; }

        public RangeObservableCollection<PageBase> ShownPages { get; set; } = [];
        [ObservableProperty] private ThemeVariant? _baseTheme;
        [ObservableProperty] private string _baseThemeText = "";
        [ObservableProperty] private string _title = "";
        [ObservableProperty] private bool _titleBarVisible;
        [ObservableProperty] private PageBase? _activePage;
        [ObservableProperty] private bool _areSettingsVisible;
        [ObservableProperty] private bool _canExit;
        [ObservableProperty] private bool _isLoggedIn;

        private readonly PagePrototype _pagePrototype;
        private readonly Session _session;
        private readonly Settings _settings;
        private readonly SukiTheme _theme;

        public MainWindowViewModel(
            PagePrototype pagePrototype,
            IEnumerable<PageBase> pages,
            ISukiDialogManager dialogManager,
            ISukiToastManager toastManager,
            Session session,
            Settings settings)
        {
            this._pagePrototype = pagePrototype;
            this._settings = settings;
            this._session = session;
            //this.ConfigurePages(User.Null);
            this.Pages = pages.ToList();
            this.TitleBarVisible = false;
            this.ToastManager = toastManager;
            this.DialogManager = dialogManager;
            //this.UpdateBaseTheme();
            this.UpdateTitle(this._session.LoggedUser.Value);
            this.IsActive = true;
        }

        protected override void SetupReactiveExtensions()
        {
            this._session
                .LoggedUser
                .Do(this.UpdateTitle)
                .Do(this.ConfigurePages)
                .Subscribe()
                .AddTo(ref Disposables);
        }

        private void UpdateTitle(User user)
        {
            var baseTitle = $"{Resources.txt_hermes}";
            if (!user.IsNull)
            {
                Title = $"{baseTitle}     (👤{user.Name})";
            }
            else
            {
                Title = baseTitle;
            }
        }

        private void ConfigurePages(User user)
        {
            var visiblePages = this._pagePrototype.GetPages(user);
            this.UpdatePages(visiblePages);
            this.AreSettingsVisible = user.HasPermission(PermissionType.OpenSettingsConfig);
            this.CanExit = user.HasPermission(PermissionType.Exit);
            this.IsLoggedIn = !user.IsNull;
            CurrentPage = GetPageByName("主界面");
        }

        private void UpdatePages(List<PageBase> visiblePages)
        {
            this.ClearPages(visiblePages);
            this.ShownPages.AddRange(visiblePages
                .Except(this.ShownPages)
                .OrderBy(x => x.Index)
                .ToList());
        }

        private void ClearPages(List<PageBase> visiblePages)
        {
            var pagesToRemove = this.ShownPages
                .Except(visiblePages)
                .ToList();
            pagesToRemove.ForEach(x => { x.IsActive = false; });
            this.ShownPages.RemoveRange(pagesToRemove);
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
            //this._theme.SwitchBaseTheme();
            //this.UpdateBaseTheme();
        }

        public PageBase GetPageByName(string displayName)
        {
            return ShownPages.FirstOrDefault(page => page.DisplayName == displayName);
        }

        [RelayCommand]
        private void Exit(SukiWindow window)
        {
            Messenger.Send(new ExitMessage());
            CanClose = true;
            window.Close();
        }

        [RelayCommand]
        private void SideMenuResize()
        {
            SideMenuExpanded = !SideMenuExpanded;
        }

        [RelayCommand]
        private void GoToHome() => CurrentPage = GetPageByName(PageNames.Home.GetPageDescription());
        [RelayCommand]
        private void GoToHistory() => CurrentPage = GetPageByName(PageNames.History.GetPageDescription());
        [RelayCommand]
        private void GoToSettings() => CurrentPage = GetPageByName(PageNames.Settings.GetPageDescription());
        [RelayCommand]
        private void GoToLogin() => CurrentPage = GetPageByName(PageNames.Login.GetPageDescription());

        public void Receive(NavigateMessage message)
        {
            var pageType = message.Value.GetType();
            var page = Pages.FirstOrDefault(x => x.GetType() == pageType);
            if (page is null || this.ActivePage?.GetType() == pageType) return;
            this.ActivePage = page;
        }
    }
}