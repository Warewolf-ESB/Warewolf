using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>
    public class RenameResource : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {

            var res = new ExecuteMessage {HasError = false};

            string resourceID = null;
            string newName = null;
            if(values == null)
            {
                throw new InvalidDataContractException("No parameter values provided.");
            }

            StringBuilder tmp;
            values.TryGetValue("ResourceID", out tmp);
            if (tmp != null)
            {
                resourceID = tmp.ToString();
            }
            values.TryGetValue("NewName", out tmp);
            if(tmp != null)
            {
                newName = tmp.ToString();
            }

            if(resourceID == null)
            {
                throw new InvalidDataContractException("No value provided for ResourceID parameter.");
            }
            if(String.IsNullOrEmpty(newName))
            {
                throw new InvalidDataContractException("No value provided for NewName parameter.");
            }

            Guid id;
            Guid.TryParse(resourceID, out id);
            var saveToWorkSpaceResult = ResourceCatalog.Instance.RenameResource(theWorkspace.ID, id, newName);
            if (saveToWorkSpaceResult.Status == ExecStatus.Success)
            {
                var saveToLocalServerResult = ResourceCatalog.Instance.RenameResource(Guid.Empty, id, newName);
                if (saveToLocalServerResult.Status == ExecStatus.Success)
                {
                    res.SetMessage(saveToLocalServerResult.Message);
                }
            }
            else
            {
                res.HasError = true;
            }

            res.SetMessage(saveToWorkSpaceResult.Message);

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(res);
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
