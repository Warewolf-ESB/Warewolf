using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Threading;
using Dev2.Providers.Events;

namespace Dev2.Services
{
    public class SubscriptionService<TEvent> : DisposableObject, ISubscriptionService<TEvent>
         where TEvent : class, new()
    {
        readonly List<IDisposable> _subscriptions;
        readonly IObservable<TEvent> _events;

        public SubscriptionService(IEventPublisher eventPublisher)
        {
            _subscriptions = new List<IDisposable>();

            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _events = eventPublisher.GetEvent<TEvent>();


            if(Dispatcher.CurrentDispatcher.CheckAccess())
            {
                try
                {
                    _events = _events.ObserveOnDispatcher();
                }
                catch
                {
                    // FOR TESTING FUNNIES!!
                }
            }
        }

        public int Count { get { return _subscriptions.Count; } }

        public virtual void Subscribe(Func<TEvent, bool> filter, Action<TEvent> onNext)
        {
            var events = filter == null ? _events : _events.Where(filter);
            var subscription = events.Subscribe(onNext);
            _subscriptions.Add(subscription);
        }

        #region OnDisposed

        protected override void OnDisposed()
        {
            foreach(var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        #endregion
    }
}