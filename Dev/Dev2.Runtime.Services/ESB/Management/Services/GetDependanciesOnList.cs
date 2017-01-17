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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Newtonsoft.Json;
using ServiceStack.Common.Extensions;

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
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

            try
            {

         
            List<string> dependancyNames = new List<string>();

            bool dependsOnMe = false;
            string resourceIdsString = string.Empty;
            string dependsOnMeString = string.Empty;
            StringBuilder tmp;
            values.TryGetValue("ResourceIds", out tmp);
            if(tmp != null)
            {
                resourceIdsString = tmp.ToString();
            }
            values.TryGetValue("GetDependsOnMe", out tmp);
            if(tmp != null)
            {
                dependsOnMeString = tmp.ToString();
            }

            IEnumerable<Guid> resourceIds = JsonConvert.DeserializeObject<List<string>>(resourceIdsString).Select(Guid.Parse);
            Dev2Logger.Info("Get Dependencies On List. " + resourceIdsString);
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
                foreach (var resourceId in resourceIds)
                {

                    dependancyNames.AddRange(FetchRecursiveDependancies(resourceId, theWorkspace.ID));

                }
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(dependancyNames);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                throw;
            }
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
                DataListSpecification = new StringBuilder("<DataList><ResourceNames ColumnIODirection=\"Input\"/><GetDependsOnMe ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
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

            var dependencies = resource?.Dependencies;

            if(dependencies != null)
            {
// ReSharper disable ImplicitlyCapturedClosure
                dependencies.ForEach(c =>
// ReSharper restore ImplicitlyCapturedClosure
                    { results.Add(c.ResourceID != Guid.Empty ? c.ResourceID.ToString() : c.ResourceName); });
                dependencies.ToList().ForEach(c =>
                    { results.AddRange(c.ResourceID != Guid.Empty ? FetchRecursiveDependancies(c.ResourceID, workspaceId) : FetchRecursiveDependancies(workspaceId, c.ResourceName)); });
            }
            return results;
        }

        IEnumerable<string> FetchRecursiveDependancies(Guid workspaceId, string resourceName)
        {
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceName);
            if (resource != null)
            {
                return FetchRecursiveDependancies(resource.ResourceID, workspaceId);
            }
            return new List<string>();
        }

        #endregion

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }
    }
}
