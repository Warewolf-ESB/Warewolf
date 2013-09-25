using System;
using Dev2.Common.ExtMethods;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>
    public class RenameResource : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string resourceID;
            string newName;
            if (values == null)
            {
                throw new InvalidDataContractException("No parameter values provided.");
            }
            values.TryGetValue("ResourceID", out resourceID);
            values.TryGetValue("NewName", out newName);

            if (resourceID == null)
            {
                throw new InvalidDataContractException("No value provided for ResourceID parameter.");
            }
            if (String.IsNullOrEmpty(newName))
            {
                throw new InvalidDataContractException("No value provided for NewName parameter.");
            }
            var saveToWorkSpaceResult = ResourceCatalog.Instance.RenameResource(theWorkspace.ID, resourceID, newName);
            if(saveToWorkSpaceResult.Status == ExecStatus.Success)
            {
                var saveToLocalServerResult = ResourceCatalog.Instance.RenameResource(Guid.Empty, resourceID, newName);
                if (saveToLocalServerResult.Status == ExecStatus.Success)
                {
                    return saveToLocalServerResult.Message;
                }
            }
            return saveToWorkSpaceResult.Message;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<DataList><ResourceID ColumnIODirection=\"Input\"/><NewName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
            ServiceAction sa = new ServiceAction();
            sa.Name = HandlesType();
            sa.ActionType = enActionType.InvokeManagementDynamicService;
            sa.SourceMethod = HandlesType();
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "RenameResourceService";
        }
    }
}
