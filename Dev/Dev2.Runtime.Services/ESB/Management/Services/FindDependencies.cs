using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find dependencies for a service
    /// </summary>
    public class FindDependencies : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var result = new ExecuteMessage() { HasError = false };

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
            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor

            if(dependsOnMe)
            {
                // FindWhatDependsOnMe(resourceName, theWorkspace.ID)
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

        StringBuilder FindWhatDependsOnMe(string resourceName, Guid workspaceID)
        {
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, resourceName);
            var sb = new StringBuilder();
            if(dependants != null)
            {
                dependants.ForEach(c =>
                {
                    var resource = ResourceCatalog.Instance.GetResource(workspaceID, c);
                    var brokenString = "false";
                    if(resource == null)
                    {
                        brokenString = "true";
                    }
                    sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"{1}\">", c, brokenString));
                    sb.Append(string.Format("<dependency id=\"{0}\" />", resourceName));
                    sb.Append("</node>");
                });

            }

            sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\">", resourceName));
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
                DataListSpecification = @"<DataList><ResourceName ColumnIODirection=""Input""/><GetDependsOnMe ColumnIODirection=""Input""/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>"
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

        // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
        private StringBuilder FindDependenciesRecursive(string resourceName, Guid workspaceID)
        {
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, resourceName);
            var sb = new StringBuilder();
            var brokenString = "true";
            if(resource != null)
            {
                brokenString = "false";
                var dependencies = resource.Dependencies;
                if(dependencies != null)
                {
                    sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\">", resourceName));
                    dependencies.ForEach(c => sb.Append(string.Format("<dependency id=\"{0}\" />", c.ResourceName)));
                    sb.Append("</node>");
                    dependencies.ToList().ForEach(c => sb.Append(FindDependenciesRecursive(c.ResourceName, workspaceID)));
                }
            }
            sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"{1}\">", resourceName, brokenString));
            sb.Append("</node>");
            return sb;
        }


        #endregion



    }
}
