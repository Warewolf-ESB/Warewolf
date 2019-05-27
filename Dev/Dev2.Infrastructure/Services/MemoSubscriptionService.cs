#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

        public void Subscribe(Guid memoID, Action<TEvent> onNext)
        {
            if(!_subscriptions.Contains(memoID))
            {
                _subscriptions.Add(memoID);
                _subscriptionService.Subscribe(m => m.InstanceID == memoID, onNext);
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
