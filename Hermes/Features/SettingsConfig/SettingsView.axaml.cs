using Avalonia.Controls;
using ConfigFactory.Models;
using ConfigFactory;

namespace Hermes.Features.SettingsConfig;

public partial class SettingsView : Window
{
    public bool CanClose { get; set; }

    public SettingsView()
    {
        InitializeComponent();
        Closing += (_, args) =>
        {
            if (CanClose) return;
            this.Hide();
            args.Cancel = true;
        };
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
        this.CanClose = true;
        this.Close();
    }
}