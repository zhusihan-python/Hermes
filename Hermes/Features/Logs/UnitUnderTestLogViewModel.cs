using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Repositories;
using Hermes.Services;
using Hermes.Types;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using DynamicData;
using Hermes.Language;
using Hermes.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hermes.Features.Logs;

public partial class UnitUnderTestLogViewModel : ViewModelBase
{
    [ObservableProperty] private UnitUnderTest _selectedUnitUnderTest = UnitUnderTest.Null;
    [ObservableProperty] private AllEnum<SfcResponseType> _selectedSfcResponse = AllEnum<SfcResponseType>.All;
    [ObservableProperty] private AllEnum<StatusType> _selectedTestStatus = AllEnum<StatusType>.All;
    [ObservableProperty] private string _serialNumberFilter = "";
    public ObservableCollection<UnitUnderTest> UnitsUnderTest { get; set; } = [];
    public ObservableCollection<AllEnum<SfcResponseType>> SfcResponseOptions { get; set; } = [];
    public ObservableCollection<AllEnum<StatusType>> StatusOptions { get; set; } = [];

    private readonly FileService _fileService;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;

    public UnitUnderTestLogViewModel(
        FileService fileService,
        UnitUnderTestRepository unitUnderTestRepository)
    {
        _fileService = fileService;
        _unitUnderTestRepository = unitUnderTestRepository;

        StatusOptions.AddRange(AllEnum<StatusType>.GetValues());
        SfcResponseOptions.AddRange(AllEnum<SfcResponseType>.GetValues());

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
        SelectedTestStatus = AllEnum<StatusType>.All;
        SelectedSfcResponse = AllEnum<SfcResponseType>.All;
        await LoadLogsAsync();
    }

    [RelayCommand]
    private async Task Search()
    {
        var units = await _unitUnderTestRepository.GetFromLast24HrsUnits(
            string.IsNullOrWhiteSpace(SerialNumberFilter) ? null : SerialNumberFilter,
            SelectedTestStatus.Value,
            SelectedSfcResponse.Value);

        UnitsUnderTest.Clear();
        UnitsUnderTest.AddRange(units);
    }
}

public class AllEnum<T> where T : struct, Enum
{
    public static readonly AllEnum<T> All = new(null);
    public T? Value { get; }

    private AllEnum(T? originValue)
    {
        this.Value = originValue;
    }

    public override string ToString()
    {
        return Value?.ToTranslatedString() ?? Resources.txt_all;
    }

    public static IEnumerable<AllEnum<T>> GetValues()
    {
        return EnumExtensions
            .GetValues<T>()
            .Select(x => new AllEnum<T>(x))
            .Prepend(All)
            .ToArray();
    }
}