
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
                    DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
                };

            ds.Actions.Add(action);

            return ds;
        }
    }
}
