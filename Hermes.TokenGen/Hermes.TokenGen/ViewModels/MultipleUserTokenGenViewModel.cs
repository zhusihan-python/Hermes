using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Hermes.Cipher;
using Hermes.TokenGen.Models;
using Hermes.TokenGen.Repositories;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.TokenGen.ViewModels;

public partial class MultipleUserTokenGenViewModel : ViewModelBase
{
    public ObservableCollection<SubUser> SubUsers { get; } = [];
    [ObservableProperty] private DateTimeOffset _selectedDate;
    private readonly User _manager;
    private readonly TokenGenerator _tokenGenerator;
    private readonly UserRepository _userRepository;

    public MultipleUserTokenGenViewModel(User manager)
    {
        _manager = manager;
        _tokenGenerator = new TokenGenerator();
        _userRepository = new UserRepository();
        SelectedDate = DateTimeOffset.Now;
        var subUsers = _userRepository.GetSubUsers();
        SubUsers.AddRange(subUsers);
        GenerateTokens();
    }

    [RelayCommand]
    private void GenerateTokens()
    {
        foreach (var subUser in SubUsers)
        {
            subUser.Token = CalculateToken(subUser);
        }
    }

    [RelayCommand]
    private void AddUser()
    {
        this.SubUsers.Insert(0, new SubUser(_manager));
    }

    [RelayCommand]
    private void RemoveUser(object? value)
    {
        if (value is SubUser subUser)
        {
            this.SubUsers.Remove(subUser);
        }
    }

    [RelayCommand]
    private void UserEdited(object? parameter)
    {
        if (parameter is SubUser subUser)
        {
            subUser.Token = CalculateToken(subUser);
        }
    }

    private string CalculateToken(SubUser subUser)
    {
        if (!int.TryParse(subUser.Number, out var id)) return "";
        var inTargetZone = TimeZoneInfo.ConvertTime(SelectedDate, TimeZoneInfo.Local);
        return _tokenGenerator.Generate(id, (int)subUser.Department, DateOnly.FromDateTime(inTargetZone.Date));
    }

    [RelayCommand]
    private async Task Closed()
    {
        await _userRepository.SaveSubUsers(SubUsers.ToList());
    }

    [RelayCommand]
    private async Task ExportToCsv()
    {
        if (App.StorageProvider?.CanPickFolder != true) return;
        var options = new FolderPickerOpenOptions();
        var folder = await App.StorageProvider.OpenFolderPickerAsync(options);
        if (folder.Count <= 0) return;
        await _userRepository.SaveSubUsersToCsv(folder[0].Path.AbsolutePath, SubUsers.ToList());
    }
}