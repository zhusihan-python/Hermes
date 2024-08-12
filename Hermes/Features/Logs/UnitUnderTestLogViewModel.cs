using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Hermes.Features.Logs
{
    public partial class UnitUnderTestLogViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<LogEntry> Logs { get; set; }
        public ObservableCollection<string> TestStatusOptions { get; set; }
        public ObservableCollection<string> SfcResponseOptions { get; set; }

        private string _serialNumberFilter;
        public string SerialNumberFilter
        {
            get => _serialNumberFilter;
            set
            {
                _serialNumberFilter = value;
                OnPropertyChanged(nameof(SerialNumberFilter));
            }
        }

        private string _selectedTestStatus;
        public string SelectedTestStatus
        {
            get => _selectedTestStatus;
            set
            {
                _selectedTestStatus = value;
                OnPropertyChanged(nameof(SelectedTestStatus));
            }
        }

        private string _selectedSfcResponse;
        public string SelectedSfcResponse
        {
            get => _selectedSfcResponse;
            set
            {
                _selectedSfcResponse = value;
                OnPropertyChanged(nameof(SelectedSfcResponse));
            }
        }

        public UnitUnderTestLogViewModel()
        {
            Logs = new ObservableCollection<LogEntry>
            {
                new LogEntry { SerialNumber = "1A65ASDF", Filename = "1A65.3dx", TestStatus = "Pass", SfcResponse = "Ok", CreatedAt = "21/07/2024", IconKind = "CheckCircleOutline" },
                new LogEntry { SerialNumber = "1AR2R43", Filename = "1AR43.3DX", TestStatus = "Pass", SfcResponse = "Wrong Station", CreatedAt = "21/04/2024", IconKind = "AlertCircleOutline" },
            };

            TestStatusOptions = new ObservableCollection<string>
            {
                "Pass",
                "Fail",
                "All"
            };

            SfcResponseOptions = new ObservableCollection<string>
            {
                "Ok",
                "Wrong Station",
                "Error",
                "All"
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LogEntry
    {
        public string SerialNumber { get; set; }
        public string Filename { get; set; }
        public string TestStatus { get; set; }
        public string SfcResponse { get; set; }
        public string CreatedAt { get; set; }
        public string IconKind { get; set; }
    }
}
