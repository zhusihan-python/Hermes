using Hermes.Types;
using Material.Icons;

namespace Hermes.Features.Bender;

public partial class BenderViewModel : PageBase
{
    public PackageTrackingViewModel PackageTrackingViewModel { get; set; }
    public PackageScannerViewModel PackageScannerViewModel { get; set; }

    public BenderViewModel(PackageTrackingViewModel packageTrackingViewModel,
        PackageScannerViewModel packageScannerViewModel) : base(
        "Bender", MaterialIconKind.Qrcode,
        PermissionLevel.Level1, 4)
    {
        this.PackageTrackingViewModel = packageTrackingViewModel;
        this.PackageScannerViewModel = packageScannerViewModel;
    }
}