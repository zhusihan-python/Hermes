using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Hermes.Common.Messages;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Material.Icons;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using Hermes.Common.Extensions;

namespace Hermes.Features.Bender;

public partial class BenderViewModel : PageBase
{
    private const int MaxDays = 3;

    [ObservableProperty] private bool _isDateFilter = true;
    [ObservableProperty] private DateTime _fromDate = DateTime.Now.AddDays(-1);
    [ObservableProperty] private DateTime _toDate = DateTime.Now;
    [ObservableProperty] private DateTime _lastDataLoadedAt = DateTime.Now;
    [ObservableProperty] private bool _isDataLoading;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(FindByPkgidCommand))]
    private string _pkgid = "";

    private readonly SfcOracleRepository _sfcOracleRepository;

    private readonly ISettingsRepository _settingsRepository;

    public ObservableCollection<Package> Packages { get; set; } = [];

    public BenderViewModel(SfcOracleRepository sfcOracleRepository, ISettingsRepository settingsRepository) : base(
        "Bender", MaterialIconKind.Qrcode,
        PermissionLevel.Level1, 4)
    {
        this._sfcOracleRepository = sfcOracleRepository;
        this._settingsRepository = settingsRepository;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteFindByPkgid))]
    private async Task FindByPkgid()
    {
        IsDataLoading = true;
        try
        {
            var normalizedPkgId = Package.NormalizePkgId(this.Pkgid);
            var packages = await this._sfcOracleRepository.GetAllPackagesTrackingByPkgid(normalizedPkgId);
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
        }
        finally
        {
            IsDataLoading = false;
        }
    }

    private bool CanExecuteFindByPkgid => !string.IsNullOrEmpty(this.Pkgid);

    [RelayCommand]
    private async Task DataReload()
    {
        this.ToDate = DateTime.Now;
        this.FromDate = this.ToDate.AddDays(-1);
        await this.FindByDate();
    }

    [RelayCommand]
    private async Task FindByDate()
    {
        IsDataLoading = true;
        if (this.ToDate - this.FromDate > TimeSpan.FromDays(MaxDays))
        {
            Messenger.Send(new ShowToastMessage("Error", $"Date range is too big, max is {MaxDays} days",
                NotificationType.Error));
            return;
        }

        var packages = await this._sfcOracleRepository.GetAllPackagesTrackingByDate(
            this._settingsRepository.Settings.Line.ToUpperString(),
            new DateTime(this.FromDate.Year, this.FromDate.Month, this.FromDate.Day, 0, 0, 0),
            new DateTime(this.ToDate.Year, this.ToDate.Month, this.ToDate.Day, 23, 59, 59));
        this.Packages.Clear();
        this.Packages.AddRange(packages.ToList());
        this.IsDataLoading = false;
        this.LastDataLoadedAt = DateTime.Now;
    }

    [RelayCommand]
    private async Task Load(Package package)
    {
        IsDataLoading = true;
        try
        {
            var result = await this._sfcOracleRepository.SetPackageTrackingLoadedAt(package.Id);
            if (result > 0)
            {
                package.LoadedAt = DateTime.Now;
            }
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage("Error", "Error loading package", NotificationType.Error));
        }
        finally
        {
            IsDataLoading = false;
        }
    }
}