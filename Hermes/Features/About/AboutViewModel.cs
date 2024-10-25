using Hermes.Language;
using Hermes.Types;
using Material.Icons;
using System.Reflection;

namespace Hermes.Features.About;

public class AboutViewModel()
    : PageBase(
        Resources.txt_about,
        MaterialIconKind.InfoOutline,
        PermissionType.FreeAccess,
        100)
{
    public string Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
}