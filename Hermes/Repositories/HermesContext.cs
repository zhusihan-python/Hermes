using System.Linq;
using System.Reflection;
using Hermes.Models;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Repositories;

public class HermesContext : DbContext
{
    private const string DbName = "dbSqlite.db";

    public DbSet<UnitUnderTest> UnitsUnderTest { get; set; }
    public DbSet<Defect> Defects { get; set; }
    public DbSet<SfcResponse> SfcResponses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            connectionString: "Filename=" + DbName,
            sqliteOptionsAction: op => { op.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName); });
        base.OnConfiguring(optionsBuilder);
    }

    public void Initialize()
    {
        if (Database.GetPendingMigrations().Any())
        {
            Database.Migrate();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UnitUnderTest>().ToTable("UnitsUnderTest");
        modelBuilder.Entity<Defect>().ToTable("Defects");
        modelBuilder.Entity<SfcResponse>().ToTable("SfcResponses");
    }
}