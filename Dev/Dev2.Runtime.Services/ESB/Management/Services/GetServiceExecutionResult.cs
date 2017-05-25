using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetServiceExecutionResult : LogDataServiceBase, IEsbManagementEndpoint
    {
        public string HandlesType()
        {
            return "GetServiceExecutionService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var executionId = values["ExecutionId"];
            var serializer = new Dev2JsonSerializer();
            var tmpObjects = BuildTempObjects();
            var single = tmpObjects.Single(o => o.Message.StartsWith("Execution Result") && o.ExecutionId?.Equals(executionId.ToString()) ?? false);
            var replace = single.Message.Replace("Execution Result [ ", "").Replace(" ]", "");
            var logEntry = new LogEntry { Result = replace };
            return serializer.SerializeToBuilder(logEntry);
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }
       
    }
}