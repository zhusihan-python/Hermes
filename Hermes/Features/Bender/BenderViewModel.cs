using Hermes.Cipher.Types;
using Hermes.Types;
using Material.Icons;

namespace Hermes.Features.Bender;

public partial class BenderViewModel : PageBase
{
    public PackageTrackingViewModel PackageTrackingViewModel { get; set; }
    public PackageScannerViewModel PackageScannerViewModel { get; set; }

    public BenderViewModel(PackageTrackingViewModel packageTrackingViewModel,
        PackageScannerViewModel packageScannerViewModel) : base(
        "Bender",
        MaterialIconKind.Qrcode,
        FeatureType.Bender,
        3,
        [StationType.Labeling, StationType.LabelingMachine])
    {
        this.PackageTrackingViewModel = packageTrackingViewModel;
        this.PackageScannerViewModel = packageScannerViewModel;
        this.PackageScannerViewModel.PackageScanned += async (_) => await this.PackageTrackingViewModel.DataReload();
    }
}