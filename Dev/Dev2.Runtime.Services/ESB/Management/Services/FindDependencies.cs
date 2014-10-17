
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using ServiceStack.Common.Extensions;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find dependencies for a service
    /// </summary>
    public class FindDependencies : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {

     
            Dev2Logger.Log.Info("Find Dependencies");
            var result = new ExecuteMessage { HasError = false };

            string resourceName = null;
            string dependsOnMeString = null;
            bool dependsOnMe = false;
            StringBuilder tmp;
            values.TryGetValue("ResourceName", out tmp);
            if(tmp != null)
            {
                resourceName = tmp.ToString();
            }
            values.TryGetValue("GetDependsOnMe", out tmp);
            if(tmp != null)
            {
                dependsOnMeString = tmp.ToString();
            }
            if(string.IsNullOrEmpty(resourceName))
            {
                throw new InvalidDataContractException("ResourceName is empty or null");
            }
            if(!string.IsNullOrEmpty(dependsOnMeString))
            {
                if(!bool.TryParse(dependsOnMeString, out dependsOnMe))
                {
                    dependsOnMe = false;
                }
            }

            if(dependsOnMe)
            {
                result.Message.Append(string.Format("<graph title=\"Local Dependants Graph: {0}\">", resourceName));
                result.Message.Append(FindWhatDependsOnMe(resourceName, theWorkspace.ID));
                result.Message.Append("</graph>");
            }
            else
            {
                result.Message.Append(string.Format("<graph title=\"Dependency Graph Of {0}\">", resourceName));
                result.Message.Append(FindDependenciesRecursive(resourceName, theWorkspace.ID));
                result.Message.Append("</graph>");
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error(e);
                throw;
            }
        }

        StringBuilder FindWhatDependsOnMe(string resourceName, Guid workspaceId)
        {
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceName) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceName);
            return FindWhatDependsOnMe(resource, workspaceId);
        }

        StringBuilder FindWhatDependsOnMe(IResource res, Guid workspaceId)
        {
            if(res == null)
            {
                throw new ArgumentNullException("res", @"Resource not found");
            }
            var resourceName = res.ResourceName;
            var resourceCat = res.ResourcePath;
            var resourceId = res.ResourceID;
            var dependants = ResourceCatalog.Instance.GetDependants(Guid.Empty, resourceId);
            dependants.AddRange(ResourceCatalog.Instance.GetDependants(workspaceId, resourceId));
            dependants = dependants.Distinct().ToList();
            var sb = new StringBuilder();
            dependants.ForEach(c =>
            {
                var resource = ResourceCatalog.Instance.GetResource(workspaceId, c) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, c);

                if(resource != null)
                {
                    const string BrokenString = "false";
                    sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"{1}\">", resource.ResourceName, BrokenString));
                    sb.Append(string.Format("<dependency id=\"{0}\" />", resourceName));
                    sb.Append("</node>");
                }
            });

            sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\">", resourceCat));
            sb.Append("</node>");
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
                DataListSpecification = new StringBuilder(@"<DataList><ResourceName ColumnIODirection=""Input""/><GetDependsOnMe ColumnIODirection=""Input""/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>")
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

        private StringBuilder FindDependenciesRecursive(string resourceName, Guid workspaceId)
        {
            var resource = ResourceCatalog.Instance.GetResource(Guid.Empty, resourceName);
            return FindDependenciesRecursive(resource.ResourceID, workspaceId);
        }
        private StringBuilder FindDependenciesRecursive(Guid resourceGuid, Guid workspaceId)
        {
            var sb = new StringBuilder();
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceGuid);
            if(resource != null)
            {
                const string BrokenString = "false";
                var dependencies = resource.Dependencies;
                if(dependencies != null)
                {
                    sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\">", resource.ResourceName));
// ReSharper disable ImplicitlyCapturedClosure
                    dependencies.ForEach(c => sb.Append(string.Format("<dependency id=\"{0}\" />", c.ResourceName)));
// ReSharper restore ImplicitlyCapturedClosure
                    sb.Append("</node>");
                    dependencies.ToList().ForEach(c => sb.Append(FindDependenciesRecursive(c.ResourceID, workspaceId)));
                }
                sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"{1}\">", resource.ResourcePath, BrokenString));
                sb.Append("</node>");
            }
            return sb;
        }


        #endregion



    }
}
