using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Cipher;
using Hermes.Common.Extensions;
using Hermes.Types;
using System;

namespace Hermes.TokenGen.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private int _id = 112530;
    [ObservableProperty] private DepartmentType _department = DepartmentType.Ee;
    [ObservableProperty] private string _departmentText = string.Empty;
    [ObservableProperty] private DateTimeOffset _selectedDate;
    [ObservableProperty] private string _token = string.Empty;
    private readonly TokenGenerator _tokenGenerator;

    public MainViewModel()
    {
        _tokenGenerator = new TokenGenerator();
        _selectedDate = DateTimeOffset.Now;
        DepartmentText = Department.ToTranslatedString();
        GenerateToken();
    }

    [RelayCommand]
    private void GenerateToken()
    {
        var inTargetZone = TimeZoneInfo.ConvertTime(SelectedDate, TimeZoneInfo.Local);
        Token = _tokenGenerator.Generate(Id, 1, DateOnly.FromDateTime(inTargetZone.Date));
    }
}