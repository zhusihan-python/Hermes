using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Hermes.Models;
using Hermes.Types;
using Material.Icons;

namespace Hermes.Features.Bender;

public partial class BenderViewModel : PageBase
{
    [ObservableProperty] private bool _isDateFilter = true;
    [ObservableProperty] private DateTime _fromDate = DateTime.Now.AddDays(-1);
    [ObservableProperty] private DateTime _toDate = DateTime.Now;

    public ObservableCollection<Package> Packages { get; set; } = [];

    public BenderViewModel() : base("Bender", MaterialIconKind.Qrcode, PermissionLevel.Level1, 4)
    {
    }

    [RelayCommand]
    private void FindByDate()
    {
        var packages = new List<Package>();
        foreach (var i in Enumerable.Range(0, 10))
        {
            packages.Add(new Package()
            {
                Id = "000007045008" + i,
                Quantity = i,
                QuantityUsed = i,
                Opened = i > 0 ? DateTime.Now : null,
                Loaded = i > 2 ? DateTime.Now : null,
                Used = i > 5 ? DateTime.Now : null,
            });
        }

        this.Packages.Clear();
        this.Packages.AddRange(packages);
    }

    [RelayCommand]
    private void Load(Package package)
    {
        package.Loaded = DateTime.Now;
    }
}