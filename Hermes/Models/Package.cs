using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Common.Extensions;

namespace Hermes.Models;

public partial class Package : ObservableObject
{
    public static readonly Package Null = new NullPackage();

    public string Id { get; set; } = "";
    public string Line { get; set; } = "";
    public int Quantity { get; set; }
    public int QuantityUsed { get; set; }
    public string HhPartNumber { get; set; } = "";
    public string SupplierPartNumber { get; set; } = "";
    public string DateCode { get; set; } = "";
    public string Lot { get; set; } = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsValid))]
    private string _vendor = "";

    public string WorkOrder { get; set; } = "";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanLoad))]
    private DateTime? _openedAt;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanLoad))]
    private DateTime? _loadedAt;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanLoad))]
    private DateTime? _scannedAt;

    public DateTime? LastUsedAt { get; set; }
    public DateTime? UsedAt => QuantityUsed >= Quantity ? LastUsedAt : null;

    public bool IsNull => this == Null;
    public bool CanLoad => ScannedAt != null && OpenedAt != null && LoadedAt == null;
    public bool HasSfcOpen => ScannedAt != null && OpenedAt != null;

    public static string NormalizePkgId(string pkgid)
    {
        return pkgid.ExtractFirstLetter('S');
    }

    public static string NormalizeWorkOrder(string workOrder)
    {
        return workOrder.PadLeft(12, '0');
    }

    public bool IsValid => !string.IsNullOrWhiteSpace(Id)
                           && !string.IsNullOrWhiteSpace(HhPartNumber)
                           && !string.IsNullOrWhiteSpace(SupplierPartNumber)
                           && !string.IsNullOrWhiteSpace(DateCode)
                           && !string.IsNullOrWhiteSpace(Lot)
                           && !string.IsNullOrWhiteSpace(Vendor);

    public string NormalizedId => NormalizePkgId(Id);
    public bool IsInUse { get; set; }

    public override string ToString()
    {
        return $"P{HhPartNumber},Q{Quantity},M{SupplierPartNumber},D{DateCode},L{Lot},S{Id},{Vendor}";
    }
}

public class NullPackage : Package
{
}