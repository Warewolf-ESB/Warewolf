using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Basic sanity test ;)
    /// </summary>
    public class Ping : IEsbManagementEndpoint
    {
        public Ping()
        {
            Now = () => DateTime.Now;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage {HasError = false};

            msg.SetMessage("Pong @ " + Now.Invoke().ToString("yyyy-MM-dd hh:mm:ss.fff"));

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            return serializer.SerializeToBuilder(msg);
        }

        public Func<DateTime> Now { get; set; }

        public string HandlesType()
        {
            return "Ping";
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService ds = new DynamicService {Name = HandlesType()};

            ServiceAction action = new ServiceAction
                {
                    Name = HandlesType(),
                    SourceMethod = HandlesType(),
                    ActionType = enActionType.InvokeManagementDynamicService,
                    DataListSpecification =
                        "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
                };

            ds.Actions.Add(action);

            return ds;
        }
    }
}
