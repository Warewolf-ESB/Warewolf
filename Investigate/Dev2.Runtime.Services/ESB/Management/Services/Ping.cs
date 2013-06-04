using System;
using System.Collections.Generic;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Basic sanity test ;)
    /// </summary>
    public class Ping : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            return "Pong @ " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
        }

        public string HandlesType()
        {
            return "Reconcile";
        }


        public DynamicService CreateServiceEntry()
        {
            DynamicService ds = new DynamicService();

            ds.Name = HandlesType();
            ServiceAction action = new ServiceAction();
            action.Name = HandlesType();
            action.SourceMethod = HandlesType();
            action.ActionType = enActionType.InvokeManagementDynamicService;
            action.DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";

            ds.Actions.Add(action);

            return ds;
        }
    }
}
