using System;

namespace Dev2.Services
{
    public interface ISubscriptionService<out TEvent> : IDisposable
         where TEvent : class, new()
    {
        int Count { get; }

        void Subscribe(Action<TEvent> onNext);
        void Subscribe(Func<TEvent, bool> filter, Action<TEvent> onNext);

        void Unsubscribe();
    }
}