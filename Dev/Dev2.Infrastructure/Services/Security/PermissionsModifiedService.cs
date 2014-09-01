using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Communication;

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
