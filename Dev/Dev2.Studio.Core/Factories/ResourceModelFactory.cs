using System;
using System.Windows;
using Dev2.Composition;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Utils;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Factories
{

    public static class ResourceModelFactory
    {
        public static IContextualResourceModel CreateResourceModel(IEnvironmentModel environment)
        {
            return new ResourceModel(environment)
            {
                UserPermissions = Permissions.Contribute
            };
        }

        public static IContextualResourceModel CreateResourceModel(IEnvironmentModel environment, ResourceType resourceType, string iconPath, string displayName)
        {
            IContextualResourceModel resource = new ResourceModel(environment);
            resource.ResourceType = resourceType;
            resource.IconPath = iconPath;
            resource.DisplayName = displayName;
            resource.UserPermissions = Permissions.Contribute;
            return resource;
        }

        public static IContextualResourceModel CreateResourceModel(IEnvironmentModel environment, string resourceType)
        {
            return CreateResourceModel(environment, resourceType, resourceType);
        }

        public static IContextualResourceModel CreateResourceModel(IEnvironmentModel environment, string resourceType, string displayName)
        {
            return CreateResourceModel(environment, resourceType, "", displayName);
        }

        public static IContextualResourceModel CreateResourceModel(IEnvironmentModel environment, string resourceType, string resourceName, string displayName)
        {
            try
            {
            IContextualResourceModel resource = CreateResourceModel(environment);
            resource.ResourceName = string.Empty;
            resource.ID = Guid.NewGuid();
            if(environment.AuthorizationService != null)
            {
                resource.UserPermissions = environment.AuthorizationService.GetResourcePermissions(resource.ID);
            }
            else
            {
                resource.UserPermissions = Permissions.Contribute;
            }

            switch(resourceType)
            {
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
                    resource.DisplayName = "PluginService";
                    resource.IsPluginService = true;
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


                case "EmailResource":   // PBI 953 - 2013.05.16 - TWR - Added
                case "WebSource":       // PBI 5656 - 2013.05.20 - TWR - Added
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = displayName; // this MUST be ResourceType; see RootWebSite.ShowDialog()
                    resource.ResourceName = resourceName;
                    break;


                case "WebService":      // PBI 1220 - 2013.05.20 - TWR - Added
                    resource.ResourceType = ResourceType.Service;
                    resource.DisplayName = displayName; // this MUST be ResourceType; see RootWebSite.ShowDialog()
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
            catch(SystemException exception)
            {
                HelperUtils.ShowTrustRelationshipError(exception);
            }
            return null;
        }

        
    }
}
