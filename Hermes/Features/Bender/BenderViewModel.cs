using Material.Icons;
using R3;

namespace Hermes.Features.Bender;

public class BenderViewModel : PageBase
{

    public BenderViewModel() : base(
        "…Ë÷√",
        MaterialIconKind.CogOutline,
        97)
    {
        this.IsActive = true;
    }

    protected override void SetupReactiveExtensions()
    {

    }
}