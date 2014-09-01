using System;
using Dev2.Common.Interfaces.Communication;

namespace Dev2.Services
{
    public interface IMemoSubscriptionService<out TEvent> : IDisposable
        where TEvent : class, IMemo, new()
    {
        void Subscribe(Guid memoID, Action<TEvent> onNext);
    }
}