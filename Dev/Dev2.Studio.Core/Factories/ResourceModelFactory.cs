using System;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.Factories{
    
    public static class ResourceModelFactory
    {
        public static IContextualResourceModel CreateResourceModel(IEnvironmentModel environment) 
        {
            return new ResourceModel(environment);
        }

        public static IContextualResourceModel CreateResourceModel(IEnvironmentModel environment, ResourceType resourceType, string iconPath, string displayName)
        {
            IContextualResourceModel resource = new ResourceModel(environment);
            resource.ResourceType = resourceType;
            resource.IconPath = iconPath;
            resource.DisplayName = displayName;
            resource.ServiceDefinition = string.Empty;
            return resource;
        }

        public static IContextualResourceModel CreateResourceModel(IEnvironmentModel environment, string resourceType, string displayName)
        {
            return CreateResourceModel(environment, resourceType, "",displayName);
        }

        public static IContextualResourceModel CreateResourceModel(IEnvironmentModel environment, string resourceType, string resourceName,string displayName)
        {
            IContextualResourceModel resource = CreateResourceModel(environment);
            resource.ResourceName = string.Empty;
            resource.ID = Guid.NewGuid();

            switch (resourceType) {
                case "Service":
                    resource.ResourceType = ResourceType.Service;
                    resource.DisplayName = displayName;
                    resource.ResourceName = resourceName;
                    break;
                case "DatabaseService":
                    resource.ResourceType = ResourceType.Service;
                    resource.DisplayName = "Service";
                    resource.IsDatabaseService = true;
                    resource.ResourceName = resourceName;
                    break;
                case "ResourceService":
                    resource.ResourceType = ResourceType.Service;
                    resource.DisplayName = "Service";
                    resource.IsResourceService = true;
                    resource.ResourceName = resourceName;
                    break;
                case "ResourceSource":
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = "Plugin";
                    resource.IsResourceService = true;
                    resource.ResourceName = resourceName;
                    break;
                case "Source":
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = displayName;
                    resource.ResourceName = resourceName;
                    break;
                case "Human Interface Workflow":
                case "HumanInterfaceProcess":
                    resource.Category = resourceType;
                    resource.AllowCategoryEditing = false;
                    resource.ResourceType = ResourceType.WorkflowService;
                    resource.DisplayName = "Human Interface Workflow";
                    resource.ResourceName = resourceName;
                    break;
                case "Website":
                    resource.Category = resourceType;
                    resource.AllowCategoryEditing = false;
                    resource.ResourceType = ResourceType.WorkflowService;
                    resource.DisplayName = displayName;
                    resource.ResourceName = resourceName;
                    break;
                case "Unknown":
                    resource.ResourceType = ResourceType.Unknown;
                    resource.DisplayName = displayName;
                    resource.ResourceName = resourceName;
                    break;
                default:
                    resource.ResourceType = ResourceType.WorkflowService;
                    resource.DisplayName = displayName;
                    resource.ResourceName = resourceName;
                    break;
            }

            return resource;
        }
    }
}
