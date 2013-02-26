using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.Studio.ViewModels.Navigation;

namespace Dev2.Studio.Factory
{
    public static class DeployViewModelFactory
    {
        public static DeployViewModel GetDeployViewModel(object input)
        {
            DeployViewModel deployViewModel = null;
            
            TypeSwitch.Do(input,
                TypeSwitch.Case<AbstractTreeViewModel>(x => deployViewModel = new DeployViewModel(x)),
                TypeSwitch.Case<IContextualResourceModel>(x => deployViewModel = new DeployViewModel(x)),
                TypeSwitch.Case<IEnvironmentModel>(x => deployViewModel = new DeployViewModel(x)),
                TypeSwitch.Default(() => deployViewModel = new DeployViewModel()));

            return deployViewModel;
        }
    }
}
