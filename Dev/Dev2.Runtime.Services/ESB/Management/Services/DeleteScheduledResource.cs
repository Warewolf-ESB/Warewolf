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
using Dev2.Common.Interfaces.Enums;
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
    public class DeleteScheduledResource : IEsbManagementEndpoint
    {
        IServerSchedulerFactory _schedulerFactory;
        ISecurityWrapper _securityWrapper;
        IResourceCatalog _catalog;

        public DeleteScheduledResource()
        {
            
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var result = new ExecuteMessage { HasError = false };
            Dev2Logger.Info("Delete Scheduled Resource Service", GlobalConstants.WarewolfInfo);
            values.TryGetValue("Resource", out StringBuilder tmp);
            var serializer = new Dev2JsonSerializer();

            if (tmp != null)
            {
                var res = serializer.Deserialize<IScheduledResource>(tmp);
                Dev2Logger.Info("Delete Scheduled Resource Service." + res, GlobalConstants.WarewolfInfo);
                using (var model = SchedulerFactory.CreateModel(GlobalConstants.SchedulerFolderId, SecurityWrapper))
                {
                    model.DeleteSchedule(res);
                }
            }
            else
            {
                Dev2Logger.Info("Delete Scheduled Resource Service. No Resource Selected", GlobalConstants.WarewolfInfo);
                result.Message.Append("No Resource Selected");
                result.HasError = true;
            }
            return serializer.SerializeToBuilder(result);
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

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "DeleteScheduledResourceService";
    }
}
