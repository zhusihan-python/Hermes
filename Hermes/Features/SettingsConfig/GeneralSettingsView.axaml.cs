using Avalonia.Controls;
using ConfigFactory.Models;
using ConfigFactory;

namespace Hermes.Features.SettingsConfig;

public partial class GeneralSettingsView : Window
{
    public GeneralSettingsView()
    {
        InitializeComponent();
        if (ConfigPage.DataContext is ConfigPageModel model)
        {
            model.Append<GeneralSettingsViewModel>();
        }
    }
}