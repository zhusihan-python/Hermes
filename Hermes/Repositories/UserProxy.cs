using System.Collections.Generic;
using System.Threading.Tasks;
using Hermes.Cipher.Types;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Repositories;

public class UserProxy
{
    private readonly UserRemoteRepository _userRemoteRepository;

    public UserProxy(UserRemoteRepository userRemoteRepository)
    {
        this._userRemoteRepository = userRemoteRepository;
    }

    public async Task<IEnumerable<User>> FindAll(DepartmentType department, UserLevel sessionUserLevel)
    {
        return await _userRemoteRepository.FindAll(department, sessionUserLevel);
    }

    public async Task<IEnumerable<User>> FindById(string searchEmployeeId, DepartmentType department,
        UserLevel sessionUserLevel)
    {
        return await _userRemoteRepository.FindById(searchEmployeeId, department, sessionUserLevel);
    }

    public async Task<int> UpdateUser(User user)
    {
        return await _userRemoteRepository.UpdateUser(user);
    }

    public async Task Add(User user)
    {
        await _userRemoteRepository.AddAndSaveAsync(user);
    }

    public void Delete(User user)
    {
        _userRemoteRepository.Delete(user);
    }

    public async Task<User> FindUser(string userName, string password)
    {
        return await _userRemoteRepository.FindUser(userName, password);
    }
}