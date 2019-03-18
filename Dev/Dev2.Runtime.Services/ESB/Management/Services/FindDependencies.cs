#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using ServiceStack.Common.Extensions;
using Warewolf.Resource.Errors;

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
                    result.Message.Append($"<graph title=\"Dependency Graph Of {resourceId}\">");
                    result.Message.Append(FindDependenciesRecursive(resource.ResourceID, theWorkspace.ID, new List<Guid>()));
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

        StringBuilder FindDependenciesRecursive(Guid resourceGuid, Guid workspaceId, List<Guid> seenResource)
        {
            var sb = new StringBuilder();

            var resource = ResourceCatalog.GetResource(workspaceId, resourceGuid);
            var dependencies = resource?.Dependencies;
            if (dependencies != null)
            {
                sb.Append($"<node id=\"{resource.ResourceID}\" x=\"\" y=\"\" broken=\"false\">");

                dependencies.ForEach(c => sb.Append($"<dependency id=\"{c.ResourceID}\" />"));

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

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), @"<DataList><ResourceId ColumnIODirection=""Input""/><GetDependsOnMe ColumnIODirection=""Input""/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FindDependencyService";
    }
}
