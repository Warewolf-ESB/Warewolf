using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetDependanciesOnList : IEsbManagementEndpoint
    {
        #region Implementation of ISpookyLoadable<string>

        public string HandlesType()
        {
            return "GetDependanciesOnListService";
        }

        #endregion

        #region Implementation of IEsbManagementEndpoint

        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            List<string> dependancyNames = new List<string>();

            bool dependsOnMe = false;
            string resourceNamesString = string.Empty;
            string dependsOnMeString = string.Empty;
            StringBuilder tmp;
            values.TryGetValue("ResourceNames", out tmp);
            if(tmp != null)
            {
                resourceNamesString = tmp.ToString();
            }
            values.TryGetValue("GetDependsOnMe", out tmp);
            if(tmp != null)
            {
                dependsOnMeString = tmp.ToString();
            }

            List<string> resourceNames = JsonConvert.DeserializeObject<List<string>>(resourceNamesString);

            if(!string.IsNullOrEmpty(dependsOnMeString))
            {
                if(!bool.TryParse(dependsOnMeString, out dependsOnMe))
                {
                    dependsOnMe = false;
                }
            }
            if(dependsOnMe)
            {
                //TODO : other way
            }
            else
            {
                foreach(string resourceName in resourceNames)
                {
                    dependancyNames.AddRange(FetchRecursiveDependancies(resourceName, theWorkspace.ID));
                }
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(dependancyNames);
        }

        /// <summary>
        /// Creates the service entry.
        /// </summary>
        /// <returns></returns>
        public DynamicService CreateServiceEntry()
        {
            var ds = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><ResourceNames ColumnIODirection=\"Input\"/><GetDependsOnMe ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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

        #endregion

        #region Private Methods

        private List<string> FetchRecursiveDependancies(string resourceName, Guid workspaceID)
        {
            List<string> results = new List<string>();
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, resourceName);
            if(resource != null)
            {
                var dependencies = resource.Dependencies;
                if(dependencies != null)
                {
                    dependencies.ForEach(c => results.Add(c.ResourceID.ToString()));
                    dependencies.ToList().ForEach(c => results.AddRange(FetchRecursiveDependancies(c.ResourceName, workspaceID)));
                }
            }
            return results;
        }

        #endregion
    }
}
