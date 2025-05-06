using Hermes.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
//using System.Reflection;

namespace Hermes.Repositories;

public class HermesLocalContext : DbContext
{
    protected string ConnectionString { get; init; } = "Filename=dbSqlite.db";

    public DbSet<User> Users { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Slide> Slides { get; set; }
    public DbSet<Heartbeat> Heartbeats { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(connectionString: ConnectionString);
        base.OnConfiguring(optionsBuilder);
    }

    public void Migrate()
    {
#if DEBUG
        //Database.EnsureDeleted();
#endif
        // if (Database.GetPendingMigrations().Any())
        // {
        //     Database.Migrate();
        // }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Doctor>().ToTable("Doctors");
        modelBuilder.Entity<Slide>().ToTable("Slides");
    }
}