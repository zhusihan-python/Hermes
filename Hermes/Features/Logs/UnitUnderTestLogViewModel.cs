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
using Material.Icons;

namespace Hermes.Features.Logs
{
    public partial class UnitUnderTestLogViewModel : ViewModelBase
    {

        private readonly FileService _fileService;

        private readonly UnitUnderTestRepository _unitUnderTestRepository;

        public ObservableCollection<LogEntry> Logs { get; set; }
        public ObservableCollection<string> TestStatusOptions { get; set; }
        public ObservableCollection<SfcResponseType> SfcResponseOptions { get; set; }

        [ObservableProperty]
        private string _serialNumberFilter;

        [ObservableProperty]
        private string _selectedTestStatus;

        [ObservableProperty]
        private string _selectedSfcResponse;

        [ObservableProperty]
        private LogEntry _selectedLogEntry;

        public UnitUnderTestLogViewModel(
            UnitUnderTestRepository unitUnderTestRepository, 
            FileService fileService)
        {
            SerialNumberFilter = "";
            SelectedTestStatus = "All";
            //SelectedTestStatus = SfcResponseType.Ok.ToTranslatedString();

            _fileService = fileService;

            InitializeDataAsync(unitUnderTestRepository);

            _unitUnderTestRepository = unitUnderTestRepository;

            Logs = new ObservableCollection<LogEntry>();

            TestStatusOptions = new ObservableCollection<string>
            {
                "Pass",
                "Fail",
                "All"
            };

            SfcResponseOptions = new ObservableCollection<SfcResponseType>(Enum.GetValues(typeof(SfcResponseType)) as SfcResponseType[]);

            LoadLogsAsync().ConfigureAwait(false);
        }

        

        public async Task InitializeDataAsync(UnitUnderTestRepository repository)
        {
            await repository.AddExampleDataAsync();
        }

        public async Task EditFile()
        {
            //var fullpath = GetFullPathBySelectedFilename();
            var FullPath = @"C:\Users\hesbon_torres\Documents\ITEMS\fail\0712230436364105.3dx";
            //fileService.OpenWithDefaultApp(FullPath);
            new Process
            {
                StartInfo = new ProcessStartInfo(FullPath)
                {
                    UseShellExecute = true
                }
            }.Start();
        }

        public string GetFullPathBySelectedFilename()
        {
            var Filename = SelectedLogEntry?.Filename.ToString();
            if (Filename == null)
            {
                Debug.WriteLine("No se selecciono nada");
            }
            //var fullpath = _fileService.GetBackupFullPathByName(Filename);
            return "";
        }

        private void ReSendFile()
        {
            var fullpath = GetFullPathBySelectedFilename();
            if (fullpath != null)
            {
                Debug.WriteLine($"Selected Filename: {fullpath}");
            }
            //Agregar Funcion Reenviar el archivo

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
            //Falta Crear
        }

        [RelayCommand]
        private async Task Edit()
        {
            //Falta Crear, solo hice un ejemplo.
            await EditFile();
        }

        [RelayCommand]
        private async Task ReSend()
        {
            //Falta Crear, solo se verifico que retornara la ruta  y name.
            ReSendFile();
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
