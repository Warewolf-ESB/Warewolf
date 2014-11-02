
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Threading;
using Dev2.Common.Interfaces.Infrastructure.Events;

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


            // Don't observe on dispatcher if this is a background thread!
            try
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                if(dispatcher.CheckAccess() && !dispatcher.Thread.IsBackground)
                {

                    _events = _events.ObserveOnDispatcher();
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // FOR TESTING FUNNIES!!
            }
        }

        public int Count { get { return _subscriptions.Count; } }

        public void Subscribe(Action<TEvent> onNext)
        {
            Subscribe(null, onNext);
        }

        public virtual void Subscribe(Func<TEvent, bool> filter, Action<TEvent> onNext)
        {
            var events = filter == null ? _events : (_events != null ? _events.Where(filter) : null);
            if(events != null)
            {
                var subscription = events.Subscribe(onNext);
                _subscriptions.Add(subscription);
            }
        }

        public void Unsubscribe()
        {
            foreach(var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        #region OnDisposed

        protected override void OnDisposed()
        {
            Unsubscribe();
        }

        #endregion
    }
}
