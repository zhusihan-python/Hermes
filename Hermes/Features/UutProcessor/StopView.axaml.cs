using System;
using Avalonia.Controls;
using SukiUI.Controls;

namespace Hermes.Features.UutProcessor;

public partial class StopView : SukiWindow
{
    public bool CanClose { get; set; }
    private bool _isClosed;

    public StopView()
    {
        InitializeComponent();
        this.Closing += (_, args) => args.Cancel = !CanClose;
        this.Closed += (_, _) => this._isClosed = true;
#if DEBUG
        this.Topmost = false;
        this.CanClose = true;
#endif
    }

    public void ForceClose()
    {
        try
        {
            if (this._isClosed) return;
            this.CanClose = true;
            this.Close();
        }
        catch (Exception)
        {
            // ignored
        }
    }
}