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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Dev2.Workspaces;
using ServiceStack.Common.Extensions;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find dependencies for a service
    /// </summary>
    public class FindDependencies : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        private IResourceCatalog _resourceCatalog;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                Dev2Logger.Info("Find Dependencies");
                var result = new ExecuteMessage { HasError = false };

                string resourceId = null;
                string dependsOnMeString = null;
                bool dependsOnMe = false;
                StringBuilder tmp;
                values.TryGetValue("ResourceId", out tmp);
                if (tmp != null)
                {
                    resourceId = tmp.ToString();
                }
                values.TryGetValue("GetDependsOnMe", out tmp);
                if (tmp != null)
                {
                    dependsOnMeString = tmp.ToString();
                }
                if (string.IsNullOrEmpty(resourceId))
                {
                    throw new InvalidDataContractException(ErrorResource.ResourceIdIsNull);
                }
                Guid resId;
                if (!Guid.TryParse(resourceId, out resId))
                {
                    throw new InvalidDataContractException(ErrorResource.ResourceIdNotAGUID);
                }
                var resource = ResourceCatalog.GetResource(theWorkspace.ID, resId);
                if (!string.IsNullOrEmpty(dependsOnMeString))
                {
                    if (!bool.TryParse(dependsOnMeString, out dependsOnMe))
                    {
                        dependsOnMe = false;
                    }
                }

                if (dependsOnMe)
                {
                    result.Message.Append($"<graph title=\"Local Dependants Graph: {resourceId}\">");
                    result.Message.Append(FindWhatDependsOnMe(theWorkspace.ID, resource.ResourceID, new List<Guid>()));
                    result.Message.Append("</graph>");
                }
                else
                {
                    result.Message.Append($"<graph title=\"Dependency Graph Of {resourceId}\">");
                    result.Message.Append(FindDependenciesRecursive(resource.ResourceID, theWorkspace.ID, new List<Guid>()));
                    result.Message.Append("</graph>");
                }

                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(result);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                throw;
            }
        }

        public IResourceCatalog ResourceCatalog
        {
            private get
            {
                return _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalog = value;
            }
        }

        StringBuilder FindWhatDependsOnMe(Guid workspaceId, Guid resourceID, List<Guid> seenResource)
        {
            if (resourceID == null)
            {
                throw new ArgumentNullException("resourceID", ErrorResource.ResourceNotFound);
            }
            var dependants = ResourceCatalog.GetDependants(Guid.Empty, resourceID) ?? new List<Guid>();
            dependants.AddRange(ResourceCatalog.GetDependants(workspaceId, resourceID) ?? new List<Guid>());
            dependants = dependants.Distinct().ToList();
            var sb = new StringBuilder();
            sb.Append($"<node id=\"{resourceID}\" x=\"\" y=\"\" broken=\"false\">");
            dependants.ForEach(c =>
            {
                var resource = ResourceCatalog.GetResource(workspaceId, c) ?? ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, c);
                if (resource != null)
                {
                    sb.Append($"<dependency id=\"{resource.ResourceID}\" />");
                }

            });
            sb.Append("</node>");
            seenResource.Add(resourceID);
            dependants.ForEach(c =>
            {
                if (!seenResource.Contains(c))
                {
                    var depOfCurrentDep = FindWhatDependsOnMe(workspaceId, c, seenResource);
                    sb.Append(depOfCurrentDep);
                }
            });
            return sb;
        }

        public string HandlesType()
        {
            return "FindDependencyService";
        }

        public DynamicService CreateServiceEntry()
        {
            var ds = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder(@"<DataList><ResourceId ColumnIODirection=""Input""/><GetDependsOnMe ColumnIODirection=""Input""/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>")
            };

            var sa = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            ds.Actions.Add(sa);

            return ds;
        }

        #region Private Methods


        private StringBuilder FindDependenciesRecursive(Guid resourceGuid, Guid workspaceId, List<Guid> seenResource)
        {
            var sb = new StringBuilder();

            var resource = ResourceCatalog.GetResource(workspaceId, resourceGuid);
            var dependencies = resource?.Dependencies;
            if (dependencies != null)
            {
                sb.Append($"<node id=\"{resource.ResourceID}\" x=\"\" y=\"\" broken=\"false\">");
                // ReSharper disable ImplicitlyCapturedClosure
                dependencies.ForEach(c => sb.Append($"<dependency id=\"{c.ResourceID}\" />"));
                // ReSharper restore ImplicitlyCapturedClosure
                sb.Append("</node>");
                seenResource.Add(resourceGuid);
                dependencies.ToList().ForEach(c =>
                {
                    if (!seenResource.Contains(c.ResourceID))
                    {
                        var findDependenciesRecursive = FindDependenciesRecursive(c.ResourceID, workspaceId, seenResource);
                        sb.Append(findDependenciesRecursive);
                    }
                });
            }
            return sb;
        }


        #endregion



    }
}
