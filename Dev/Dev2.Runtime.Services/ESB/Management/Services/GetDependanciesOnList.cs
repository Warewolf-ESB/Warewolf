using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Dev2.DynamicServices;
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
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            List<string> dependancyNames = new List<string>();
            
            bool dependsOnMe = false;
            string resourceNamesString;
            string dependsOnMeString;
            values.TryGetValue("ResourceNames", out resourceNamesString);
            values.TryGetValue("GetDependsOnMe", out dependsOnMeString);
            List<string> resourceNames = JsonConvert.DeserializeObject<List<string>>(resourceNamesString);            

            if (!string.IsNullOrEmpty(dependsOnMeString))
            {
                if (!bool.TryParse(dependsOnMeString, out dependsOnMe))
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
                foreach (string resourceName in resourceNames)
                {
                    dependancyNames.AddRange(FetchRecursiveDependancies(resourceName, theWorkspace.ID));
                }    
            }                      
            return JsonConvert.SerializeObject(dependancyNames);
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
                DataListSpecification = @"<DataList><ResourceNames/><GetDependsOnMe/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>"
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

        private List<string> FetchRecursiveDependancies(string resourceName,Guid workspaceID)
        {
            List<string> results = new List<string>();
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, resourceName);                        
            if (resource != null)
            {                
                var dependencies = resource.Dependencies;
                if (dependencies != null)
                {                    
                    dependencies.ForEach(c => results.Add( c.ResourceName));
                    dependencies.ToList().ForEach(c => results.AddRange(FetchRecursiveDependancies(c.ResourceName, workspaceID)));
                }
            }            
            return results;
        }

        #endregion
    }
}
