using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Infrastructure.Events;

namespace Dev2.Services
{
    public class MemoSubscriptionService<TEvent> : DisposableObject, IMemoSubscriptionService<TEvent>
        where TEvent : class, IMemo, new()
    {
        readonly ISubscriptionService<TEvent> _subscriptionService;
        readonly List<Guid> _subscriptions;

        public MemoSubscriptionService(IEventPublisher eventPublisher)
            : this(new SubscriptionService<TEvent>(eventPublisher))
        {
        }

        public MemoSubscriptionService(ISubscriptionService<TEvent> subscriptionService)
        {
            _subscriptions = new List<Guid>();

            VerifyArgument.IsNotNull("subscriptionService", subscriptionService);
            _subscriptionService = subscriptionService;
        }

        public void Subscribe(Guid memoInstanceID, Action<TEvent> onNext)
        {
            if(!_subscriptions.Contains(memoInstanceID))
            {
                _subscriptions.Add(memoInstanceID);
                _subscriptionService.Subscribe(m => m.InstanceID == memoInstanceID, onNext);
            }
        }

        #region OnDisposed

        protected override void OnDisposed()
        {
            _subscriptionService.Dispose();
        }

        #endregion
    }
}
