using System;
using System.Collections.Generic;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Types;
using Hermes.Common;
using Hermes.Models;
using Hermes.Repositories;
using Material.Icons;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using R3;
using System.Threading;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoxViewModel : ViewModelBase
{
    private int _rowIndex = 1;
    private int _columnIndex = 1;
    private int _selectedSlideIndex = -1;
    public int RowIndex 
    { 
        get { return _rowIndex; }
        set { SetProperty(ref _rowIndex, value); } 
    }
    public int ColumnIndex
    { 
        get { return _columnIndex; }
        set { SetProperty(ref _columnIndex, value); }
    }
    public int SelectedSlideIndex
    {
        get { return _selectedSlideIndex; }
        set { SetProperty(ref _selectedSlideIndex, value); }
    }
    [ObservableProperty] private string _programId;
    [ObservableProperty] private string _pathologyId;
    [ObservableProperty] private string _slideId;
    [ObservableProperty] private string _patientName;
    [ObservableProperty] private string _doctorName;
    [ObservableProperty] private string _entryDate;
    public List<SlideModel> ItemList { get; set; } = new List<SlideModel>();
    public RangeObservableCollection<Slide> Slides { get; set; } = [];
    private readonly SlideRepository _slideRepository;
    public SlideBoxViewModel()
    {
        var ids = new List<string>() {"24001124","24001125","24001126","24001127","24001128","24001129","24001130",
                        "24001131","24001132","24001133","24001134","24001135","24001136","24001137","24001138","24001139","24001140" };
        Random _random = new Random();
        SlideState[] states = Enum.GetValues(typeof(SlideState))
                              .Cast<SlideState>()
                              .ToArray();
        for (int i = 0; i < 20; i++)
        {
            // 随机选择一个 SlideState
            SlideState randomState = states[_random.Next(states.Length)];

            ItemList.Add(new SlideModel
            {
                State = randomState,
                RowIndex = i, // 可选：根据索引设置行号
            });
        }
    }

    [RelayCommand]
    private void ChangeRowIndex(int rowIndex)
    {
        if (SelectedSlideIndex >= 0)
        {
            ItemList[SelectedSlideIndex].IsSelected = false;
        }
        SelectedSlideIndex = rowIndex;
        ItemList[rowIndex].IsSelected = true;
        Refresh();
    }

    public IObservable<Unit> LoadSlidesObservable()
    {
        return (IObservable<Unit>)Observable.FromAsync(async (cancellationToken) =>
        {
            Slides.Clear();
            var ids = new List<string> { "24001124","24001125","24001126","24001127","24001128","24001129","24001130",
                        "24001131","24001132","24001133","24001134","24001135","24001136","24001137","24001138","24001139","24001140" };
            var result = await _slideRepository.FindByIds(ids);
            Slides.AddRange(result);
        });
    }

    public void ExecuteLoadSlides()
    {
        LoadSlidesObservable()
            .Subscribe(
                _ => Console.WriteLine("Slides loaded successfully"),
                ex => Console.WriteLine($"Error loading slides: {ex.Message}")
            );
    }

    protected override void SetupReactiveExtensions()
    {
        ExecuteLoadSlides();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        this.ProgramId = Slides[SelectedSlideIndex].ProgramId;
        this.PathologyId = Slides[SelectedSlideIndex].PathologyId.ToString();
        this.SlideId = Slides[SelectedSlideIndex].SlideId.ToString();
        this.PatientName = Slides[SelectedSlideIndex].PatientName;
        this.DoctorName = Slides[SelectedSlideIndex].Doctor.Name;
        this.EntryDate = Slides[SelectedSlideIndex].EntryDate;
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
