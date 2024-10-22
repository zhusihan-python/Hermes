using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Language;
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

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(FindByPkgIdCommand))]
    private string _pkgId = "";

    private readonly ILogger _logger;
    private readonly Session _session;
    private readonly ISfcRepository _sfcRepository;
    private readonly Timer _timer;
    private readonly Timer _timerTick;

    public PackageTrackingViewModel(
        ILogger logger,
        ISfcRepository sfcRepository,
        Session session)
    {
        this._logger = logger;
        this._sfcRepository = sfcRepository;
        this._session = session;
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

    [RelayCommand(CanExecute = nameof(CanExecuteFindByPkgId))]
    private async Task FindByPkgId()
    {
        try
        {
            IsDataLoading = true;
            this.StopTimers();
            try
            {
                var normalizedPkgId = Package.NormalizePkgId(this.PkgId);
                var packages = await this._sfcRepository.FindAllPackagesTrackingByPkgid(normalizedPkgId);
                this.Packages.Clear();
                this.Packages.AddRange(packages.ToList());
                if (this.Packages.Count <= 0)
                {
                    this.ShowWarningToast(Resources.msg_package_not_found, Resources.txt_not_found);
                }
            }
            finally
            {
                IsDataLoading = false;
                this.StartTimers();
            }
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
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

    private bool CanExecuteFindByPkgId => !string.IsNullOrEmpty(this.PkgId);

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
            throw new Exception($"Date range is too big, max is {MaxDays} days");
        }

        IsDataLoading = true;
        this.StopTimers();
        try
        {
            var packages = await this._sfcRepository.FindAllPackagesTrackingByDate(
                this._session.Settings.Line.ToUpperString(),
                this.FromDate.ToStartOfDay(),
                this.ToDate.ToEndOfDay());
            this.Packages.Clear();
            this.Packages.AddRange(packages.ToList());
            this.LastDataLoadedAt = DateTime.Now;
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
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
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
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
            this.ShowSuccessToast(Resources.msg_package_removed_from_loaded);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
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
            this.ShowSuccessToast(Resources.msg_package_removed);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }
}