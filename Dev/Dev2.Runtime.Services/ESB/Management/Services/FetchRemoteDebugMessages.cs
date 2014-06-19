using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Communication;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Internal service to fetch compile time messages
    /// </summary>
    public class FetchRemoteDebugMessages : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string invokerID = null;
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            StringBuilder tmp;
            values.TryGetValue("InvokerID", out tmp);
            if(tmp != null)
            {
                invokerID = tmp.ToString();
            }

            if(string.IsNullOrEmpty(invokerID))
            {
                throw new InvalidDataContractException("Null or empty ServiceID or WorkspaceID");
            }

            Guid iGuid;
            // RemoteDebugMessageRepo
            Guid.TryParse(invokerID, out iGuid);

            if(iGuid != Guid.Empty)
            {
                var items = RemoteDebugMessageRepo.Instance.FetchDebugItems(iGuid);

                return serializer.SerializeToBuilder(items);
            }

            return new StringBuilder();
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<DataList><InvokerID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
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
