using Avalonia.Controls;
using System;

namespace Hermes.Features.Bender;

public partial class PackageScannerView : UserControl
{
    public PackageScannerView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        var vm = (PackageScannerViewModel)DataContext!;
        vm.InstructionsChanged += OnInstructionsChanged;
    }

    private void OnInstructionsChanged(string instruction)
    {
        if (instruction == Hermes.Language.Resources.msg_scan_2d_package)
        {
            PackageCodeTextBox.Focus();
        }
        else if (instruction == Hermes.Language.Resources.msg_scan_vendor)
        {
            VendorTextBox.Focus();
        }
    }
}