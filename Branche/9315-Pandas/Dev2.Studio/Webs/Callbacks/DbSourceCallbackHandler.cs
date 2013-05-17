using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Webs.Callbacks
{
    public class DbSourceCallbackHandler : WebsiteCallbackHandler
    {
        public DbSourceCallbackHandler()
            : this(EnvironmentRepository.Instance)
        {
        }

        public DbSourceCallbackHandler(IEnvironmentRepository currentEnvironmentRepository)
            : base(currentEnvironmentRepository)
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
