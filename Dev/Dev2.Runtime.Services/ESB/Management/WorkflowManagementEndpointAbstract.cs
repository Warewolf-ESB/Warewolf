﻿using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Runtime.ESB.Management
{
    public abstract class WorkflowManagementEndpointAbstract : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("resourceID", out StringBuilder tmp);
            if (tmp != null && Guid.TryParse(tmp.ToString(), out Guid resourceId))
            {
                return resourceId;
            }

            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;

        public virtual StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Workflow Resume Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue("resourceID", out StringBuilder resourceIdString);
                if (resourceIdString == null)
                {
                    throw new InvalidDataContractException("resourceID is missing");
                }
                if (!Guid.TryParse(resourceIdString.ToString(), out Guid resourceId))
                {
                    throw new InvalidDataContractException("resourceID is not a valid GUID.");
                }
                var message = ExecuteImpl(serializer, resourceId,values);

                return serializer.SerializeToBuilder(message);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                var res = new ExecuteMessage { HasError = true, Message = new StringBuilder(err.Message) };
                return serializer.SerializeToBuilder(res);
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService { Name = HandlesType() };
            var sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);
            return newDs;
        }


        abstract protected ExecuteMessage ExecuteImpl(Dev2JsonSerializer serializer, Guid resourceId, Dictionary<string, StringBuilder> values);
        abstract public string HandlesType();
    }
}
