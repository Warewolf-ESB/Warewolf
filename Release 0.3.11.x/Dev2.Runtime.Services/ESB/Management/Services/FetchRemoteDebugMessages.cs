using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Internal service to fetch compile time messages
    /// </summary>
    public class FetchRemoteDebugMessages : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string invokerID;

            StringBuilder result = new StringBuilder();

            values.TryGetValue("InvokerID", out invokerID);

            if (string.IsNullOrEmpty(invokerID))
            {
                throw new InvalidDataContractException("Null or empty ServiceID or WorkspaceID");
            }

            Guid iGuid;
            // RemoteDebugMessageRepo
            Guid.TryParse(invokerID, out iGuid);

            if (iGuid != Guid.Empty)
            {
                var items = RemoteDebugMessageRepo.Instance.FetchDebugItems(iGuid);
                var tmp = JsonConvert.SerializeObject(items);
                result.Append(tmp);
            }

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<DataList><InvokerID/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
            ServiceAction sa = new ServiceAction();
            sa.Name = HandlesType();
            sa.ActionType = enActionType.InvokeManagementDynamicService;
            sa.SourceMethod = HandlesType();
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "FetchRemoteDebugMessagesService";
        }
    }
}
