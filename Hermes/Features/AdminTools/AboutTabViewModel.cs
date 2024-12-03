using System.Reflection;

namespace Hermes.Features.AdminTools;

public partial class AboutTabViewModel : ViewModelBase
{
    public string Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

    public AboutTabViewModel () { }
}
