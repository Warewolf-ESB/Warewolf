using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Scheduler;
using Dev2.Scheduler.Interfaces;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetScheduledResources : IEsbManagementEndpoint
    {
        private IServerSchedulerFactory _schedulerFactory;

        public string HandlesType()
        {
            return "GetScheduledResources";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ObservableCollection<IScheduledResource> resources;
            using (var model = SchedulerFactory.CreateModel(GlobalConstants.SchedulerFolderId))
            {
                resources = model.GetScheduledResources();
            }

            var sb = new StringBuilder( JsonConvert.SerializeObject(resources, Formatting.Indented, new JsonSerializerSettings
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
            DynamicService getScheduledResourcesService = new DynamicService();
            getScheduledResourcesService.Name = HandlesType();
            getScheduledResourcesService.DataListSpecification = "<DataList></DataList>";


            ServiceAction getScheduledResourcesAction = new ServiceAction();
            getScheduledResourcesAction.Name = HandlesType();
            getScheduledResourcesAction.ActionType = enActionType.InvokeManagementDynamicService;
            getScheduledResourcesAction.SourceName = HandlesType();
            getScheduledResourcesAction.SourceMethod = HandlesType();

            getScheduledResourcesService.Actions.Add(getScheduledResourcesAction);

            return getScheduledResourcesService;
        }
    }
}
