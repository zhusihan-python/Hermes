using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;
using System;
using Hermes.Services;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Hermes.Common.Aspects;

/// <summary>
/// This class is used to log exceptions that occur during method execution and show a toast, then return default.
/// </summary>
public class CatchExceptionAndShowErrorToastAttribute : OverrideMethodAspect
{
    [IntroduceDependency] private ILogger? _logger;
    [IntroduceDependency] private WindowService? _windowService;

    public override dynamic? OverrideMethod()
    {
        try
        {
            return meta.Proceed();
        }
        catch (Exception ex)
        {
            _logger?.Info($"{meta.Target.Method} | {ex.Message}");
            _windowService?.ShowErrorToast(ex.Message);
        }

        return default;
    }
}