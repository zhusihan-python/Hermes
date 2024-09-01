using System;
using System.Collections.ObjectModel;
using Hermes.Models;
using Hermes.Types;
using Material.Icons;

namespace Hermes.Features.PackageId;

public class PackageIdViewModel : PageBase
{
    public ObservableCollection<Package> Packages { get; set; } = new()
    {
        new Package()
        {
            Id = "1234567890",
            Quantity = 10,
            QuantityUsed = 5,
            Opened = DateTime.Now
        }
    };

    public ObservableCollection<UnitUnderTest> UnitsUnderTest { get; set; } = new()
    {
        new UnitUnderTest()
        {
            Id = 1,
            FileName = "FileName",
            SerialNumber = "SerialNumber",
            CreatedAt = DateTime.Now
        }
    };

    public PackageIdViewModel() : base("Package Id", MaterialIconKind.PackageVariant, PermissionLevel.Level1, 3)
    {
        for (int i = 0; i < 100; i++)
        {
            this.UnitsUnderTest.Add(new UnitUnderTest()
            {
                Id = i,
                FileName = $"FileName{i}",
                SerialNumber = $"SerialNumber{i}",
                CreatedAt = DateTime.Now
            });
            
            this.Packages.Add(new Package()
            {
                Id = "1234567890",
                Quantity = 10,
                QuantityUsed = Random.Shared.Next(10),
                Opened = DateTime.Now
            });
        }
    }
}