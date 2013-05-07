using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find dependencies for a service
    /// </summary>
    public class FindDependencies : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string resourceName;
            string dependsOnMeString;
            bool dependsOnMe=false;
            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("GetDependsOnMe", out dependsOnMeString);
            if(string.IsNullOrEmpty(resourceName))
            {
                throw new InvalidDataContractException("ResourceName is empty or null");
            }
            if(!string.IsNullOrEmpty(dependsOnMeString))
            {
                if (!bool.TryParse(dependsOnMeString, out dependsOnMe))
                {
                    throw new InvalidDataContractException("GetDependsOnMe is not valid boolean value");
                }
            }
            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            return dependsOnMe ? string.Format("<graph title=\"Dependants Graph Of {0}\">{1}</graph>", resourceName, FindWhatDependsOnMe(resourceName, theWorkspace.ID)) : string.Format("<graph title=\"Dependency Graph Of {0}\">{1}</graph>", resourceName, FindDependenciesRecursive(resourceName, theWorkspace.ID));
        }

        string FindWhatDependsOnMe(string resourceName, Guid workspaceID)
        {
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, resourceName);
            var sb = new StringBuilder();
            if (dependants != null)
            {
                dependants.ForEach(c =>
                {
                    var resource = ResourceCatalog.Instance.GetResource(workspaceID, c);
                    var brokenString = "false";
                    if(resource == null)
                    {
                        brokenString = "true";
                    }
                    sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"{1}\">", c,brokenString));
                    sb.Append(string.Format("<dependency id=\"{0}\" />", resourceName));
                    sb.Append("</node>");
                });
               
            }
//            if (dependencies != null)
//            {
//                dependencies.ToList().ForEach(c => sb.Append(FindDependenciesRecursive(c.ResourceName, workspaceID)));
//            }
            sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"false\">", resourceName));
            sb.Append("</node>");
            var findDependenciesRecursive = sb.ToString();
            return findDependenciesRecursive;
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
                DataListSpecification = @"<DataList><ResourceName/><GetDependsOnMe/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>"
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
        private string FindDependenciesRecursive(string resourceName, Guid workspaceID)
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
            sb.Append(string.Format("<node id=\"{0}\" x=\"\" y=\"\" broken=\"{1}\">", resourceName,brokenString));
            sb.Append("</node>");
            var findDependenciesRecursive = sb.ToString();
            return findDependenciesRecursive;
        }


        #endregion



    }
}
