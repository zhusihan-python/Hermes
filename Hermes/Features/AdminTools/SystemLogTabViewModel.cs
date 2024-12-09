using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Hermes.Types;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System;
using Hermes.Common;

namespace Hermes.Features.AdminTools;

public partial class SystemLogTabViewModel : ViewModelBase
{
    [ObservableProperty]
    private UnitUnderTest _selectedUnitUnderTest = UnitUnderTest.Null;

    [ObservableProperty] private SfcResponseType? _selectedSfcResponse;
    [ObservableProperty] private StatusType? _selectedTestStatus;
    [ObservableProperty] private TimeSpanType? _selectedTimeSpan = TimeSpanType.OneDay;
    [ObservableProperty] private string _serialNumberFilter = "";
    [ObservableProperty] private string _sfcResponseContentFilter = "";
    public RangeObservableCollection<UnitUnderTest> UnitsUnderTest { get; set; } = [];
    public static IEnumerable<SfcResponseType?> SfcResponseOptions => NullableExtensions.GetValues<SfcResponseType>();
    public static IEnumerable<TimeSpanType?> TimeSpanOptions => NullableExtensions.GetValues<TimeSpanType>();

    public SystemLogTabViewModel()
    {

    }

    private async Task LoadLogsAsync()
    {
        await Task.Delay(200);
    }

    [RelayCommand]
    private void UnitUnderTestSelected(UnitUnderTest? unitUnderTest)
    {
        this.SelectedUnitUnderTest = unitUnderTest ?? UnitUnderTest.Null;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        SerialNumberFilter = "";
        SelectedTestStatus = null;
        SelectedSfcResponse = null;
        SfcResponseContentFilter = "";
        SelectedTimeSpan = TimeSpanType.OneDay;
        await LoadLogsAsync();
    }

    [RelayCommand]
    private void Search()
    {

    }
}