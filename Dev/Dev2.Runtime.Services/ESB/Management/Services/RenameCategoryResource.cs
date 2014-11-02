
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
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
    public class RenameResourceCategory : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {

            
            string oldCategory = null;
            string newCategory = null;
            string resourceType = null;
            if(values == null)
            {
                throw new InvalidDataContractException("No parameter values provided.");
            }
            StringBuilder tmp;
            values.TryGetValue("OldCategory", out tmp);
            if(tmp != null)
            {
                oldCategory = tmp.ToString();
            }
            values.TryGetValue("NewCategory", out tmp);
            if(tmp != null)
            {
                newCategory = tmp.ToString();
            }
            values.TryGetValue("ResourceType", out tmp);
            if(tmp != null)
            {
                resourceType = tmp.ToString();
            }

            if(oldCategory == null)
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
            Dev2Logger.Log.Info(String.Format( "Rename Category. Old {0} New {1} Type{2}",oldCategory,newCategory,resourceType));
            var saveResult = ResourceCatalog.Instance.RenameCategory(Guid.Empty, oldCategory, newCategory);

            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage(saveResult.Message);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><OldCategory ColumnIODirection=\"Input\"/><NewCategory ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "RenameResourceCategoryService";
        }
    }
}
