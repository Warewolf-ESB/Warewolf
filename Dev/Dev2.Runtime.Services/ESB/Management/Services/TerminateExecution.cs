using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Execution;

namespace Dev2.Runtime.ESB.Management
{
    public class TerminateExecution : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, Workspaces.IWorkspace theWorkspace)
        {
            string resourceIDString = null;

            StringBuilder tmp;
            values.TryGetValue("ResourceID", out tmp);
            
            if (tmp != null)
            {
                resourceIDString = tmp.ToString();
            }

            if(resourceIDString == null)
            {
                throw new InvalidDataContractException("ResourceID is missing");
            }

            var res = new ExecuteMessage { HasError = false };

            Guid resourceID;
            var hasResourceID = Guid.TryParse(resourceIDString, out resourceID);
            if(!hasResourceID)
            {
                res.SetMessage(Resources.CompilerError_TerminationFailed);
                res.HasError = true;
            }
            var service = ExecutableServiceRepository.Instance.Get(theWorkspace.ID, resourceID);
            if(service == null)
            {
                res.SetMessage(Resources.CompilerError_TerminationFailed);
                res.HasError = true;
            }

            if(service != null)
            {
                service.Terminate();
                res.SetMessage(Resources.CompilerMessage_TerminationSuccess);
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(res);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<DataList><Roles ColumnIODirection=\"Input\"/><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
            ServiceAction sa = new ServiceAction();
            sa.Name = HandlesType();
            sa.ActionType = enActionType.InvokeManagementDynamicService;
            sa.SourceMethod = HandlesType();
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "TerminateExecutionService";
        }
    }
}
