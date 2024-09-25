using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System;

namespace Hermes.Features.Bender;

public partial class PackageTrackingViewModel : ViewModelBase
{
    private const int MaxDays = 3;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(1);

    public ObservableCollection<Package> Packages { get; set; } = [];
    public bool IsRowSelected => this.SelectedPackage != null;

    [ObservableProperty] private bool _isDateFilter = true;
    [ObservableProperty] private DateTime _fromDate = DateTime.Now.AddDays(-1);
    [ObservableProperty] private DateTime _toDate = DateTime.Now;
    [ObservableProperty] private DateTime _lastDataLoadedAt = DateTime.Now;
    [ObservableProperty] private bool _isDataLoading;
    [ObservableProperty] private double _elapsedRefreshTime;
    [ObservableProperty] private double _elapsedRefreshPercentage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRowSelected))]
    [NotifyCanExecuteChangedFor(nameof(RemovePackageFromLoadedCommand))]
    private Package? _selectedPackage;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(FindByPkgidCommand))]
    private string _pkgid = "";

    private readonly ISfcRepository _sfcRepository;

    private readonly ISettingsRepository _settingsRepository;
    private readonly Timer _timer;
    private readonly Timer _timerTick;
    private readonly ILogger _logger;

    public PackageTrackingViewModel(ISfcRepository sfcRepository, ISettingsRepository settingsRepository,
        ILogger logger)
    {
        this._sfcRepository = sfcRepository;
        this._settingsRepository = settingsRepository;
        this._logger = logger;
        this._timer = new Timer(RefreshInterval.TotalMilliseconds);
        this._timer.Elapsed += TimerOnElapsed;
        this._timerTick = new Timer(TickInterval.TotalMilliseconds);
        this._timerTick.Elapsed += TimerTickOnElapsed;
    }

    private void TimerTickOnElapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            this.ElapsedRefreshTime++;
            this.ElapsedRefreshPercentage = (this.ElapsedRefreshTime / RefreshInterval.TotalSeconds) * 100;
        });
    }

    private async void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!IsDataLoading)
        {
            await Dispatcher.UIThread.InvokeAsync(async () => { await this.FindByDate(); });
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteFindByPkgid))]
    private async Task FindByPkgid()
    {
        IsDataLoading = true;
        this.StopTimers();
        try
        {
            var normalizedPkgId = Package.NormalizePkgId(this.Pkgid);
            var packages = await this._sfcRepository.FindAllPackagesTrackingByPkgid(normalizedPkgId);
            this.Packages.Clear();
            this.Packages.AddRange(packages.ToList());
            if (this.Packages.Count <= 0)
            {
                Messenger.Send(new ShowToastMessage("Not found", "Package not found", NotificationType.Warning));
            }
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage("Error", "Error finding package", NotificationType.Error));
            _logger.Error(e.Message);
        }
        finally
        {
            IsDataLoading = false;
            this.StartTimers();
        }
    }

    private void StartTimers()
    {
        this._timer.Start();
        this._timerTick.Start();
    }

    private void StopTimers()
    {
        this._timer.Stop();
        this._timerTick.Stop();
        this.ElapsedRefreshTime = 0;
        this.ElapsedRefreshPercentage = 0;
    }

    private bool CanExecuteFindByPkgid => !string.IsNullOrEmpty(this.Pkgid);

    [RelayCommand]
    public async Task DataReload()
    {
        this.ToDate = DateTime.Now;
        this.FromDate = this.ToDate.AddDays(-1);
        await this.FindByDate();
    }

    [RelayCommand]
    private async Task FindByDate()
    {
        if (this.ToDate - this.FromDate > TimeSpan.FromDays(MaxDays))
        {
            Messenger.Send(new ShowToastMessage("Error", $"Date range is too big, max is {MaxDays} days",
                NotificationType.Error));
            return;
        }

        IsDataLoading = true;
        this.StopTimers();
        try
        {
            var packages = await this._sfcRepository.FindAllPackagesTrackingByDate(
                this._settingsRepository.Settings.Line.ToUpperString(),
                this.FromDate.ToStartOfDay(),
                this.ToDate.ToEndOfDay());
            this.Packages.Clear();
            this.Packages.AddRange(packages.ToList());
            this.LastDataLoadedAt = DateTime.Now;
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage("Error", $"Error loading packages: {e.Message}",
                NotificationType.Error));
            _logger.Error(e.Message);
        }
        finally
        {
            this.IsDataLoading = false;
            this.StartTimers();
        }
    }

    [RelayCommand]
    private async Task Load(Package package)
    {
        IsDataLoading = true;
        try
        {
            var result = await this._sfcRepository.UpdatePackageTrackingLoadedAt(package.Id);
            if (result > 0)
            {
                package.LoadedAt = DateTime.Now;
            }
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage("Error", "Error loading package", NotificationType.Error));
            _logger.Error(e.Message);
        }
        finally
        {
            IsDataLoading = false;
        }
    }

    [RelayCommand]
    private void PackageSelected(Package? package)
    {
        if (package == null) return;
        this.SelectedPackage = package.IsUsed ? null : package;
    }

    [RelayCommand(CanExecute = nameof(CanRemovePackageFromLoaded))]
    private void RemovePackageFromLoaded()
    {
        try
        {
            if (SelectedPackage == null) return;
            this._sfcRepository.ResetPackageTrackingLoadedAt(SelectedPackage.Id);
            SelectedPackage.LoadedAt = null;
            Messenger.Send(new ShowToastMessage("Success", "Package removed from loaded", NotificationType.Success));
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage("Error", "Error removing package from loaded", NotificationType.Error));
            _logger.Error(e.Message);
        }
    }

    private bool CanRemovePackageFromLoaded => SelectedPackage is { IsLoaded: true };

    [RelayCommand]
    private async Task DeletePackageTracking()
    {
        try
        {
            if (SelectedPackage == null) return;
            await this._sfcRepository.DeletePackageTracking(SelectedPackage.Id);
            await this.DataReload();
            Messenger.Send(new ShowToastMessage("Success", "Package removed", NotificationType.Success));
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage("Error", "Error removing package", NotificationType.Error));
            _logger.Error(e.Message);
        }
    }
}