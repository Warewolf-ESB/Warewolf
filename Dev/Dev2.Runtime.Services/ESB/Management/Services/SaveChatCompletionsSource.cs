/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveChatCompletionsSource : IEsbManagementEndpoint
    {
        IResourceCatalog _resourceCatalog;

        public const string ChatCompletionsSource = "ChatCompletionsSource";

        public SaveChatCompletionsSource()
        {

        }

        public SaveChatCompletionsSource(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ChatCompletionsSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Save Chat Completions Source Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue(ChatCompletionsSource, out StringBuilder resourceDefinition);

                IChatCompletionsSource chatCompletionsSourceDef = serializer.Deserialize<ChatCompletionsSourceDefinition>(resourceDefinition);

                if (chatCompletionsSourceDef.Path == null)
                {
                    chatCompletionsSourceDef.Path = string.Empty;
                }

                if (chatCompletionsSourceDef.Path.EndsWith("\\"))
                {
                    chatCompletionsSourceDef.Path = chatCompletionsSourceDef.Path.Substring(0, chatCompletionsSourceDef.Path.LastIndexOf("\\", StringComparison.Ordinal));
                }

                var chatCompletionsSource = new Data.ServiceModel.ChatCompletionsSource
                {
                    ResourceID = chatCompletionsSourceDef.Id,
                    ApiKey = chatCompletionsSourceDef.ApiKey,
                    CompletionsEndpoint = chatCompletionsSourceDef.CompletionsEndpoint,
                    ResourceName = chatCompletionsSourceDef.Name
                };

                ResourceCat.SaveResource(GlobalConstants.ServerWorkspaceID, chatCompletionsSource, chatCompletionsSourceDef.Path);
                msg.HasError = false;
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public IResourceCatalog ResourceCat
        {
            get => _resourceCatalog ?? ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public string HandlesType() => nameof(SaveChatCompletionsSource);
    }
}
