
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
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Execution;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ESB.Management
    // ReSharper restore CheckNamespace
{
    public class TerminateExecution : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, Workspaces.IWorkspace theWorkspace)
        {
            string resourceIdString = null;

            StringBuilder tmp;
            values.TryGetValue("ResourceID", out tmp);
            
            if (tmp != null)
            {
                resourceIdString = tmp.ToString();
            }

            if(resourceIdString == null)
            {
                throw new InvalidDataContractException("ResourceID is missing");
            }

            var res = new ExecuteMessage { HasError = false };

            Guid resourceId;
            var hasResourceId = Guid.TryParse(resourceIdString, out resourceId);
            if(!hasResourceId)
            {
                res.SetMessage(Resources.CompilerError_TerminationFailed);
                res.HasError = true;
            }
            var service = ExecutableServiceRepository.Instance.Get(theWorkspace.ID, resourceId);
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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "TerminateExecutionService";
        }
    }
}
