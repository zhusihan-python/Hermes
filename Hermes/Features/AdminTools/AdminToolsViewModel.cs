using Hermes.Language;
using Material.Icons;

namespace Hermes.Features.AdminTools;

public class AdminToolsViewModel(FeaturePermissionsViewModel featurePermissionsViewModel) : PageBase(
    "����",
    MaterialIconKind.HelpCircle,
    98)
{
    public FeaturePermissionsViewModel FeaturePermissionsViewModel { get; } = featurePermissionsViewModel;
}