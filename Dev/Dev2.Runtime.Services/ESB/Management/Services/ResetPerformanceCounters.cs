#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class ResetPerformanceCounters : EsbManagementEndpointBase
    {
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override  AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override  StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();

            var serializer = new Dev2JsonSerializer();
            try
            {
                Manager.ResetCounters();
                msg.HasError = false;
                msg.Message = new StringBuilder();
            }
            catch(Exception err)
            {

                msg.HasError = true;
                msg.Message = new StringBuilder( err.Message);
            }

            return serializer.SerializeToBuilder(msg);
        }

        IPerformanceCounterRepository Manager => CustomContainer.Get<IPerformanceCounterRepository>();

        public override  DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public  override string HandlesType() => "ResetPerformanceCounters";
    }
}