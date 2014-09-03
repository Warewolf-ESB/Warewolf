using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Security;
using Dev2.Scheduler;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetScheduledResourceHistory : IEsbManagementEndpoint
    {
        private IServerSchedulerFactory _schedulerFactory;
        ISecurityWrapper _securityWrapper;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, Workspaces.IWorkspace theWorkspace)
        {
            try
            {


                StringBuilder tmp;
                values.TryGetValue("Resource", out tmp);
                var serializer = new Dev2JsonSerializer();

                if (tmp != null)
                {
                    var res = serializer.Deserialize<IScheduledResource>(tmp);
                    Dev2Logger.Log.Info("Get Scheduled History. " +tmp);
                    IList<IResourceHistory> resources;
                    using (var model = SchedulerFactory.CreateModel(GlobalConstants.SchedulerFolderId, SecurityWrapper))
                    {
                        resources = model.CreateHistory(res);
                    }
                    return serializer.SerializeToBuilder(resources);
                }
                Dev2Logger.Log.Debug("No resource Provided");
                return serializer.SerializeToBuilder(new List<IResourceHistory>());
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error(e);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var getResourceHistory = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification =
                    "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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
            get { return _schedulerFactory ?? new ServerSchedulerFactory(); }
            set { _schedulerFactory = value; }
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
