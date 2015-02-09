using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.ToolBox;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetToolsService : IEsbManagementEndpoint
    {
        private IToolManager _serverToolManager;
       
        public string HandlesType()
        {
            return "FetchToolsService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {

            var serializer = new Dev2JsonSerializer();

            return serializer.SerializeToBuilder(ServerToolManager.LoadTools());
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public IToolManager ServerToolManager
        {
            get
            {
                return _serverToolManager ?? new ServerToolRepository(new List<string> { AppDomain.CurrentDomain.BaseDirectory + "\\Dev2.Activities.dll" }, new List<string>());
            }
            set { _serverToolManager = value; }
        }
    }
}