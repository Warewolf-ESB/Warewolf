using System.ComponentModel.Composition;
using Caliburn.Micro;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Studio.Webs.Callbacks
{
    public class DbServiceCallbackHandler : WebsiteCallbackHandler
    {
        [Import]
        public IEventAggregator EventAggregator { get; set; }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            LoadService(environmentModel, jsonObj.ResourceID.Value, jsonObj.ResourceName.Value, jsonObj.ResourceType.Value);
        }

        public void LoadService(IEnvironmentModel environmentModel, string serviceID, string serviceName, string resourceType)
        {
            var effectedResources = environmentModel.Resources.ReloadResource(serviceName, ResourceType.Service, ResourceModelEqualityComparer.Current);
            foreach(var resource in effectedResources)
            {
                EventAggregator.Publish(new UpdateResourceMessage(resource));
            }
        }
    }
}
