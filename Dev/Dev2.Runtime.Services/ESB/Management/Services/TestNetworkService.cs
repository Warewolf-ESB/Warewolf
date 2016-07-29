using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TestNetworkService : IEsbManagementEndpoint
    {
        public TestNetworkService()
        {
            Now = () => DateTime.Now;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage { HasError = false };

            msg.SetMessage(values["payload"].ToString());

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

            return serializer.SerializeToBuilder(msg);
        }

        public Func<DateTime> Now { get; set; }

        public string HandlesType()
        {
            return "TestNetworkService";
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService ds = new DynamicService { Name = HandlesType() };

            ServiceAction action = new ServiceAction
            {
                Name = HandlesType(),
                SourceMethod = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };

            ds.Actions.Add(action);

            return ds;
        }
    }
}