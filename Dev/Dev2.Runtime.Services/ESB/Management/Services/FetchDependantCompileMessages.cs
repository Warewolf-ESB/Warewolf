
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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using ServiceStack.Common.Extensions;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Internal service to fetch compile time messages
    /// </summary>
    public class FetchDependantCompileMessages : IEsbManagementEndpoint
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
            var msgs = new CompileMessageList();
            var dependants = new List<Guid>();
            if(thisService != null)
            {
                var workspaceGuids = WorkspaceRepository.Instance.GetWorkspaceGuids();
                workspaceGuids.ForEach(guid =>
                {
                    var union = dependants.Union(ResourceCatalog.Instance.GetDependants(guid, thisService.ResourceID));
                    dependants = union.ToList();
                });
                
                var enumerable = dependants.Select(a =>
                {
                    var resource = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID,a) ?? ResourceCatalog.Instance.GetResource(wGuid,a);
                    return resource==null?"": resource.ResourcePath;
                });
                var deps = enumerable.Distinct().ToList();
                if(deps.Count > 0)
                {
                    // ReSharper disable ExpressionIsAlwaysNull
                    msgs = new CompileMessageList();
                    var compileMessageTo = new CompileMessageTO
                    {
                        ErrorType = ErrorType.Critical, 
                        MessageType = CompileMessageType.MappingChange
                    };
                    msgs.Dependants =new List<string>();
                    deps.ForEach(s => msgs.Dependants.Add(s));
                    msgs.MessageList = new List<ICompileMessageTO> { compileMessageTo };
                    // ReSharper restore ExpressionIsAlwaysNull
                    return serializer.SerializeToBuilder(msgs);
                }
            }
            else
            {
                result.Message.Append("Could not locate service with ID [ " + sGuid + " ]");
            }

            return serializer.SerializeToBuilder(msgs);
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
            return "FetchDependantCompileMessagesService";
        }
    }
}
