/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Execution;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;
using Dev2.Runtime.ESB.Management.Services;

namespace Dev2.Runtime.ESB.Management
{
    public class TerminateExecution : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string resourceIdString = null;

            values.TryGetValue("ResourceID", out StringBuilder tmp);

            if (tmp != null)
            {
                resourceIdString = tmp.ToString();
            }

            if(resourceIdString == null)
            {
                throw new InvalidDataContractException(ErrorResource.ResourceIdIsNull);
            }

            var res = new ExecuteMessage { HasError = false };

            var hasResourceId = Guid.TryParse(resourceIdString, out Guid resourceId);
            if (!hasResourceId)
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

        public override DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public override string HandlesType()
        {
            return "TerminateExecutionService";
        }
    }
}
