﻿using Avalonia.Controls.Notifications;
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

public partial class UnitUnderTestLogViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReSendCommand))]
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

    public UnitUnderTestLogViewModel(
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

    [RelayCommand]
    private async Task Export()
    {
        string date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
        string name = $"{date} Report-List";
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var subFolderPath = Path.Combine(path, "Exports");
        Directory.CreateDirectory(subFolderPath);
        var filePath = Path.Combine(subFolderPath, $"{name}.csv");
        using (var writer = new StreamWriter(filePath))
        {
            await writer.WriteLineAsync("Id,SerialNumber,Filename,TestStatus,SfcResponse,CreatedAt");

            foreach (var uut in UnitsUnderTest)
            {
                await writer.WriteLineAsync(
                    $"{uut.Id},{uut.SerialNumber},{uut.FileName},{uut.IsPass},{uut.SfcResponse},{uut.CreatedAt}");
            }
        }

        Messenger.Send(new ShowToastMessage("Success", "Export Success: " + filePath, NotificationType.Success));
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


    [RelayCommand(CanExecute = nameof(CanResend))]
    private void ReSend()
    {
        Messenger.Send(new ReSendUnitUnderTestMessage(SelectedUnitUnderTest));
    }

    private bool CanResend() => !string.IsNullOrEmpty(SelectedUnitUnderTest.SerialNumber);

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
        var units = await _unitUnderTestRepository.GetLastUnits(
            SerialNumberFilter,
            SelectedTestStatus,
            SelectedSfcResponse,
            SfcResponseContentFilter,
            SelectedTimeSpan == null ? null : TimeSpan.FromHours((int)SelectedTimeSpan));

        UnitsUnderTest.Clear();
        UnitsUnderTest.AddRange(units);
    }
}