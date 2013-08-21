
namespace Dev2.Services.Events
{
    public class StudioSubscriptionService<TEvent> : SubscriptionService<TEvent>
        where TEvent : class, new()
    {
        public StudioSubscriptionService()
            : base(EventPublishers.Studio)
        {
        }
    }
}
