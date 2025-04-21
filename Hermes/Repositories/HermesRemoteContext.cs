using Hermes.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Hermes.Repositories;

public class HermesRemoteContext : DbContext
{
    //private const string DatabaseName = "hermes";
    //private const string User = "hermes";
    //private const string Password = "AmazingPassword";
    //private string _server = "localhost";

    private string ConnectionString => $"Filename=dbSqlite.db";
    public DbSet<User> Users { get; set; }
    public DbSet<FeaturePermission> FeaturePermissions { get; set; }

//    public HermesRemoteContext(Settings settings)
//    {
////#if !DEBUG
////        _server = settings.DatabaseServer;
////#endif
//    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(connectionString: ConnectionString);
        base.OnConfiguring(optionsBuilder);
    }

    public void Migrate()
    {
        try
        {
            if (Database.GetPendingMigrations().Any())
            {
                Database.Migrate();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<FeaturePermission>().ToTable("FeaturePermissions");
    }
}