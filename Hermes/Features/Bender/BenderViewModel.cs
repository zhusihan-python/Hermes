using Material.Icons;
using R3;

namespace Hermes.Features.Bender;

public class BenderViewModel : PageBase
{
    public ParamSettingsViewModel ParamSettingsViewModel { get; set; }

    public BenderViewModel(ParamSettingsViewModel paramSettingsViewModel) : base(
        "…Ë÷√",
        MaterialIconKind.CogOutline,
        97)
    {
        this.ParamSettingsViewModel = paramSettingsViewModel;
        this.IsActive = true;
    }

    protected override void SetupReactiveExtensions()
    {

    }
}