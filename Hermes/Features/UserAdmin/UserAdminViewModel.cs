using System;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Material.Icons;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Hermes.Features.UserAdmin;

public partial class UserAdminViewModel : PageBase
{
    public ObservableCollection<User> Users { get; } = [];
    private readonly UserRepository _userRepository;

    public UserAdminViewModel(UserRepository userRepository)
        : base(
            "User Admin",
            MaterialIconKind.Users,
            FeatureType.UserAdmin,
            1,
            [StationType.Labeling]
        )
    {
        _userRepository = userRepository;
    }

    [RelayCommand]
    private void FindAllUsers()
    {
        // TODO
        throw new NotImplementedException();
    }

    [RelayCommand]
    private void AddUser()
    {
        // TODO
        throw new NotImplementedException();
    }

    [RelayCommand]
    private void RemoveUser(object? value)
    {
        // TODO
        throw new NotImplementedException();
    }

    [RelayCommand]
    private void UserEdited(object? parameter)
    {
        // TODO
        throw new NotImplementedException();
    }

    [RelayCommand]
    private async Task ExportToCsv()
    {
        // TODO
        throw new NotImplementedException();
        // await _userRepository.SaveSubUsersToCsv(folder[0].Path.AbsolutePath, SubUsers.ToList());
    }
}