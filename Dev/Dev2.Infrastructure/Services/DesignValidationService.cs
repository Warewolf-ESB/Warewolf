using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Communication;

namespace Dev2.Services
{
    public class DesignValidationService : MemoSubscriptionService<DesignValidationMemo>, IDesignValidationService
    {
        public DesignValidationService(IEventPublisher eventPublisher)
            : base(eventPublisher)
        {
        }
    }
}
