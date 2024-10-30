using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Cipher.Types;
using Hermes.Features.Controls.Token;
using Hermes.Models;
using Hermes.Repositories;
using SukiUI.Toasts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
            cloneTokenViewModel.Unlocked += this.OnMultiUserTokenUnlocked;
            this.StopLineTokenViewModels.Add(cloneTokenViewModel);
            if (department == DepartmentType.Qa)
            {
                this._qaTokenViewModel = cloneTokenViewModel;
            }
        }

        this.Actions = string.Empty;
        this.StopMachineTokenViewModel = tokenViewModel;
        this.StopMachineTokenViewModel.Unlocked += this.OnSingleUserTokenUnlocked;
    }

    private bool CanUnlockQa()
    {
        return this.StopLineTokenViewModels
                   .Where(x => x != _qaTokenViewModel)
                   .All(tokenViewModel => tokenViewModel.IsUnlocked) &&
               this.Actions.Length > 5;
    }

    private void OnSingleUserTokenUnlocked(object? sender, EventArgs e)
    {
        var user = (sender as TokenViewModel)?.User;
        this.RestoreStop(user == null ? [] : [user]);
    }

    private void OnMultiUserTokenUnlocked(object? sender, EventArgs e)
    {
        if (this.StopLineTokenViewModels.All(tokenViewModel => tokenViewModel.IsUnlocked))
        {
            var users = this.StopLineTokenViewModels
                .Select(tokenViewModel => tokenViewModel.User)
                .ToList();
            this.RestoreStop(users);
        }
    }

    partial void OnStopChanged(Stop value)
    {
        IsMachineStop = value.IsMachineStop;
    }

    private async void RestoreStop(List<User> users)
    {
        try
        {
            if (this.Stop is { IsNull: false, IsFake: false })
            {
                this.Stop.Actions = this.Actions;
                await this._stopRepository.RestoreAsync(this.Stop, users);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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