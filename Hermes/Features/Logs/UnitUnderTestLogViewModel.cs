using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Common.Extensions;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.Common;

namespace Hermes.Features.Logs;

public partial class UnitUnderTestLogViewModel : ViewModelBase
{
    [ObservableProperty] private RecordType? _selectedRecordType;
    [ObservableProperty] private RecordStatusType? _selectedRecordStatus;
    [ObservableProperty] private TimeSpanType? _selectedTimeSpan = TimeSpanType.OneDay;
    public RangeObservableCollection<Record> ReCordData { get; set; } = [];
    public static IEnumerable<RecordType?> RecordTypeOptions => NullableExtensions.GetValues<RecordType>();
    public static IEnumerable<RecordStatusType?> StatusOptions => NullableExtensions.GetValues<RecordStatusType>();
    public static IEnumerable<TimeSpanType?> TimeSpanOptions => NullableExtensions.GetValues<TimeSpanType>();

    private readonly RecordRepository _recordRepository;

    public UnitUnderTestLogViewModel(
        RecordRepository recordRepository)
    {
        _recordRepository = recordRepository;
        _ = LoadRecordsAsync();
    }

    private async Task LoadRecordsAsync()
    {
        ReCordData.Clear();
        ReCordData.AddRange(await _recordRepository.FindAll());
    }

    [RelayCommand]
    private async Task Refresh()
    {
        SelectedRecordType = null;
        SelectedRecordStatus = null;
        SelectedTimeSpan = TimeSpanType.OneDay;
        await LoadRecordsAsync();
    }

    [RelayCommand]
    private async Task Search()
    {
        var records = await _recordRepository.GetRecords(
            SelectedRecordType,
            SelectedRecordStatus,
            SelectedTimeSpan);

        ReCordData.Clear();
        ReCordData.AddRange(records);
    }
}