using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Cipher.Types;
using Hermes.Common;
using Hermes.Features.Controls.Token;
using Hermes.Models;
using Hermes.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Features.UutProcessor;

public partial class StopViewModel : ViewModelBase
{
    public event EventHandler? Restored;
    public TokenViewModel StopMachineTokenViewModel { get; }
    public List<TokenViewModel> StopLineTokenViewModels { get; } = [];
    private readonly TokenViewModel _qaTokenViewModel = null!;
    [ObservableProperty] private bool _isMachineStop = true;
    [ObservableProperty] private Stop _stop = Stop.Null;
    [ObservableProperty] private string _dateText = string.Empty;
    private readonly ILogger _logger;
    private readonly StopRepository _stopRepository;

    public string Actions
    {
        get => _actions;
        set
        {
            this._qaTokenViewModel.CanUnlock = this.CanUnlockQa();
            SetProperty(ref _actions, value);
        }
    }

    private string _actions = "";

    public StopViewModel(
        ILogger logger,
        StopRepository stopRepository,
        TokenViewModel tokenViewModel)
    {
        this._logger = logger;
        this._stopRepository = stopRepository;

        var departments = Enum.GetValues<DepartmentType>();
        foreach (var department in departments)
        {
            var cloneTokenViewModel = tokenViewModel.Clone();
            cloneTokenViewModel.Department = department;
            cloneTokenViewModel.Unlocked += this.OnTokenUnlocked;
            this.StopLineTokenViewModels.Add(cloneTokenViewModel);
            if (department == DepartmentType.Qa)
            {
                this._qaTokenViewModel = cloneTokenViewModel;
            }
        }

        this.Actions = string.Empty;
        this.StopMachineTokenViewModel = tokenViewModel;
        this.StopMachineTokenViewModel.Unlocked += this.OnTokenUnlocked;
    }

    private bool CanUnlockQa()
    {
        return this.StopLineTokenViewModels
                   .Where(x => x != _qaTokenViewModel)
                   .All(tokenViewModel => tokenViewModel.IsUnlocked) &&
               this.Actions.Length > 5;
    }

    private void OnTokenUnlocked(object? sender, EventArgs e)
    {
        if (
            sender == StopMachineTokenViewModel ||
            this.StopLineTokenViewModels.All(tokenViewModel => tokenViewModel.IsUnlocked))
        {
            this.RestoreStop();
        }
    }

    partial void OnStopChanged(Stop value)
    {
        IsMachineStop = value.IsMachineStop;
        if (value is { IsNull: false, IsFake: false })
        {
            Task.Run(() => this._stopRepository.AddAndSaveAsync(value));
        }
    }

    private async void RestoreStop()
    {
        if (this.Stop is { IsNull: false, IsFake: false })
        {
            await this._stopRepository.RestoreAsync(this.Stop);
        }

        this._logger.Info($"Stop restored | type:{this.Stop.Type} | id:{this.Stop.Id}");
        this.Restored?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        this.Stop = Stop.Null;
        this.StopMachineTokenViewModel.Reset();
        foreach (var tokenViewModel in this.StopLineTokenViewModels)
        {
            tokenViewModel.Reset();
        }

        this.Actions = string.Empty;
    }

    public void UpdateDate()
    {
        this.DateText = DateTime.Now.ToString("yyyy MM dd");
    }
}