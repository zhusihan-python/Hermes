using Hermes.Language;
using Material.Icons;

namespace Hermes.Features.AdminTools;

public class AdminToolsViewModel(FeaturePermissionsViewModel featurePermissionsViewModel) : PageBase(
    Resources.txt_admin_tools,
    MaterialIconKind.HelpCircle,
    98)
{
    public FeaturePermissionsViewModel FeaturePermissionsViewModel { get; } = featurePermissionsViewModel;
}