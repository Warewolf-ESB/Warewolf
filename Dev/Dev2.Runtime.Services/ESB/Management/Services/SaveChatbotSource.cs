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
using Warewolf.Data;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveChatbotSource : IEsbManagementEndpoint
    {
        IResourceCatalog _resourceCatalog;

        public const string ChatbotSource = "ChatbotSource";

        public SaveChatbotSource()
        {

        }

        public SaveChatbotSource(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ChatbotSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Save Chatbot Source Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue(ChatbotSource, out StringBuilder resourceDefinition);

                IChatbotSource chatbotSourceDef = serializer.Deserialize<ChatbotSourceDefinition>(resourceDefinition);

                if (chatbotSourceDef.Path == null)
                {
                    chatbotSourceDef.Path = string.Empty;
                }

                if (chatbotSourceDef.Path.EndsWith("\\"))
                {
                    chatbotSourceDef.Path = chatbotSourceDef.Path.Substring(0, chatbotSourceDef.Path.LastIndexOf("\\", StringComparison.Ordinal));
                }

                var chatbotSource = new Data.ServiceModel.ChatbotSource
                {
                    ResourceID = chatbotSourceDef.Id,
                    ApiKey = chatbotSourceDef.ApiKey,
                    CompletionsEndpoint = chatbotSourceDef.CompletionsEndpoint,
                    ModelsEndpoint = chatbotSourceDef.ModelsEndpoint,
                    SelectedModel = chatbotSourceDef.SelectedModel,
                    Provider = chatbotSourceDef.Provider,
                    ResourceName = chatbotSourceDef.Name
                };

                ResourceCat.SaveResource(GlobalConstants.ServerWorkspaceID, chatbotSource, chatbotSourceDef.Path);

                // If this source is the one currently configured in chatbot settings,
                // refresh the encrypted payload so SendChatbotMessage uses the updated source definition.
                var chatbotSettings = Config.Chatbot;
                if (chatbotSettings.ChatbotSource != null && chatbotSettings.ChatbotSource.Value == chatbotSourceDef.Id)
                {
                    var payload = serializer.Serialize(chatbotSource);
                    chatbotSettings.ChatbotSource = new NamedGuidWithEncryptedPayload
                    {
                        Name = chatbotSource.ResourceName,
                        Value = chatbotSource.ResourceID,
                        Payload = DpapiWrapper.Encrypt(payload)
                    };
                }

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

        public string HandlesType() => nameof(SaveChatbotSource);
    }
}
