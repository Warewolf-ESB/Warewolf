using Dev2.Communication;
using Dev2.Providers.Events;

namespace Dev2.Services.Security
{
    public class PermissionsModifiedService : MemoSubscriptionService<PermissionsModifiedMemo>, IPermissionsModifiedService
    {
        public PermissionsModifiedService(IEventPublisher eventPublisher)
            : base(eventPublisher)
        {
        }
    }
}
