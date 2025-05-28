using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Common;
using Hermes.Common.Extensions;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hermes.Features.Logs;

public partial class SlideManageTabViewModel : ViewModelBase
{
    [ObservableProperty] private SealStateType? _selectedSealState;
    [ObservableProperty] private SortStateType? _selectedSortState;
    [ObservableProperty] private TimeSpanType? _selectedTimeSpan = TimeSpanType.SevenDays;
    [ObservableProperty] private string _slideIdFilter = "";
    [ObservableProperty] private string _pathologyIdFilter = "";
    [ObservableProperty] private Doctor? _selectedDoctor;
    private List<Slide> OriginSlides = new List<Slide>();
    public RangeObservableCollection<Slide> SlideData { get; set; } = [];
    private List<Doctor> _availableDoctors = [];
    public List<Doctor> AvailableDoctors
    {
        get => _availableDoctors;
        set => SetProperty(ref _availableDoctors, value); // SetProperty is from ObservableObject
    }
    public static IEnumerable<SealStateType?> SealOptions => NullableExtensions.GetValues<SealStateType>();
    public static IEnumerable<SortStateType?> SortOptions => NullableExtensions.GetValues<SortStateType>();
    public static IEnumerable<StatusType?> StatusOptions => NullableExtensions.GetValues<StatusType>();
    public static IEnumerable<TimeSpanType?> TimeSpanOptions => NullableExtensions.GetValues<TimeSpanType>();

    private readonly SlideRepository _slideRepository;
    private readonly DoctorRepository _doctorRepository;

    public SlideManageTabViewModel(
        SlideRepository slideRepository,
        DoctorRepository doctorRepository
        )
    {
        _slideRepository = slideRepository;
        _doctorRepository = doctorRepository;
        _ = LoadDoctorAsync();
        _ = LoadSlidesAsync();
    }

    private async Task LoadSlidesAsync()
    {
        OriginSlides.Clear();
        SlideData.Clear();
        OriginSlides.AddRange(await _slideRepository.FindAll());
        SlideData.AddRange(await _slideRepository.FindAll());
    }

    private async Task LoadDoctorAsync()
    {
        AvailableDoctors.Clear();
        AvailableDoctors.AddRange(await _doctorRepository.FindAll());
    }

    [RelayCommand]
    private async Task Refresh()
    {
        PathologyIdFilter = "";
        SlideIdFilter = "";
        SelectedDoctor = null;
        SelectedSealState = null;
        SelectedSealState = null;
        SelectedTimeSpan = null;
        await LoadSlidesAsync();
    }

    [RelayCommand]
    private async Task Search()
    {
        var slides = await _slideRepository.GetSlides(
            PathologyIdFilter,
            SlideIdFilter,
            SelectedDoctor,
            SelectedSealState,
            SelectedSortState,
            SelectedTimeSpan);

        OriginSlides.Clear();
        OriginSlides.AddRange(slides);
        SlideData.Clear();
        SlideData.AddRange(slides);
        await Task.Delay(100);
    }

    partial void OnSlideIdFilterChanged(string value)
    {
        ApplyFilterAndSort(); // Trigger filter/sort when filter text changes
    }

    partial void OnPathologyIdFilterChanged(string value)
    {
        ApplyFilterAndSort();
    }

    private void ApplyFilterAndSort()
    {
        IEnumerable<Slide> query = OriginSlides;

        // 1. Apply Filtering
        if (!string.IsNullOrWhiteSpace(PathologyIdFilter))
        {
            query = query.Where(slide =>
                slide.PathologyId.ToString().Contains(PathologyIdFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SlideIdFilter))
        {
            query = query.Where(slide =>
                slide.SlideId.ToString().Contains(SlideIdFilter, StringComparison.OrdinalIgnoreCase));
        }

        SlideData.Clear();
        SlideData.AddRange(query);
    }
}