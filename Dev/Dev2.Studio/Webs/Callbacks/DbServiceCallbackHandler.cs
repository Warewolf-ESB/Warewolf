using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Webs.Callbacks
{
    public class DbServiceCallbackHandler : WebsiteCallbackHandler
    {
        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            ReloadResource(environmentModel, jsonObj.ResourceName.Value, ResourceType.Service);
        }
    }
}
