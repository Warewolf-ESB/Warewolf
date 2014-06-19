using System;

namespace Dev2.Providers.Events
{
    public interface IEventPublisher
    {
        int Count { get; }

        bool RemoveEvent<TEvent>() where TEvent : class, new();

        IObservable<TEvent> GetEvent<TEvent>() where TEvent : class, new();

        void Publish<TEvent>(TEvent sampleEvent) where TEvent : class, new();

        void PublishObject(object sampleEvent);

    }
}