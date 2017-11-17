/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchDependantCompileMessages : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string serviceId = null;
            string workspaceId = null;

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var result = new ExecuteMessage { HasError = false };

            values.TryGetValue("ServiceID", out StringBuilder tmp);
            if (tmp != null)
            {
                serviceId = tmp.ToString();
            }
            values.TryGetValue("WorkspaceID", out tmp);
            if(tmp != null)
            {
                workspaceId = tmp.ToString();
            }
            values.TryGetValue("FilterList", out tmp);

            if(string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(workspaceId))
            {
                throw new InvalidDataContractException(ErrorResource.NullServiceIDOrWorkspaceID);
            }

            Guid.TryParse(workspaceId, out Guid wGuid);
            Guid.TryParse(serviceId, out Guid sGuid);


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
                    return resource==null?"": resource.GetResourcePath(wGuid);
                });
                var deps = enumerable.Distinct().ToList();
                if(deps.Count > 0)
                {
                    
                    msgs = new CompileMessageList();
                    var compileMessageTo = new CompileMessageTO
                    {
                        ErrorType = ErrorType.Critical, 
                        MessageType = CompileMessageType.MappingChange
                    };
                    msgs.Dependants =new List<string>();
                    deps.ForEach(s => msgs.Dependants.Add(s));
                    msgs.MessageList = new List<ICompileMessageTO> { compileMessageTo };
                    
                    return serializer.SerializeToBuilder(msgs);
                }
            }
            else
            {
                result.Message.Append(string.Format(ErrorResource.CouldNotLocateService, sGuid));
            }

            return serializer.SerializeToBuilder(msgs);
        }

        public override DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = new StringBuilder("<DataList><ServiceID ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><FilterList ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
            };
            using (ServiceAction sa = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            })
            {
                newDs.Actions.Add(sa);
                return newDs;
            }
        }

        public override string HandlesType()
        {
            return "FetchDependantCompileMessagesService";
        }
    }
}
