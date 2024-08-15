using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Hermes.Cipher.Services;
using Hermes.Cipher;
using Hermes.TokenGen.Models;
using System.Collections.Generic;
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

    public MultipleUserTokenGenViewModel(User manager)
    {
        _manager = manager;
        _tokenGenerator = new TokenGenerator();
        SelectedDate = DateTimeOffset.Now;
        var subUsers = FileService.ReadJsonEncrypted<List<SubUser>>(App.SubUsersFullpath);
        if (subUsers is not null)
        {
            SubUsers.AddRange(subUsers);
        }

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
        await FileService.WriteJsonEncryptedAsync(App.SubUsersFullpath, SubUsers.ToList());
    }

    [RelayCommand]
    private async Task ExportToCsv()
    {
        if (App.StorageProvider?.CanPickFolder != true) return;
        var options = new FolderPickerOpenOptions();
        var folder = await App.StorageProvider.OpenFolderPickerAsync(options);
        if (folder.Count <= 0) return;
        var csv = "Employee Name, Employee Number, Department, Token\n";
        csv += SubUsers
            .Select(subUser => $"{subUser.Name},{subUser.Number},{subUser.Department},{subUser.Token}")
            .Aggregate((a, b) => $"{a}\n{b}");
        await FileService.WriteAllTextAsync(
            folder[0].Path.AbsolutePath + @$"\hermes_users_tokens_{DateTime.Now:yyyy_MM_dd_mm_ss}.csv",
            csv);
    }
}