using Reactive.Bindings.Disposables;

namespace Hermes.Common.Extensions;

public static class CompositeDisposableExtensions
{
    public static void DisposeItems(this CompositeDisposable disposable)
    {
        foreach (var disposableItem in disposable)
        {
            disposableItem.Dispose();
            disposable.Remove(disposableItem);
        }
    }
}