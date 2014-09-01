using System;
using Caliburn.Micro;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Webs.Callbacks
{
    public class SourceCallbackHandler : WebsiteCallbackHandler
    {

        public SourceCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public SourceCallbackHandler(IEnvironmentRepository currentEnvironmentRepository)
            : this(EventPublishers.Aggregator, currentEnvironmentRepository)
        {
        }

        public SourceCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository)
            : base(eventPublisher, currentEnvironmentRepository)
        {
        }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            ReloadResource(environmentModel, Guid.Parse(jsonObj.ResourceID.Value), ResourceType.Source);
        }

        public override void Cancel()
        {
            Close();
        }

    }
}
