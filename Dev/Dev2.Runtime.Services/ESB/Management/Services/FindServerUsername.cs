using System;
using System.Collections.Generic;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find Server Username
    /// </summary>
    public class FindServerUsername : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            return Environment.UserDomainName + "\\" + Environment.UserName;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findServerUsernameService = new DynamicService();
            findServerUsernameService.Name = HandlesType();
            findServerUsernameService.DataListSpecification = "<root></root>";
            

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
