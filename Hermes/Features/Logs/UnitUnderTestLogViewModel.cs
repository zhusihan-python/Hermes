using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Hermes.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Hermes.Features.Logs;

public partial class UnitUnderTestLogViewModel : ViewModelBase
{
    [ObservableProperty] private UnitUnderTest _selectedUnitUnderTest = UnitUnderTest.Null;
    [ObservableProperty] private SfcResponseType? _selectedSfcResponse;
    [ObservableProperty] private StatusType? _selectedTestStatus;
    [ObservableProperty] private string _serialNumberFilter = "";
    public ObservableCollection<UnitUnderTest> UnitsUnderTest { get; set; } = [];
    public ObservableCollection<SfcResponseType?> SfcResponseOptions { get; set; } = [];
    public ObservableCollection<StatusType?> StatusOptions { get; set; } = [];

    private readonly FileService _fileService;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;

    public UnitUnderTestLogViewModel(
        FileService fileService,
        UnitUnderTestRepository unitUnderTestRepository)
    {
        _fileService = fileService;
        _unitUnderTestRepository = unitUnderTestRepository;

        SfcResponseOptions.AddRange(NullableExtensions.GetValues<SfcResponseType>());
        StatusOptions.AddRange(NullableExtensions.GetValues<StatusType>());

        LoadLogsAsync().ConfigureAwait(false);
    }

    private async Task ReSendFile()
    {
        await _fileService.CopyFromBackupToInputAsync(
            _fileService.GetBackupFullPathByName(SelectedUnitUnderTest.FileName));
    }

    private async Task LoadLogsAsync()
    {
        UnitsUnderTest.Clear();
        UnitsUnderTest.AddRange(await _unitUnderTestRepository.GetAllLast24HrsUnits());
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

    [RelayCommand]
    private void Edit()
    {
        new Process
        {
            StartInfo = new ProcessStartInfo(_fileService.GetBackupFullPathByName(SelectedUnitUnderTest.FileName))
            {
                UseShellExecute = true
            }
        }.Start();
    }

    [RelayCommand]
    private async Task ReSend()
    {
        await ReSendFile();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        SerialNumberFilter = "";
        SelectedTestStatus = null;
        SelectedSfcResponse = null;
        await LoadLogsAsync();
    }

    [RelayCommand]
    private async Task Search()
    {
        var units = await _unitUnderTestRepository.GetFromLast24HrsUnits(
            string.IsNullOrWhiteSpace(SerialNumberFilter) ? null : SerialNumberFilter,
            SelectedTestStatus,
            SelectedSfcResponse);

        UnitsUnderTest.Clear();
        UnitsUnderTest.AddRange(units);
    }
}