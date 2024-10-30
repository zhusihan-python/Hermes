using System;
using Avalonia.Controls;
using ConfigFactory.Models;
using ConfigFactory;

namespace Hermes.Features.SettingsConfig;

public partial class SettingsView : Window
{
    public bool CanClose { get; set; }
    private bool _isClosed;

    public SettingsView()
    {
        InitializeComponent();
        this.Closed += (_, _) => this._isClosed = true;
    }

    public void Append(SettingsConfigModel settingsConfigModel)
    {
        if (ConfigPage.DataContext is ConfigPageModel model)
        {
            model.Append(settingsConfigModel);
        }
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