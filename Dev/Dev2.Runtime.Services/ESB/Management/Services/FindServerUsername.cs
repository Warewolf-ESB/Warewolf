using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
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
            try
            {
                Dev2Logger.Log.Info("Find Server User Name");
         
                return new StringBuilder(Environment.UserDomainName + "\\" + Environment.UserName);
            }
            catch (Exception err)
            {
                Dev2Logger.Log.Error(err);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findServerUsernameService = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };

            ServiceAction findServerUsernameServiceAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceName = HandlesType(), SourceMethod = HandlesType() };

            findServerUsernameService.Actions.Add(findServerUsernameServiceAction);

            return findServerUsernameService;
        }

        public string HandlesType()
        {
            return "FindServerUsernameService";
        }
    }
}
