using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SavePerformanceCounters : IEsbManagementEndpoint
    {
        private IPerformanceCounterRepository _manager;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                Manager.Save(serializer.Deserialize<IPerformanceCounterTo>(values["PerformanceCounterTo"]));
                msg.HasError = false;
                msg.Message = new StringBuilder();
            }
            catch(Exception err)
            {

                msg.HasError = true;
                msg.Message = new StringBuilder( err.Message);
            }

            return serializer.SerializeToBuilder(msg);
        }
        
        public IPerformanceCounterRepository Manager
        {
            private get => _manager ?? CustomContainer.Get<IPerformanceCounterRepository>();
            set => _manager = value;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><PerformanceCounterTo ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "SavePerformanceCounters";
    }
}
