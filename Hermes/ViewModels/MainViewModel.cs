using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Models.Messages;
using Hermes.Repositories;

namespace Hermes.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public UutProcessorViewModel UutProcessorViewModel { get; }

        public MainViewModel(UutProcessorViewModel uutProcessorViewModel, HermesContext hermesContext)
        {
            this.UutProcessorViewModel = uutProcessorViewModel;
            hermesContext.Initialize();
        }

        [RelayCommand]
        private void Exit(Window window)
        {
            this.UutProcessorViewModel.Stop();
            Messenger.Send(new ExitMessage());
        }

        [RelayCommand]
        private void ShowSnackbar()
        {
            Messenger.Send(new ShowSnackbarMessage("This is a snackbar!"));
        }
    }
}