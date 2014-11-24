
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Internal service to fetch compile time messages
    /// </summary>
    public class FetchCompileMessages : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string serviceId = null;
            string workspaceId = null;

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var result = new ExecuteMessage { HasError = false };

            StringBuilder tmp;
            values.TryGetValue("ServiceID", out tmp);
            if(tmp != null)
            {
                serviceId = tmp.ToString();
            }
            values.TryGetValue("WorkspaceID", out tmp);
            if(tmp != null)
            {
                workspaceId = tmp.ToString();
            }
            values.TryGetValue("FilterList", out tmp);
            if(tmp != null)
            {
            }

            if(string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(workspaceId))
            {
                throw new InvalidDataContractException("Null or empty ServiceID or WorkspaceID");
            }

            Guid wGuid;
            Guid sGuid;

            Guid.TryParse(workspaceId, out wGuid);
            Guid.TryParse(serviceId, out sGuid);


            var thisService = ResourceCatalog.Instance.GetResource(wGuid, sGuid);

            if(thisService != null)
            {
                var deps = thisService.Dependencies;

                CompileMessageType[] filters = null; // TODO : Convert string list to enum array ;)

                // ReSharper disable ExpressionIsAlwaysNull
                CompileMessageList msgs = CompileMessageRepo.Instance.FetchMessages(wGuid, sGuid, deps, filters);
                // ReSharper restore ExpressionIsAlwaysNull

                result.Message.Append(serializer.SerializeToBuilder(msgs));
            }
            else
            {
                result.Message.Append("Could not locate service with ID [ " + sGuid + " ]");
                result.HasError = true;
            }

            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ServiceID ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><FilterList ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "FetchCompileMessagesService";
        }
    }
}
