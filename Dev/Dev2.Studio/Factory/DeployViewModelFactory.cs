using Dev2.Models;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.Deploy;

namespace Dev2.Factory
{
    public static class DeployViewModelFactory
    {
        public static DeployViewModel GetDeployViewModel(object input)
        {
            DeployViewModel deployViewModel = null;

            if(input != null)
            {
                TypeSwitch.Do(input,
                              TypeSwitch.Case<ExplorerItemModel>(
                                  x => deployViewModel = new DeployViewModel(x.ResourceId, x.EnvironmentId)),
                              TypeSwitch.Case<ResourceModel>(
                                  x => deployViewModel = new DeployViewModel(x.ID, x.Environment.ID)),
                              TypeSwitch.Default(() => deployViewModel = new DeployViewModel()));
            }
            else
            {
                deployViewModel = new DeployViewModel();
            }

            return deployViewModel;
        }
    }
}
