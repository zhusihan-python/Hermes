using System;
using System.Threading;
using System.Threading.Tasks;
using Hermes.Models;
using Hermes.Services;
using Hermes.Types;
using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;

namespace Hermes.Common.Aspects;

/// <summary>
/// This class is used to validate if the current logged user has the permission to access the requested resource.
/// </summary>
public class ValidatePermissionAttribute : OverrideMethodAspect

{
    [IntroduceDependency] private Session? _session { get; set; }
    [IntroduceDependency] private WindowService? _windowService;

    /// <summary>
    /// Gets or sets the permission type.
    /// </summary>
    public int Permission { get; set; }


    public override dynamic? OverrideMethod()
    {
        // if (_session.HasUserPermission(Permission))
        // {
        //     // TODO: Show toast with error message
        //     return default;
        // }
        //
        // return meta.Proceed();
        return default;
    }
}