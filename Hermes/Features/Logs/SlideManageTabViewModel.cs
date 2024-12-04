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

namespace Hermes.Features.Logs;

public partial class SlideManageTabViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    private UnitUnderTest _selectedUnitUnderTest = UnitUnderTest.Null;

    [ObservableProperty] private SfcResponseType? _selectedSfcResponse;
    [ObservableProperty] private StatusType? _selectedTestStatus;
    [ObservableProperty] private TimeSpanType? _selectedTimeSpan = TimeSpanType.OneDay;
    [ObservableProperty] private string _serialNumberFilter = "";
    [ObservableProperty] private string _sfcResponseContentFilter = "";
    public RangeObservableCollection<UnitUnderTest> UnitsUnderTest { get; set; } = [];
    public static IEnumerable<SfcResponseType?> SfcResponseOptions => NullableExtensions.GetValues<SfcResponseType>();
    public static IEnumerable<StatusType?> StatusOptions => NullableExtensions.GetValues<StatusType>();
    public static IEnumerable<TimeSpanType?> TimeSpanOptions => NullableExtensions.GetValues<TimeSpanType>();

    private readonly FileService _fileService;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;

    public SlideManageTabViewModel(
        FileService fileService,
        UnitUnderTestRepository unitUnderTestRepository)
    {
        _fileService = fileService;
        _unitUnderTestRepository = unitUnderTestRepository;
    }

    private async Task LoadLogsAsync()
    {
        UnitsUnderTest.Clear();
        UnitsUnderTest.AddRange(await _unitUnderTestRepository.GetAllLast24HrsUnits());
    }

    [RelayCommand]
    private void UnitUnderTestSelected(UnitUnderTest? unitUnderTest)
    {
        this.SelectedUnitUnderTest = unitUnderTest ?? UnitUnderTest.Null;
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void Edit()
    {
        try
        {
            Process.Start("notepad.exe", SelectedUnitUnderTest.FullPath);
        }
        catch (Exception e)
        {
            this.ShowErrorToast(e.Message);
        }
    }

    private bool CanEdit() => !string.IsNullOrEmpty(SelectedUnitUnderTest.FullPath);

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
    private async Task Search()
    {
        //var units = await _unitUnderTestRepository.GetLastUnits(
        //    SerialNumberFilter,
        //    SelectedTestStatus,
        //    SelectedSfcResponse,
        //    SfcResponseContentFilter,
        //    SelectedTimeSpan == null ? null : TimeSpan.FromHours((int)SelectedTimeSpan));

        //UnitsUnderTest.Clear();
        //UnitsUnderTest.AddRange(units);
    }
}