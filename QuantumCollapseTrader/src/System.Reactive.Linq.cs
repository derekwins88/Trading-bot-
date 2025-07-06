using System;

namespace System.Reactive.Linq
{
    // Minimal extensions to mimic Reactive Subscribe behavior for stubs
    public static class ObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
        {
            return source.Subscribe(new ActionObserver<T>(onNext));
        }

        private class ActionObserver<T> : IObserver<T>
        {
            private readonly Action<T> _onNext;
            public ActionObserver(Action<T> onNext) { _onNext = onNext; }
            public void OnCompleted() { }
            public void OnError(Exception error) { }
            public void OnNext(T value) { _onNext?.Invoke(value); }
        }
    }
}
