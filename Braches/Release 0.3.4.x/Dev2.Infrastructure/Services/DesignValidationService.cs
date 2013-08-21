using Dev2.Communication;
using Dev2.Providers.Events;

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
