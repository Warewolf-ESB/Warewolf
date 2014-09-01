using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Newtonsoft.Json;
using ServiceStack.Common.Extensions;

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
                    var resource = ResourceCatalog.Instance.GetResource(theWorkspace.ID, resourceName);
                    if(resource != null)
                    {
                        dependancyNames.AddRange(FetchRecursiveDependancies(resource.ResourceID, theWorkspace.ID));
                    }
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

        private IEnumerable<string> FetchRecursiveDependancies(Guid resourceId, Guid workspaceId)
        {
            List<string> results = new List<string>();
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceId);
            if(resource != null)
            {
                var dependencies = resource.Dependencies;
                if(dependencies != null)
                {
// ReSharper disable ImplicitlyCapturedClosure
                    dependencies.ForEach(c =>
// ReSharper restore ImplicitlyCapturedClosure
                    {
                        if(c.ResourceID != Guid.Empty)
                        {
                            results.Add(c.ResourceID.ToString());
                        }
                    });
                    dependencies.ToList().ForEach(c => results.AddRange(FetchRecursiveDependancies(c.ResourceID, workspaceId)));
                }
            }
            return results;
        }

        #endregion
    }
}
