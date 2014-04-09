using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Security;
using Dev2.Scheduler;
using Dev2.Scheduler.Interfaces;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetScheduledResources : IEsbManagementEndpoint
    {
        private IServerSchedulerFactory _schedulerFactory;
        ISecurityWrapper _securityWrapper;

        public string HandlesType()
        {
            return "GetScheduledResources";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ObservableCollection<IScheduledResource> resources;
            using(var model = SchedulerFactory.CreateModel(GlobalConstants.SchedulerFolderId, SecurityWrapper))
            {
                resources = model.GetScheduledResources();
            }

            var sb = new StringBuilder(JsonConvert.SerializeObject(resources, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            }));
            return sb;
        }

        public IServerSchedulerFactory SchedulerFactory
        {
            get { return _schedulerFactory ?? new ServerSchedulerFactory(); }
            set { _schedulerFactory = value; }
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService getScheduledResourcesService = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList></DataList>" };

            ServiceAction getScheduledResourcesAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceName = HandlesType(), SourceMethod = HandlesType() };

            getScheduledResourcesService.Actions.Add(getScheduledResourcesAction);

            return getScheduledResourcesService;
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
