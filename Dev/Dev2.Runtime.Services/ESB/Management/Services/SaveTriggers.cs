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
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Warewolf.Triggers;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveTriggers : EsbManagementEndpointBase
    {
        private ITriggersCatalog _triggersCatalog;
        private IResourceCatalog _resourceCatalog;

        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("resourceID", out StringBuilder tmp);
            if (tmp != null && Guid.TryParse(tmp.ToString(), out var resourceId))
            {
                return resourceId;
            }
            return Guid.Empty;
        }

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Triggers Service", GlobalConstants.WarewolfInfo);
                values.TryGetValue("resourceID", out var resourceIdString);
                if (resourceIdString == null)
                {
                    throw new InvalidDataContractException("resourceID is missing");
                }
                if (!Guid.TryParse(resourceIdString.ToString(), out var resourceId))
                {
                    throw new InvalidDataContractException("resourceID is not a valid GUID.");
                }
                values.TryGetValue("resourcePath", out var resourcePathString);
                if (resourcePathString == null)
                {
                    throw new InvalidDataContractException("resourcePath is missing");
                }
                values.TryGetValue("triggerDefinitions", out var triggerDefinitionMessage);
                if (triggerDefinitionMessage == null || triggerDefinitionMessage.Length == 0)
                {
                    throw new InvalidDataContractException("triggerDefinitions is missing");
                }
                var res = new ExecuteMessage
                {
                    HasError = false,
                    Message = serializer.SerializeToBuilder("")
                };

                var decompressedMessage = serializer.Deserialize<CompressedExecuteMessage>(triggerDefinitionMessage).GetDecompressedMessage();
                var triggerQueues = serializer.Deserialize<List<ITriggerQueue>>(decompressedMessage);
                var resource = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
                if (resource == null)
                {
                    var message = $"Resource {resourcePathString} has been deleted. No Triggers can be saved for this resource.";
                    res.Message = serializer.SerializeToBuilder(message);
                }
                else
                {
                    var resourcePath = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID);
                    if (!resourcePath.Equals(resourcePathString.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        var message = $"Resource {resourcePathString} has changed to {resourcePath}. Triggers have been saved for this resource.";
                        res.Message = serializer.SerializeToBuilder(message);
                    }
                    TriggersCatalog.SaveTriggers(resourceId, triggerQueues);
                }
                return serializer.SerializeToBuilder(res);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                var res = new ExecuteMessage { HasError = true, Message = new StringBuilder(err.Message) };
                return serializer.SerializeToBuilder(res);
            }
        }

        public ITriggersCatalog TriggersCatalog
        {
            get => _triggersCatalog ?? Hosting.TriggersCatalog.Instance;
            set => _triggersCatalog = value;
        }

        public IResourceCatalog ResourceCatalog
        {
            get => _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }

        public override DynamicService CreateServiceEntry()
        {
            var newDs = new DynamicService { Name = HandlesType() };
            var sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public override string HandlesType() => nameof(SaveTriggers);
    }
}