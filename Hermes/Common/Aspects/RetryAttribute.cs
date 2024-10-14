using System;
using System.Threading;
using System.Threading.Tasks;
using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Aspects;

namespace Hermes.Common.Aspects;

/// <summary>
/// This class is used to retry methods that may fail.
/// </summary>
public class RetryAttribute : OverrideMethodAspect

{
    [IntroduceDependency] private ILogger? _logger;

    /// <summary>
    /// Gets or sets the maximum number of times that the method should be executed.
    /// </summary>
    public int Attempts { get; set; } = 3;

    /// <summary>
    /// Gets or set the delay, in ms, to wait between the first and the second attempt.
    /// The delay is doubled at every further attempt.
    /// </summary>
    public double Delay { get; set; } = 100;

    public override dynamic? OverrideMethod()
    {
        for (var i = 0;; i++)
        {
            try
            {
                return meta.Proceed();
            }
            catch (Exception e) when (i < this.Attempts)
            {
                var delay = this.Delay * Math.Pow(2, i + 1);
                this._logger?.Warn(e.Message + $" Waiting {delay} ms.");
                Thread.Sleep((int)delay);
            }
        }
    }

    public override async Task<dynamic?> OverrideAsyncMethod()
    {
        for (var i = 0;; i++)
        {
            try
            {
                return await meta.ProceedAsync();
            }
            catch (Exception e) when (i < this.Attempts)
            {
                var delay = this.Delay * Math.Pow(2, i + 1);
                this._logger?.Warn(e.Message + $" Waiting {delay} ms.");
                await Task.Delay((int)delay);
            }
        }
    }
}