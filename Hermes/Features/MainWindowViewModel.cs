using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Repositories;
using SukiUI;
using System.Collections.Generic;
using System.Linq;
using Hermes.Language;
using SukiUI.Controls;

namespace Hermes.Features
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NavigateMessage>
    {
        public IAvaloniaReadOnlyList<PageBase> Pages { get; }
        [ObservableProperty] private ThemeVariant? _baseTheme;
        [ObservableProperty] private string _baseThemeText = "";
        [ObservableProperty] private bool _titleBarVisible;
        [ObservableProperty] private PageBase? _activePage;

        private readonly SukiTheme _theme;

        public MainWindowViewModel(IEnumerable<PageBase> pages, ISettingsRepository settingsRepository)
        {
            this.Pages = new AvaloniaList<PageBase>(pages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
            this._theme = SukiTheme.GetInstance();
            this._theme.ChangeBaseTheme(ThemeVariant.Light);
            this.UpdateBaseTheme();
            if (settingsRepository.Settings.AutostartUutProcessor)
            {
                Messenger.Send(new StartUutProcessorMessage());
            }
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