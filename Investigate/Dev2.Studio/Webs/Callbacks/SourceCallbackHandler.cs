using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Webs.Callbacks
{
    public class SourceCallbackHandler : WebsiteCallbackHandler
    {

        public SourceCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public SourceCallbackHandler(IEnvironmentRepository environmentRepository)
            : base(environmentRepository)
        {
        }

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            ReloadResource(environmentModel, jsonObj.ResourceName.Value, ResourceType.Source);
        }

        public override void Cancel()
        {
            Close();
        }

    }
}
