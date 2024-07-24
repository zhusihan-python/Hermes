using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Models;
using Hermes.Services;
using Hermes.Utils;
using System;

namespace Hermes.ViewModels;

public partial class StopViewModel : ViewModelBase
{
    public event EventHandler? Restored;
    public TokenViewModel TokenViewModel { get; }
    [ObservableProperty] private Stop _stop = Models.Stop.Null;
    private readonly ILogger _logger;


    public StopViewModel(
        TokenViewModel tokenViewModel,
        ILogger logger
    )
    {
        this._logger = logger;
        this.TokenViewModel = tokenViewModel;
        tokenViewModel.Unlocked += this.OnTokenUnlocked;
    }

    private void OnTokenUnlocked(object? sender, EventArgs e)
    {
        // TODO
        this.RestoreStop();
    }

    private async void RestoreStop()
    {
        if (this.Stop != null)
        {
            // TODO await this._stopService.RestoreAsync(this._session.Stop);
        }

        this._logger.Info($"Stop restore type:{this.Stop?.Type} id:{this.Stop?.Id}");
        this.Restored?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        this.Stop = Stop.Null;
        this.TokenViewModel.Reset();
    }
}