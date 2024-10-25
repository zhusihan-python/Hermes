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
    [ObservableProperty] private string _selectedSfcResponse = Resources.txt_all;
    [ObservableProperty] private string _selectedTestStatus = Resources.txt_all;
    [ObservableProperty] private string _serialNumberFilter = "";
    public ObservableCollection<UnitUnderTest> UnitsUnderTest { get; set; } = [];
    public ObservableCollection<AllEnum<SfcResponseType>> SfcResponseOptions { get; set; } = [];
    public ObservableCollection<string> TestStatusOptions { get; set; } = [];

    private readonly FileService _fileService;
    private readonly UnitUnderTestRepository _unitUnderTestRepository;

    public UnitUnderTestLogViewModel(
        FileService fileService,
        UnitUnderTestRepository unitUnderTestRepository)
    {
        _fileService = fileService;
        _unitUnderTestRepository = unitUnderTestRepository;

        TestStatusOptions.AddRange(EnumExtensions
            .ValuesToTranslatedString<StatusType>(includeAllOption: true));
        SfcResponseOptions.AddRange(EnumExtensions
            .ValuesToTranslatedString<SfcResponseType>(includeAllOption: true));

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
        SelectedTestStatus = Resources.txt_all;
        await LoadLogsAsync();
    }

    [RelayCommand]
    private async Task Search()
    {
        var units = await _unitUnderTestRepository.GetFromLast24HrsUnits(
            string.IsNullOrWhiteSpace(SerialNumberFilter) ? null : SerialNumberFilter,
            SelectedTestStatus.ToEnum<StatusType>(),
            SelectedSfcResponse.ToEnum<SfcResponseType>());

        UnitsUnderTest.Clear();
        UnitsUnderTest.AddRange(units);
    }
}

public class AllEnum<T> where T : Enum
{
    public readonly T OriginValue;
    private readonly bool _isAll;

    private AllEnum(T originValue, bool isAll = false)
    {
        this.OriginValue = originValue;
        this._isAll = isAll;
    }

    public override string ToString()
    {
        return _isAll ? Resources.txt_all : OriginValue.ToTranslatedString();
    }

    public static readonly List<AllEnum<T>> GetValues = EnumExtensions
        .GetValues<T>()
        .Select(x => new AllEnum<T>(x))
        .Prepend(new AllEnum<T>(default!, isAll: true))
        .ToList();

    /*public static IEnumerable<string> GetValuesToTranslatedString()
    {
        return GetValues.Select(x => _isAll
                ? x.OriginValue.ToTranslatedString()
                : Resources.txt_all)
            .ToList();
    }*/
}