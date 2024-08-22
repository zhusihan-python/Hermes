using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Cipher.Services;
using Hermes.TokenGen.Models;

namespace Hermes.TokenGen.Repositories;

public class UserRepository
{
    public async Task SaveUser(User user)
    {
        await FileService.WriteJsonEncryptedAsync(App.ConfigFullpath, user);
    }

    public User GetUser()
    {
        try
        {
            if (FileService.FileExists(App.ConfigFullpath))
            {
                var user = FileService.ReadJsonEncrypted<User>(App.ConfigFullpath);
                if (user is not null)
                {
                    return user;
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return User.Null;
    }

    public void DeleteUser()
    {
        FileService.DeleteFile(App.ConfigFullpath);
    }

    public List<SubUser> GetSubUsers()
    {
        try
        {
            if (FileService.FileExists(App.SubUsersFullpath))
            {
                var subUsers = FileService.ReadJsonEncrypted<List<SubUser>>(App.SubUsersFullpath);
                if (subUsers is not null)
                {
                    return subUsers;
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return new List<SubUser>();
    }

    public async Task SaveSubUsers(List<SubUser> subUsers)
    {
        await FileService.WriteJsonEncryptedAsync(App.SubUsersFullpath, subUsers);
    }

    public async Task SaveSubUsersToCsv(string path, List<SubUser> subUsers)
    {
        var csv = "Employee Name, Employee Number, Department, Token\n";
        csv += subUsers
            .Select(subUser => $"{subUser.Name},{subUser.Number},{subUser.Department},{subUser.Token}")
            .Aggregate((a, b) => $"{a}\n{b}");
        await FileService.WriteAllTextAsync(
            path + @$"\hermes_users_tokens_{DateTime.Now:yyyy_MM_dd_mm_ss}.csv",
            csv);
    }
}