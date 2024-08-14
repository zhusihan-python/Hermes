using System;
using System.Threading.Tasks;
using Android.Content;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Cipher;
using Hermes.Cipher.Services;
using Hermes.Cipher.Types;
using Hermes.TokenGen.Common.Messages;
using Hermes.TokenGen.Models;

namespace Hermes.TokenGen.ViewModels;

public partial class TokenGenViewModel : ViewModelBase
{
    public User User { get; init; }
    [ObservableProperty] private string _employeeNumber = "";
    [ObservableProperty] private DateTimeOffset _selectedDate;
    [ObservableProperty] private string _token = string.Empty;
    public bool IsDesktop=> App.IsDesktop;

    private readonly TokenGenerator _tokenGenerator;

    public TokenGenViewModel(User user)
    {
        User = user;
        EmployeeNumber = user.EmployeeNumber;
        _tokenGenerator = new TokenGenerator();
        _selectedDate = DateTimeOffset.Now;
        GenerateToken();
    }

    [RelayCommand]
    private void GenerateToken()
    {
        if (int.TryParse(EmployeeNumber, out var employeeNumber))
        {
            var inTargetZone = TimeZoneInfo.ConvertTime(SelectedDate, TimeZoneInfo.Local);
            Token = _tokenGenerator.Generate(
                employeeNumber,
                (int)User.Department,
                DateOnly.FromDateTime(inTargetZone.Date));
            _tapCount = 0;
        }
    }

    private int _tapCount = 0;

    [RelayCommand]
    private void IconTapped(TappedEventArgs _)
    {
        _tapCount += 1;
        if (_tapCount <= 15) return;
        Messenger.Send(new NavigateMessage(new RegisterViewModel()));
    }

    [RelayCommand]
    private void Share()
    {
        try
        {
            var shareIntent = new Intent(Intent.ActionSend);
            shareIntent.SetType("text/plain");
            shareIntent.PutExtra(Android.Content.Intent.ExtraText,
                $"Hermes Token: {Token}\nDate: {SelectedDate:yyyy MM dd}");
            shareIntent.PutExtra(Android.Content.Intent.ExtraSubject, "Token");
            Android.App.Application.Context.StartActivity(shareIntent);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}