﻿using System;
using CommunityToolkit.Mvvm.ComponentModel;
//using System.Linq;
using System.Threading.Tasks;
using Hermes.Types;
using Hermes.Repositories;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Hermes.Models;

namespace Hermes.Features.UutProcessor;

public partial class SlideBoxViewModel : ViewModelBase
{
    private int _rowIndex = 1;
    private int _columnIndex = 1;
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

    [ObservableProperty] private string _programId;
    [ObservableProperty] private string _pathologyId;
    [ObservableProperty] private string _slideId;
    [ObservableProperty] private string _patientName;
    [ObservableProperty] private string _doctorName;
    [ObservableProperty] private string _entryDate;
    [ObservableProperty] private bool _boxInPlace;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isBusy;
    public ObservableCollection<SlideModel> ItemList { get; set; } = new ObservableCollection<SlideModel>();
    //public RangeObservableCollection<Slide> Slides { get; set; } = [];
    //private readonly SlideRepository _slideRepository;
    public SlideBoxViewModel(SlideRepository slideRepository)
    {
        //this._slideRepository = slideRepository;
        for (int i = 0; i < 20; i++)
        {
            ItemList.Add(new SlideModel
            {
                State = SlideState.Empty,
            });
        }
        Console.WriteLine("Operation completed");
    }

    //public Observable<Unit> LoadSlidesObservable()
    //{
    //    return Observable.FromAsync(async (cancellationToken) =>
    //    {
    //        Slides.Clear();
    //        var ids = new List<string> { "24001124","24001125","24001126","24001127","24001128","24001129","24001130",
    //                    "24001131","24001132","24001133","24001134","24001135","24001136","24001137","24001138",
    //                    "24001139","24001140", "24001141","24001142","24001143" };
    //        var result = await _slideRepository.FindBySlideIds(ids);
    //        Slides.AddRange(result);
    //    });
    //}

    //public void ExecuteLoadSlides()
    //{
    //    LoadSlidesObservable().Subscribe(
    //        _ => Console.WriteLine("Operation completed")
    //    );
    //}

    protected override void SetupReactiveExtensions()
    {

    }

    [RelayCommand]
    private async Task Refresh()
    {
        //this.ProgramId = Slides[SelectedSlideIndex].ProgramId;
        //this.PathologyId = Slides[SelectedSlideIndex].PathologyId.ToString();
        //this.SlideId = Slides[SelectedSlideIndex].SlideId.ToString();
        //this.PatientName = Slides[SelectedSlideIndex].PatientName;
        //this.DoctorName = Slides[SelectedSlideIndex].Doctor.Name;
        //this.EntryDate = Slides[SelectedSlideIndex].EntryDate;
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

    public void UpdateSlide(int slideIndex, Slide slide)
    {
        if (slideIndex < 0 ||  slideIndex >= ItemList.Count)
        {
            return;
        }
        ItemList[slideIndex].Slide = slide;
    }
}
