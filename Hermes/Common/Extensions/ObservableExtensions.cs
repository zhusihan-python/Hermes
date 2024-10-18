using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System;

namespace Hermes.Common.Extensions;

public static class ObservableExtensions
{
    /// <summary>
    /// Emits a value from the source Observable, then ignores subsequent source values for a duration determined by sampleDuration, then repeats this process.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">Source sequence to throttle.</param>
    /// <param name="sampleDuration">Duration for throttling applied to each element.</param>
    /// <param name="scheduler">Scheduler to run throttle timers on.</param>
    /// <returns>The source sequence with the first element taken during the specified duration from the source sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="scheduler"/> is null.</exception>
    /// <remarks>
    /// This operator takes the first element and then throttles the source sequence by holding on to each element for the duration denoted by <paramref name="sampleDuration"/>.
    /// If another element is produced within this time window, the element is dropped and a new timer is started for the current element, repeating this whole
    /// process.
    /// </remarks>
    public static IObservable<T> TakeFirstAndThrottle<T>(
        this IObservable<T> source,
        TimeSpan sampleDuration,
        IScheduler? scheduler = null)
    {
        scheduler = scheduler ?? Scheduler.Default;
        return source.Publish(ps =>
            ps.Window(() => ps.Throttle(sampleDuration, scheduler))
                .SelectMany(x => x.Take(1)));
    }
}