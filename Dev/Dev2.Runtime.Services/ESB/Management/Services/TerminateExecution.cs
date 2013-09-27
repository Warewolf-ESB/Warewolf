using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.ExtMethods;
using Dev2.DynamicServices;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.ESB.Management
{
    public class TerminateExecution : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, Workspaces.IWorkspace theWorkspace)
        {
            string roles;
            string resourceIDString;

            values.TryGetValue("Roles", out roles);
            values.TryGetValue("ResourceID", out resourceIDString);
            if (string.IsNullOrEmpty(roles) || string.IsNullOrEmpty(resourceIDString))
            {
                throw new InvalidDataContractException("Roles or ResourceID is missing");
            }
            Guid resourceID;
            var hasResourceID = Guid.TryParse(resourceIDString, out resourceID);
            if (!hasResourceID)
            {
                return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerError_TerminationFailed);
            }
            var service = ExecutableServiceRepository.Instance.Get(theWorkspace.ID, resourceID);
            if (service == null)
            {
                return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerError_TerminationFailed);
            }

            service.Terminate();

            return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerMessage_TerminationSuccess);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<DataList><Roles/><ResourceID/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
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
