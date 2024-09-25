using System;
using System.Threading.Tasks;
using Hermes.Cipher;
using Hermes.Cipher.Types;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Repositories;

public class UserRepository
{
    private readonly TokenGenerator _tokenGenerator;

    public UserRepository(TokenGenerator tokenGenerator)
    {
        _tokenGenerator = tokenGenerator;
    }

    public async Task<User> GetUser(string userName, string password)
    {
        // TODO

        return User.Null;
    }
}