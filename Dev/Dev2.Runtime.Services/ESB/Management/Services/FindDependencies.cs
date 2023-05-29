#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ResourceCatalogImpl;
using Dev2.Workspaces;
using ServiceStack.Common;
using Warewolf.Resource.Errors;
using ServiceStack.Extensions;
using ServiceStack;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FindDependencies : DefaultEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Any;

        IResourceCatalog _resourceCatalog;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                Dev2Logger.Info("Find Dependencies", GlobalConstants.WarewolfInfo);
                var result = new ExecuteMessage { HasError = false };

                string resourceId = null;
                string dependsOnMeString = null;
                var dependsOnMe = false;
                values.TryGetValue("ResourceId", out StringBuilder tmp);
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
                if (!Guid.TryParse(resourceId, out Guid resId))
                {
                    throw new InvalidDataContractException(ErrorResource.ResourceIdNotAGUID);
                }
                var resource = ResourceCatalog.GetResource(theWorkspace.ID, resId);

                if (!string.IsNullOrEmpty(dependsOnMeString) && !bool.TryParse(dependsOnMeString, out dependsOnMe))
                {
                    dependsOnMe = false;
                }


                if (dependsOnMe)
                {
                    result.Message.Append($"<graph title=\"Local Dependants Graph: {resourceId}\">");
                    result.Message.Append(FindWhatDependsOnMe(theWorkspace.ID, resource.ResourceID, new List<Guid>()));
                    result.Message.Append("</graph>");
                }
                else
                {
                    List<IResource> resourceList = new List<IResource>();
                    var count = 0;
                    resourceList.Add(resource);

                    while (count != -1)
                    {
                        List<IResourceForTree> resourceForTrees = new List<IResourceForTree>();
                        var dependencies = resourceList[count]?.Dependencies;
                        if (dependencies != null && dependencies.Count > 0)
                        {
                            dependencies.ToList().ForEach(c =>
                            {
                                c.Resource = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, c.ResourceID);

                                resourceList.Add(c.Resource);
                                resourceForTrees.Add(c);

                                count++;    
                            });
                        }
                        else
                        {
                            count = -1;
                        }
                    }
                    resource = resourceList[0];

                    result.Message.Append($"<graph title=\"Dependency Graph Of {resourceId}\">");
                    result.Message.Append(FindDependenciesRecursive(resource, theWorkspace.ID, new List<Guid>()));
                    result.Message.Append("</graph>");
                }

                var serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(result);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
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

        #region Private Methods

        StringBuilder FindDependenciesRecursive(IResource resource, Guid workspaceId, List<Guid> seenResource)
        {
            var sb = new StringBuilder();

            var dependencies = resource?.Dependencies;
            if (dependencies != null)
            {
                sb.Append($"<node id=\"{resource.ResourceID}\" x=\"\" y=\"\" broken=\"false\">");

                //dependencies.ForEach(c => sb.Append($"<dependency id=\"{c.ResourceID}\" />"));
                dependencies.Each(c => sb.Append($"<dependency id=\"{c.ResourceID}\" />"));

                sb.Append("</node>");
                seenResource.Add(resource.ResourceID);
                dependencies.ToList().ForEach(c =>
                {
                    if (!seenResource.Contains(c.ResourceID))
                    {
                        var findDependenciesRecursive = FindDependenciesRecursive(c.Resource, workspaceId, seenResource);
                        sb.Append(findDependenciesRecursive);
                    }
                });
            }
            return sb;
        }

        #endregion

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), @"<DataList><ResourceId ColumnIODirection=""Input""/><GetDependsOnMe ColumnIODirection=""Input""/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FindDependencyService";
    }
}
