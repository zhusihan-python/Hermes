namespace Hermes.Features.SettingsConfig;

public class SettingsViewModel : ViewModelBase
{
    public SettingsConfigModel Model { get; init; }

    public SettingsViewModel(SettingsConfigModel model)
    {
        this.Model = model;
    }
}