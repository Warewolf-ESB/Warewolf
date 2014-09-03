using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
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
            try
            {

         
            Dev2Logger.Log.Info("Fetch Remote Debug Messages");
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
            catch (Exception err)
            {
                Dev2Logger.Log.Error("Fetch Remote Debug Messages Error", err);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><InvokerID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "FetchRemoteDebugMessagesService";
        }
    }
}
