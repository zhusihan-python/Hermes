using Avalonia.Controls;
using R3;
using System;

namespace Hermes.Features.Bender;

public partial class PackageScannerView : UserControl
{
    public PackageScannerView()
    {
        InitializeComponent();
    }

    private DisposableBag _disposables;

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        var vm = (PackageScannerViewModel)DataContext!;
        vm.ObservePropertyChanged(x => x.PackageScanned)
            .Subscribe(UpdateInstruction)
            .AddTo(ref _disposables);
    }

    private void UpdateInstruction(string instruction)
    {
        if (instruction == Language.Resources.msg_scan_2d_package)
        {
            PackageCodeTextBox.Focus();
        }
        else if (instruction == Language.Resources.msg_scan_vendor)
        {
            VendorTextBox.Focus();
        }
    }
}