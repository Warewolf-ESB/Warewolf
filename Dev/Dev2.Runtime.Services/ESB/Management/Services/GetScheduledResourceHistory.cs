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
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Scheduler;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetScheduledResourceHistory : DefaultEsbManagementEndpoint
    {
        IServerSchedulerFactory _schedulerFactory;
        ISecurityWrapper _securityWrapper;
        IResourceCatalog _catalog;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                values.TryGetValue("Resource", out StringBuilder tmp);
                var serializer = new Dev2JsonSerializer();

                if (tmp != null)
                {
                    var res = serializer.Deserialize<IScheduledResource>(tmp);
                    Dev2Logger.Info("Get Scheduled History. " +tmp, GlobalConstants.WarewolfInfo);
                    IList<IResourceHistory> resources;
                    using (var model = SchedulerFactory.CreateModel(GlobalConstants.SchedulerFolderId, SecurityWrapper))
                    {
                        resources = model.CreateHistory(res);
                    }
                    return serializer.SerializeToBuilder(resources);
                }
                Dev2Logger.Debug("No resource Provided", GlobalConstants.WarewolfDebug);
                return serializer.SerializeToBuilder(new List<IResourceHistory>());
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public IServerSchedulerFactory SchedulerFactory
        {
            get => _schedulerFactory ?? new ServerSchedulerFactory(a => ResourceCatalogue.GetResourcePath(GlobalConstants.ServerWorkspaceID, a.ResourceId));
            set => _schedulerFactory = value;
        }

        public IResourceCatalog ResourceCatalogue
        {
            get => _catalog ?? ResourceCatalog.Instance;
            set => _catalog = value;
        }

        public ISecurityWrapper SecurityWrapper
        {
            get => _securityWrapper ?? new SecurityWrapper(ServerAuthorizationService.Instance);
            set => _securityWrapper = value;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetScheduledResourceHistoryService";
    }
}
