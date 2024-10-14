using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Hermes.Common.Extensions;
using Hermes.Types;
using Hermes.Services;
using Hermes.Models;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Material.Icons;
using ReactiveUI;
using Avalonia.Controls.Notifications;
using Hermes.Common.Messages;
using Hermes.Language;
using CommunityToolkit.Mvvm.Messaging;

namespace Hermes.Features.Logs
{
    public partial class UnitUnderTestLogViewModel : ViewModelBase
    {
        private readonly FileService _fileService;

        private readonly UnitUnderTestRepository _unitUnderTestRepository;

        private readonly UnitUnderTestLogView _view;

        public ObservableCollection<LogEntry> Logs { get; set; }
        public ObservableCollection<string> TestStatusOptions { get; set; }
        public ObservableCollection<SfcResponseType> SfcResponseOptions { get; set; }

        [ObservableProperty] private string _serialNumberFilter;

        [ObservableProperty] private string _selectedTestStatus;

        [ObservableProperty] private string _selectedSfcResponse;

        [ObservableProperty] private LogEntry _selectedLogEntry;

        public UnitUnderTestLogViewModel(
            UnitUnderTestRepository unitUnderTestRepository,
            FileService fileService,
            UnitUnderTestLogView view)
        {
            _view = view;
            SerialNumberFilter = "";
            SelectedTestStatus = "All";
            _fileService = fileService;
            _unitUnderTestRepository = unitUnderTestRepository;
            Logs = new ObservableCollection<LogEntry>();
            TestStatusOptions = new ObservableCollection<string>
            {
                "Pass",
                "Fail",
                "All"
            };
            SfcResponseOptions =
                new ObservableCollection<SfcResponseType>(Enum.GetValues(typeof(SfcResponseType)) as SfcResponseType[]);
            LoadLogsAsync().ConfigureAwait(false);
        }

        public async Task EditFile()
        {
            var fullPath = GetFullPathBySelectedFilename();
            if (fullPath != null)
            {
                new Process
                {
                    StartInfo = new ProcessStartInfo(fullPath)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
        }

        public string GetFullPathBySelectedFilename()
        {
            var filename = SelectedLogEntry?.Filename.ToString();
            if (filename != null)
            {
                var fullpath = _fileService.GetBackupFullPathByName(filename);
                return fullpath;
            }
            else
            {
                Debug.WriteLine("No se selecciono nada");
            }

            return null;
        }

        private async Task ReSendFile()
        {
            var fullpath = GetFullPathBySelectedFilename();
            if (fullpath != null)
            {
                Debug.WriteLine($"Selected Filename: {fullpath}");
                Debug.WriteLine("Try copy");
                await _fileService.CopyFromBackupToInputAsync(fullpath);
            }
        }

        private async Task LoadLogsAsync()
        {
            var units = await _unitUnderTestRepository.GetAllUnits();
            foreach (var unit in units)
            {
                Logs.Add(new LogEntry
                {
                    Id = unit.Id,
                    SerialNumber = unit.SerialNumber,
                    Filename = unit.FileName,
                    TestStatus = unit.IsFail ? "Fail" : "Pass",
                    SfcResponse = unit.SfcResponse?.ResponseType.ToTranslatedString(),
                    CreatedAt = unit.CreatedAt.ToString("dd/MM/yyyy"),
                    IconKind = unit.IsFail ? "AlertCircleOutline" : "CheckCircleOutline"
                });
            }
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

                foreach (var log in Logs)
                {
                    await writer.WriteLineAsync(
                        $"{log.Id},{log.SerialNumber},{log.Filename},{log.TestStatus},{log.SfcResponse},{log.CreatedAt}");
                }
            }

            Messenger.Send(new ShowToastMessage("Success", "Export Success: " + filePath, NotificationType.Success));
        }

        [RelayCommand]
        private async Task Edit()
        {
            await EditFile();
        }

        [RelayCommand]
        private async Task ReSend()
        {
            await ReSendFile();
        }

        [RelayCommand]
        private async Task Refresh()
        {
            Logs.Clear();
            SerialNumberFilter = "";
            SelectedTestStatus = "All";
            await LoadLogsAsync();
        }

        [RelayCommand]
        private async Task Search()
        {
            Logs.Clear();
            var units = await _unitUnderTestRepository.GetAllUnits();
            if (!string.IsNullOrWhiteSpace(SerialNumberFilter))
            {
                units = units.Where(u => u.SerialNumber.ToLower().Contains(SerialNumberFilter.ToLower())).ToList();
            }

            if (!string.IsNullOrWhiteSpace(SelectedTestStatus) && SelectedTestStatus != "All")
            {
                bool isFail = SelectedTestStatus == "Fail";
                units = units.Where(u => u.IsFail == isFail).ToList();
            }

            if (SelectedSfcResponse != null)
            {
                units = units.Where(u => u.SfcResponse?.ResponseType.ToString() == SelectedSfcResponse).ToList();
            }

            foreach (var unit in units)
            {
                Logs.Add(new LogEntry
                {
                    Id = unit.Id,
                    SerialNumber = unit.SerialNumber,
                    Filename = unit.FileName,
                    TestStatus = unit.IsFail ? "Fail" : "Pass",
                    SfcResponse = unit.SfcResponse?.ResponseType.ToTranslatedString(),
                    CreatedAt = unit.CreatedAt.ToString("dd/MM/yyyy"),
                    IconKind = unit.IsFail ? "AlertCircleOutline" : "CheckCircleOutline"
                });
            }
        }
    }

    public class LogEntry
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string Filename { get; set; }
        public string TestStatus { get; set; }
        public string SfcResponse { get; set; }
        public string CreatedAt { get; set; }
        public string IconKind { get; set; }
    }
}