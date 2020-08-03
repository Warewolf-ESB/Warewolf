#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Dev2.Runtime.Interfaces;
using Dev2.Common.Interfaces;
using Newtonsoft.Json;
using ServiceStack.Common.Extensions;
using Warewolf.Triggers;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetDependanciesOnList : DefaultEsbManagementEndpoint
    {
        IResourceCatalog _resourceCatalog;
        private ITestCatalog _testCatalog;
        private ITriggersCatalog _triggersCatalog;
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                var dependancyNames = new List<string>();
                var dependsOnMe = false;
                var resourceIdsString = string.Empty;
                var dependsOnMeString = string.Empty;
                values.TryGetValue("ResourceIds", out StringBuilder tmp);
                if (tmp != null)
                {
                    resourceIdsString = tmp.ToString();
                }
                values.TryGetValue("GetDependsOnMe", out tmp);
                if (tmp != null)
                {
                    dependsOnMeString = tmp.ToString();
                }

                var resourceIds = JsonConvert.DeserializeObject<List<string>>(resourceIdsString).Select(Guid.Parse);
                Dev2Logger.Info("Get Dependencies On List. " + resourceIdsString, GlobalConstants.WarewolfInfo);
                if (!string.IsNullOrEmpty(dependsOnMeString) && !bool.TryParse(dependsOnMeString, out dependsOnMe))
                {
                    dependsOnMe = false;
                }

                if (dependsOnMe)
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

                var serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(dependancyNames);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        IEnumerable<string> FetchRecursiveDependancies(Guid resourceId, Guid workspaceId)
        {
            var results = new List<string>();
            var resource = Hosting.ResourceCatalog.Instance.GetResource(workspaceId, resourceId);
            var dependencies = resource?.Dependencies;
            if (dependencies != null)
            {
                dependencies.ForEach(c => { results.Add(c.ResourceID != Guid.Empty ? c.ResourceID.ToString() : c.ResourceName); });
                dependencies.ToList().ForEach(c => { results.AddRange(c.ResourceID != Guid.Empty ? FetchRecursiveDependancies(c.ResourceID, workspaceId) : FetchRecursiveDependancies(workspaceId, c.ResourceName)); });
            }

            return results;
        }

        IEnumerable<string> FetchRecursiveDependancies(Guid workspaceId, string resourceName)
        {
            var resource = Hosting.ResourceCatalog.Instance.GetResource(workspaceId, resourceName);
            if (resource != null)
            {
                return FetchRecursiveDependancies(resource.ResourceID, workspaceId);
            }

            return new List<string>();
        }
        public ITriggersCatalog TriggersCatalog
        {
            get => _triggersCatalog ?? Hosting.TriggersCatalog.Instance;
            set => _triggersCatalog = value;
        }
        public ITestCatalog TestCatalog
        {
            private get => _testCatalog ?? Runtime.TestCatalog.Instance;
            set => _testCatalog = value;
        }

        public IResourceCatalog ResourceCatalog
        {
            private get => _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }
        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceNames ColumnIODirection=\"Input\"/><GetDependsOnMe ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");
        public override string HandlesType() => nameof(Warewolf.Service.GetDependanciesOnList);
    }
}