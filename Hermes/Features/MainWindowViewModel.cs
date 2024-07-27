using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Features.UutProcessor;
using Hermes.Models;
using SukiUI;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.Features
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NavigateMessage>
    {
        public IAvaloniaReadOnlyList<PageBase> Pages { get; }
        [ObservableProperty] private ThemeVariant? _baseTheme;
        [ObservableProperty] private bool _titleBarVisible;
        [ObservableProperty] private PageBase? _activePage;

        private readonly SukiTheme _theme;

        public MainWindowViewModel(IEnumerable<PageBase> pages, Settings settings)
        {
            this.Pages = new AvaloniaList<PageBase>(pages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
            this._theme = SukiTheme.GetInstance();
            this.UpdateBaseTheme();
            if (settings.AutostartUutProcessor)
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
        }

        [RelayCommand]
        private void ToggleTitleBar()
        {
            TitleBarVisible = !TitleBarVisible;
            Messenger.Send(new ShowToastMessage(
                $"Title Bar {(TitleBarVisible ? "Visible" : "Hidden")}",
                $"Window title bar has been {(TitleBarVisible ? "shown" : "hidden")}."
            ));
        }

        [RelayCommand]
        private void Exit(Window window)
        {
            Messenger.Send(new ExitMessage());
        }

        [RelayCommand]
        private void ShowSnackbar()
        {
            Messenger.Send(new ShowToastMessage("Hello!", "This is a snackbar!"));
        }

        public void Receive(NavigateMessage message)
        {
            var pageType = message.Value.GetType();
            var page = Pages.FirstOrDefault(x => x.GetType() == pageType);
            if (page is null || this.ActivePage?.GetType() == pageType) return;
            this.ActivePage = page;
        }

        private void OnTemeChanged(object? sender, ThemeVariant themeVariant)
        {
            this.BaseTheme = themeVariant;
        }

        public void Start()
        {
            var pageType = typeof(UutProcessorViewModel);
            var page = Pages.FirstOrDefault(x => x.GetType() == pageType);
            (page as UutProcessorViewModel)?.StartCommand.Execute(null);
        }
    }
}