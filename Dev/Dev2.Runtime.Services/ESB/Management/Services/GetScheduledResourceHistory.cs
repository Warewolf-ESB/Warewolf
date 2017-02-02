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
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Scheduler;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetScheduledResourceHistory : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        private IServerSchedulerFactory _schedulerFactory;
        ISecurityWrapper _securityWrapper;
        private IResourceCatalog _catalog;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {


                StringBuilder tmp;
                values.TryGetValue("Resource", out tmp);
                var serializer = new Dev2JsonSerializer();

                if (tmp != null)
                {
                    var res = serializer.Deserialize<IScheduledResource>(tmp);
                    Dev2Logger.Info("Get Scheduled History. " +tmp);
                    IList<IResourceHistory> resources;
                    using (var model = SchedulerFactory.CreateModel(GlobalConstants.SchedulerFolderId, SecurityWrapper))
                    {
                        resources = model.CreateHistory(res);
                    }
                    return serializer.SerializeToBuilder(resources);
                }
                Dev2Logger.Debug("No resource Provided");
                return serializer.SerializeToBuilder(new List<IResourceHistory>());
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var getResourceHistory = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };

            var getHistoryAction = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };


            getResourceHistory.Actions.Add(getHistoryAction);

            return getResourceHistory;
        }

        public string HandlesType()
        {
            return "GetScheduledResourceHistoryService";
        }
        public IServerSchedulerFactory SchedulerFactory
        {
            get { return _schedulerFactory ?? new ServerSchedulerFactory(a => ResourceCatalogue.GetResourcePath(GlobalConstants.ServerWorkspaceID, a.ResourceId)); }
            set { _schedulerFactory = value; }
        }

        public IResourceCatalog ResourceCatalogue
        {
            get { return _catalog ?? ResourceCatalog.Instance; }
            set { _catalog = value; }
        }

        public ISecurityWrapper SecurityWrapper
        {
            get
            {
                return _securityWrapper ?? new SecurityWrapper(ServerAuthorizationService.Instance);
            }
            set
            {
                _securityWrapper = value;
            }
        }
    }
}
