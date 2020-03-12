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

using Dev2.Common;
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
    public abstract class WorkflowManagementEndpointAbstract : EsbManagementEndpointBase
    {
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("resourceID", out StringBuilder tmp);
            if (tmp != null && Guid.TryParse(tmp.ToString(), out Guid resourceId))
            {
                return resourceId;
            }

            return Guid.Empty;
        }

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
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

        public override DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService { Name = HandlesType() };
            var sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);
            return newDs;
        }


        abstract protected ExecuteMessage ExecuteImpl(Dev2JsonSerializer serializer, Guid resourceId, Dictionary<string, StringBuilder> values);
        abstract public override string HandlesType();
    }
}
