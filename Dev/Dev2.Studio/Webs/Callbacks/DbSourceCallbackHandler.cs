using Caliburn.Micro;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Webs.Callbacks
{
    public class DbSourceCallbackHandler : WebsiteCallbackHandler
    {
        public DbSourceCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public DbSourceCallbackHandler(IEnvironmentRepository currentEnvironmentRepository)
            : this(EventPublishers.Aggregator, currentEnvironmentRepository)
        {
        }

        public DbSourceCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository)
            : base(eventPublisher, currentEnvironmentRepository)
        {
        }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            ReloadResource(environmentModel, jsonObj.ResourceID.Value, ResourceType.Source);
        }

        public override void Cancel()
        {
            Close();
        }

    }
}
