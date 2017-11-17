/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Security;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Utils;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common;
using System.IO;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Studio.Core.Factories

{
    public static class ResourceModelFactory
    {
        public static IContextualResourceModel CreateResourceModel(IServer environment)
        {
            return new ResourceModel(environment)
            {
                UserPermissions = Permissions.Contribute
            };
        }

        public static IContextualResourceModel CreateResourceModel(IServer environment, IResource resource, XDocument xElement)
        {
            IContextualResourceModel contextualResource = CreateResourceModel(environment);
            contextualResource.ID = resource.ResourceID;
            contextualResource.UserPermissions = Permissions.Contribute;
            contextualResource.Category = Path.Combine(EnvironmentVariables.ResourcePath, resource.ResourceName);
            if (resource.ResourceType == "WorkflowService" || resource.ResourceType == "Workflow")
            {
                var def = xElement.Element("Service").Element("Action").Element("XamlDefinition").ToStringBuilder();
                XElement xaml = def.Unescape().Replace("<XamlDefinition>", "").Replace("</XamlDefinition>", "").ToXElement();
                contextualResource.WorkflowXaml = xaml.ToString(SaveOptions.DisableFormatting).ToStringBuilder();
                return SetResourceProperties(resource.ResourceType, resource.ResourceName, resource.ResourceName, contextualResource);
            }
            return null;
        }

        public static IContextualResourceModel CreateResourceModel(IServer environment, string resourceType, string displayName)
        {
            return CreateResourceModel(environment, resourceType, "", displayName);
        }

        public static IContextualResourceModel CreateResourceModel(IServer environment, string resourceType, string resourceName, string displayName)
        {
            try
            {
                IContextualResourceModel resource = CreateResourceModel(environment);
                resource.ResourceName = resourceName;
                resource.ID = Guid.NewGuid();
                resource.UserPermissions = environment.AuthorizationService != null ? environment.AuthorizationService.GetResourcePermissions(resource.ID) : Permissions.Contribute;
                return SetResourceProperties(resourceType, resourceName, displayName, resource);
            }
            catch (SystemException exception)
            {
                HelperUtils.ShowTrustRelationshipError(exception);
            }
            return null;
        }

        private static IContextualResourceModel SetResourceProperties(string resourceType, string resourceName, string displayName, IContextualResourceModel resource)
        {
            switch (resourceType)
            {
                case "Service":
                    resource.ResourceType = ResourceType.Service;
                    resource.DisplayName = displayName;
                    resource.ResourceName = resourceName;
                    break;
                case "DatabaseService":
                    resource.ResourceType = ResourceType.Service;
                    resource.DisplayName = "Service";
                    resource.ServerResourceType = "DbService";
                    resource.IsDatabaseService = true;
                    resource.ResourceName = resourceName;
                    break;
                case "ResourceService":
                    resource.ResourceType = ResourceType.Service;
                    resource.DisplayName = "PluginService";
                    resource.ServerResourceType = "PluginService";
                    resource.IsPluginService = true;
                    resource.ResourceName = resourceName;
                    break;
                case "ResourceSource":
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = "Plugin";
                    resource.ServerResourceType = "PluginSource";
                    resource.IsResourceService = true;
                    resource.ResourceName = resourceName;
                    break;
                case "Source":
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = displayName;
                    resource.ResourceName = resourceName;
                    break;
                case "Server":
                    resource.ResourceType = ResourceType.Server;
                    resource.DisplayName = displayName;
                    resource.ServerResourceType = "ServerSource";
                    resource.ResourceName = resourceName;
                    break;
                case "Human Interface Workflow":
                case "HumanInterfaceProcess":
                    resource.Category = resourceType;
                    resource.AllowCategoryEditing = false;
                    resource.ResourceType = ResourceType.WorkflowService;
                    resource.ServerResourceType = "WorkflowService";
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
                    resource.ServerResourceType = "WebSource";
                    resource.ResourceName = resourceName;
                    break;
                case "EmailSource":       // PBI 5656 - 2013.05.20 - TWR - Added
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = displayName; // this MUST be ResourceType; see RootWebSite.ShowDialog()
                    resource.ServerResourceType = "EmailSource";
                    resource.ResourceName = resourceName;
                    break;

                case "DbSource":
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = displayName;
                    resource.ServerResourceType = "DbSource";
                    resource.ResourceName = resourceName;
                    break;
                case "WebService":      // PBI 1220 - 2013.05.20 - TWR - Added
                    resource.ResourceType = ResourceType.Service;
                    resource.DisplayName = displayName; // this MUST be ResourceType; see RootWebSite.ShowDialog()
                    resource.ServerResourceType = "WebService";
                    resource.ResourceName = resourceName;
                    break;

                case "WorkflowService":
                case "Workflow":
                    resource.ResourceType = ResourceType.WorkflowService;
                    resource.DisplayName = displayName;
                    resource.ServerResourceType = "WorkflowService";
                    resource.ResourceName = resourceName;
                    break;
                case "DropboxSource":
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = displayName;
                    resource.ServerResourceType = "DropboxSource";
                    resource.ResourceName = resourceName;
                    resource.ID = Guid.Empty;
                    break;
                case "SharepointServerSource":
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = displayName;
                    resource.ServerResourceType = "SharepointServerSource";
                    resource.ResourceName = resourceName;
                    resource.ID = Guid.Empty;
                    break;
                case "RabbitMQSource":
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = displayName;
                    resource.ServerResourceType = "RabbitMQSource";
                    resource.ResourceName = resourceName;
                    break;
                case "PluginSource":
                    resource.ResourceType = ResourceType.Source;
                    resource.DisplayName = displayName;
                    resource.ServerResourceType = "PluginSource";
                    resource.ResourceName = resourceName;
                    break;
                default:
                    break;
            }
            return resource;
        }
    }
}
