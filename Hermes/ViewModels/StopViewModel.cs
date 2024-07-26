using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Models;
using Hermes.Utils;
using System;
using System.Threading.Tasks;
using Hermes.Repositories;

namespace Hermes.ViewModels;

public partial class StopViewModel : ViewModelBase
{
    public event EventHandler? Restored;
    public TokenViewModel TokenViewModel { get; }
    [ObservableProperty] private Stop _stop = Stop.Null;
    private readonly ILogger _logger;
    private readonly StopRepository _stopRepository;

    public StopViewModel(
        ILogger logger,
        TokenViewModel tokenViewModel,
        StopRepository stopRepository
    )
    {
        this._logger = logger;
        this.TokenViewModel = tokenViewModel;
        this._stopRepository = stopRepository;
        tokenViewModel.Unlocked += this.OnTokenUnlocked;
    }

    private void OnTokenUnlocked(object? sender, EventArgs e)
    {
        // TODO
        this.RestoreStop();
    }

    partial void OnStopChanged(Stop value)
    {
        if (!value.IsNull)
        {
            Task.Run(() => this._stopRepository.AddAsync(value));
        }
    }

    private async void RestoreStop()
    {
        if (!this.Stop.IsNull)
        {
            await this._stopRepository.RestoreAsync(this.Stop);
        }

        this._logger.Info($"Stop restore type:{this.Stop.Type} id:{this.Stop.Id}");
        this.Restored?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        this.Stop = Stop.Null;
        this.TokenViewModel.Reset();
    }
}