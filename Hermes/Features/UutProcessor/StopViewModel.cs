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
using SukiUI.Toasts;

namespace Hermes.Features.UutProcessor;

public partial class StopViewModel : ViewModelBase
{
    public ISukiToastManager ToastManager { get; }

    public event EventHandler? Restored;
    public TokenViewModel StopMachineTokenViewModel { get; }
    public List<TokenViewModel> StopLineTokenViewModels { get; } = [];
    private readonly TokenViewModel _qaTokenViewModel = null!;
    [ObservableProperty] private bool _isMachineStop = true;
    [ObservableProperty] private Stop _stop = Stop.Null;
    [ObservableProperty] private string _dateText = string.Empty;
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
        StopRepository stopRepository,
        TokenViewModel tokenViewModel)
    {
        this._stopRepository = stopRepository;
        this.ToastManager = new SukiToastManager();
        tokenViewModel.ToastManager = this.ToastManager;

        DepartmentType[] departmentsToUnlockStop =
            [DepartmentType.Ee, DepartmentType.Mfg, DepartmentType.Aoi, DepartmentType.Qa];
        foreach (var department in departmentsToUnlockStop)
        {
            var cloneTokenViewModel = tokenViewModel.Clone();
            cloneTokenViewModel.ClearDepartments();
            cloneTokenViewModel.Add(department);
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