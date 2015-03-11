
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
            string resourceDefinition;

            values.TryGetValue("Roles", out roles);
            values.TryGetValue("ResourceXml", out resourceDefinition);

            resourceDefinition = resourceDefinition.Unescape();

            if (string.IsNullOrEmpty(roles) || string.IsNullOrEmpty(resourceDefinition))
            {
                throw new InvalidDataContractException("Roles or ResourceXml is missing");
            }

            var compiledResources = DynamicObjectHelper.GenerateObjectGraphFromString(resourceDefinition);
            if (compiledResources.Count == 0)
            {
                return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerError_TerminationFailed);
            }

            var resource = compiledResources.First() as DynamicService;
            if (resource == null)
            {
                return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerError_TerminationFailed);
            }

            var service = ExecutableServiceRepository.Instance.Get(theWorkspace.ID, resource.ID);
            if (service == null)
            {
                return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerError_TerminationFailed);
            }

            var task = service.Terminate();
            task.Wait();

            return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerMessage_TerminationSuccess);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<DataList><Roles/><ResourceXml/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
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
