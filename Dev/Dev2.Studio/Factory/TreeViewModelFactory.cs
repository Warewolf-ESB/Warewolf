#region

using Dev2.Services;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;
using Unlimited.Applications.BusinessDesignStudio.Activities;

#endregion

namespace Dev2.Studio.Factory
{
    public static class TreeViewModelFactory
    {
        public static ITreeNode Create()
        {
            var root = new RootTreeViewModel();
            return root;
        }

        public static ITreeNode Create(IContextualResourceModel resource, ITreeNode parent, bool isWizard,bool isNewResource = true)
        {
            ResourceTreeViewModel vm;
            var validationService = new DesignValidationService(resource.Environment.Connection.ServerEvents);

            if (isWizard)
            {
                return new WizardTreeViewModel(validationService, parent, resource);
            }

            if(resource != null &&
               (resource.ResourceType == ResourceType.Service ||
                resource.ResourceType == ResourceType.WorkflowService))
            {
                vm = new ResourceTreeViewModel(validationService, parent, resource,
                    typeof(DsfActivity).AssemblyQualifiedName);
            }
            else
            {
                vm = new ResourceTreeViewModel(validationService, parent, resource);
            }
            vm.IsNew = isNewResource;
            return vm;
        }

        public static ITreeNode Create(IEnvironmentModel environmentModel, ITreeNode parent)
        {
            var vm = new EnvironmentTreeViewModel(parent, environmentModel);
            return vm;
        }

        public static ITreeNode Create(ResourceType resourceCategory, ITreeNode parent)
        {
            var vm = new ServiceTypeTreeViewModel(resourceCategory, parent);
            return vm;
        }

        public static ITreeNode CreateCategory(string name, ResourceType resourceType, ITreeNode parent)
        {
            bool updateParentVisibility = parent != null && (parent.Children == null || parent.Children.Count == 0);
            var vm = new CategoryTreeViewModel(name, resourceType, parent);

            return vm;
        }
    }
}