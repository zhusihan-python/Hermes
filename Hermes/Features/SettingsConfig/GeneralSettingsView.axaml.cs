using Avalonia.Controls;
using ConfigFactory.Models;
using ConfigFactory;

namespace Hermes.Features.SettingsConfig;

public partial class GeneralSettingsView : Window
{
    public bool CanClose { get; set; }

    public GeneralSettingsView()
    {
        InitializeComponent();
        Closing += (_, args) =>
        {
            if (CanClose) return;
            this.Hide();
            args.Cancel = true;
        };
    }

    public void Append(GeneralSettingsConfigModel generalSettingsConfigModel)
    {
        if (ConfigPage.DataContext is ConfigPageModel model)
        {
            model.Append(generalSettingsConfigModel);
        }
    }

    public void ForceClose()
    {
        this.CanClose = true;
        this.Close();
    }
}