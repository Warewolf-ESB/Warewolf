using Caliburn.Micro;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Webs.Callbacks;
using System;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs.Callbacks
{
    public class WebServiceCallbackHandler : WebsiteCallbackHandler
    {
        public WebServiceCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public WebServiceCallbackHandler(IEnvironmentRepository currentEnvironmentRepository)
            : this(EventPublishers.Aggregator, currentEnvironmentRepository)
        {
        }

        public WebServiceCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository)
            : base(eventPublisher, currentEnvironmentRepository)
        {
        }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            ReloadResource(environmentModel, Guid.Parse(jsonObj.ResourceID.Value), ResourceType.Service);
        }

        public override void Cancel()
        {
            Close();
        }

    }
}
