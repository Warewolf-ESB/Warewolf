using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Internal service to fetch compile time messages
    /// </summary>
    public class FetchCompileMessages : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string serviceID;
            string workspaceID;
            string filterList;

            StringBuilder result = new StringBuilder();

            values.TryGetValue("ServiceID", out serviceID);
            values.TryGetValue("WorkspaceID", out workspaceID);
            values.TryGetValue("FilterList", out filterList);

            if (string.IsNullOrEmpty(serviceID) || string.IsNullOrEmpty(workspaceID))
            {
                throw new InvalidDataContractException("Null or empty ServiceID or WorkspaceID");  
            }

            Guid wGuid;
            Guid sGuid;

            Guid.TryParse(workspaceID, out wGuid);
            Guid.TryParse(serviceID, out sGuid);


            var thisService = ResourceCatalog.Instance.GetResource(wGuid, sGuid);

            if (thisService != null)
            {
                var deps = thisService.Dependencies;

                CompileMessageType[] filters = null; // TODO : Convert string list to enum array ;)

                CompileMessageList msgs = CompileMessageRepo.Instance.FetchMessages(wGuid, sGuid, deps, filters);

                result.Append(JsonConvert.SerializeObject(msgs));
            }
            else
            {
                result.Append("<Error> Could not locate service with ID [ " + sGuid + " ]</Error>");
            }

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<DataList><ServiceID/><WorkspaceID/><FilterList/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
            ServiceAction sa = new ServiceAction();
            sa.Name = HandlesType();
            sa.ActionType = enActionType.InvokeManagementDynamicService;
            sa.SourceMethod = HandlesType();
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "FetchCompileMessagesService";
        }
    }
}
