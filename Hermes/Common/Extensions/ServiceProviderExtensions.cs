using Avalonia.Controls;
using Hermes.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Hermes.Common.Extensions;

public static class ServiceProviderExtensions
{
    public static Window? BuildWindow<TViewModel>(this ServiceProvider provider, bool isActive)
        where TViewModel : ViewModelBase
    {
        var viewModel = provider.GetRequiredService<TViewModel>();
        viewModel.IsActive = isActive;
        return provider.GetRequiredService<ViewLocator>().BuildWindow(viewModel);
    }
}