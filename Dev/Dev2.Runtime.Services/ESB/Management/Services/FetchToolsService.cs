using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class FetchToolsService : DefaultEsbManagementEndpoint
    {
        private IToolManager _serverToolManager;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(ServerToolManager.LoadTools());
        }

        public IToolManager ServerToolManager
        {
            get
            {
                var internalToolsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Dev2.Activities.dll");
                return _serverToolManager ?? new ServerToolRepository(new List<string> { internalToolsPath }, new List<string>());
            }
            set => _serverToolManager = value;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchToolsService";
    }
}