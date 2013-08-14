#region

using System;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.Services;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.ViewModels.Navigation;
using Unlimited.Applications.BusinessDesignStudio.Activities;

#endregion

namespace Dev2.Studio.Factory
{
    public static class TreeViewModelFactory
    {
        public static ITreeNode Create()
        {
            return new RootTreeViewModel();
        }

        public static ITreeNode Create(IEventAggregator eventPublisher, IWizardEngine wizardEngine)
        {
            var root = new RootTreeViewModel(eventPublisher, wizardEngine);
            return root;
        }

        public static ITreeNode Create(IEventAggregator eventPublisher, IWizardEngine wizardEngine, IContextualResourceModel resource, ITreeNode parent, bool isWizard, bool isNewResource = true)
        {
            var validationService = new DesignValidationService(resource.Environment.Connection.ServerEvents);

            if(isWizard)
            {
                return new WizardTreeViewModel(validationService, parent, resource);
            }

            ResourceTreeViewModel vm;
            if(resource != null &&
               (resource.ResourceType == ResourceType.Service ||
                resource.ResourceType == ResourceType.WorkflowService))
            {
                // This is used to determine the type of activity that is dragged/dropped from the explorer tree
                Type type;
                switch(resource.ServerResourceType)
                {
                        // PBI 9135 - 2013.07.15 - TWR - Added
                    case "DbService":
                        type = typeof(DsfDatabaseActivity);
                        break;
                    case "PluginService":
                        type = typeof(DsfPluginActivity);
                        break;

                    default:
                        type = typeof(DsfActivity);
                        break;
                }
                vm = new ResourceTreeViewModel(eventPublisher, wizardEngine, validationService, parent, resource, type.AssemblyQualifiedName);
            }
            else
            {
                vm = new ResourceTreeViewModel(eventPublisher, wizardEngine, validationService, parent, resource);
            }
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