using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.Studio.ViewModels.Navigation;

namespace Dev2.Factory
{
    public static class DeployViewModelFactory
    {
        public static DeployViewModel GetDeployViewModel(object input)
        {
            DeployViewModel deployViewModel = null;

            if (input != null)
            {
                TypeSwitch.Do(input,
                              TypeSwitch.Case<AbstractTreeViewModel>(
                                  x => deployViewModel = new DeployViewModel(x.DisplayName, x.EnvironmentModel)),
                              TypeSwitch.Case<ResourceModel>(
                                  x => deployViewModel = new DeployViewModel(x.ResourceName, x.Environment)),
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
