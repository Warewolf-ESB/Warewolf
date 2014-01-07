using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.Navigation;

// ReSharper disable once CheckNamespace
namespace Dev2.UI
{
    public class ConnectControlViewModelBuilder
    {
        public ConnectControlViewModel BuildConnectControlViewModel(SimpleBaseViewModel deployResource, IEnvironmentModel mainViewModelActiveEnvironment)
        {
            // Moved code incorrectly put into ConnectViewModel back here
            var abstractTreeViewModel = deployResource as AbstractTreeViewModel;
            IEnvironmentModel activeEnvironment = null;
            if(abstractTreeViewModel != null)
            {
                activeEnvironment = abstractTreeViewModel.EnvironmentModel;
            }
            else
            {
                var resourceModel = deployResource as ResourceModel;
                if(resourceModel != null)
                {
                    activeEnvironment = resourceModel.Environment;
                }
            }
            if(activeEnvironment == null)
            {
                activeEnvironment = mainViewModelActiveEnvironment;
            }
            return new ConnectControlViewModel(activeEnvironment);
        }
    }
}