using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find Server Username
    /// </summary>
    public class FindServerUsername : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            return new StringBuilder(Environment.UserDomainName + "\\" + Environment.UserName);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findServerUsernameService = new DynamicService();
            findServerUsernameService.Name = HandlesType();
            findServerUsernameService.DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
            

            ServiceAction findServerUsernameServiceAction = new ServiceAction();
            findServerUsernameServiceAction.Name = HandlesType();
            findServerUsernameServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findServerUsernameServiceAction.SourceName = HandlesType();
            findServerUsernameServiceAction.SourceMethod = HandlesType();

            findServerUsernameService.Actions.Add(findServerUsernameServiceAction);

            return findServerUsernameService;
        }

        public string HandlesType()
        {
            return "FindServerUsernameService";
        }
    }
}
