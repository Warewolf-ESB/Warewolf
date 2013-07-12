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
    public class RenameResourceCategory : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string oldCategory;
            string newCategory;
            string resourceType;
            if(values == null)
            {
                throw new InvalidDataContractException("No parameter values provided.");
            }
            values.TryGetValue("OldCategory", out oldCategory);
            values.TryGetValue("NewCategory", out newCategory);
            values.TryGetValue("ResourceType", out resourceType);

            if(oldCategory==null)
            {
                throw new InvalidDataContractException("No value provided for OldCategory parameter.");
            }
            if(String.IsNullOrEmpty(newCategory))
            {
                throw new InvalidDataContractException("No value provided for NewCategory parameter.");
            }
            if(String.IsNullOrEmpty(resourceType))
            {
                throw new InvalidDataContractException("No value provided for ResourceType parameter.");
            }
            var saveResult = ResourceCatalog.Instance.RenameCategory(Guid.Empty, oldCategory,newCategory,resourceType);
            return saveResult.Message;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<DataList><OldCategory ColumnIODirection=\"Input\"/><NewCategory ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
            ServiceAction sa = new ServiceAction();
            sa.Name = HandlesType();
            sa.ActionType = enActionType.InvokeManagementDynamicService;
            sa.SourceMethod = HandlesType();
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "RenameResourceCategoryService";
        }
    }
}
