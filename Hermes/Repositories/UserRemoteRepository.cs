using Hermes.Cipher.Types;
using Hermes.Models;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Repositories;

public class UserRemoteRepository(IDbContextFactory<HermesRemoteContext> context)
{
    public async Task<IEnumerable<User>> FindAll(DepartmentType department, UserLevel sessionUserLevel)
    {
        await using var ctx = await context.CreateDbContextAsync();
        return await FindAllUsersQuery(ctx, department, sessionUserLevel).ToListAsync();
    }

    public async Task<IEnumerable<User>> FindById(string searchEmployeeId, DepartmentType department,
        UserLevel sessionUserLevel)
    {
        await using var ctx = await context.CreateDbContextAsync();
        return await FindAllUsersQuery(ctx, department, sessionUserLevel)
            .Where(x => x.EmployeeId.Contains(searchEmployeeId))
            .ToListAsync();
    }

    private IQueryable<User> FindAllUsersQuery(HermesRemoteContext ctx, DepartmentType department,
        UserLevel sessionUserLevel)
    {
        return ctx.Users
            .Where(x => department == DepartmentType.Admin ||
                        (x.Department == department && x.Level < sessionUserLevel));
    }

    public async Task<int> UpdateUser(User user)
    {
        try
        {
            await using var ctx = await context.CreateDbContextAsync();
            var entity = await ctx.Users.FirstOrDefaultAsync(x => x.EmployeeId == user.EmployeeId);
            if (entity == null)
            {
                return 0;
            }

            ctx.Users.Entry(entity).CurrentValues.SetValues(user);
            await ctx.SaveChangesAsync();
            return 1;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public void Delete(User user)
    {
        using var ctx = context.CreateDbContext();
        ctx.Users.Remove(user);
        ctx.SaveChanges();
    }

    public async Task<User> FindUser(string userName, string password)
    {
        await using var ctx = await context.CreateDbContextAsync();
        var user = await ctx.Users
            .Where(x => x.EmployeeId == userName && x.Password == password)
            .FirstOrDefaultAsync();
        if (user == null) return User.Null;
        user.Permissions = await ctx.FeaturePermissions
            .Where(x => x.Department <= user.Department && x.Level <= user.Level).ToListAsync();
        return user;
    }

    public async Task AddAndSaveAsync(User user)
    {
        await using var ctx = await context.CreateDbContextAsync();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();
    }
}