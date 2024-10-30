using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace Hermes.Features.UutProcessor;

public partial class SuccessView : Window
{
    public bool CanClose { get; set; }
    private bool _isClosed;

    public SuccessView()
    {
        InitializeComponent();
        this.Closed += (_, _) => this._isClosed = true;
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        this.Hide();
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