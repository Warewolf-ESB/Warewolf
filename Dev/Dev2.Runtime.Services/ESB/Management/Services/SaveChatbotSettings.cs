/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Configuration;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveChatbotSettings : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Save Chatbot Settings Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue(Warewolf.Service.SaveChatbotSettings.ChatbotSettings, out StringBuilder settings);

                // Try to deserialize as ChatbotSettingsData first (full save with ChatbotSource)
                var chatbotSettingsData = serializer.Deserialize<ChatbotSettingsData>(settings);
                if (chatbotSettingsData != null && chatbotSettingsData.ChatbotSource != null)
                {
                    // Full save - update everything
                    Config.Chatbot.ChatbotSource = chatbotSettingsData.ChatbotSource;
                    Config.Chatbot.IncludeSystemLog = chatbotSettingsData.IncludeSystemLog;
                    Config.Chatbot.IncludeResourcesXaml = chatbotSettingsData.IncludeResourcesXaml;
                    Config.Chatbot.IncludeResourcesJson = chatbotSettingsData.IncludeResourcesJson;
                    Config.Chatbot.NumberOfLogLines = chatbotSettingsData.NumberOfLogLines;
                }
                else
                {
                    // If that fails, try as ChatbotSettingsTo (from SettingsWriteService)
                    var chatbotSettingsTo = serializer.Deserialize<Dev2.Services.Chatbot.ChatbotSettingsTo>(settings);
                    if (chatbotSettingsTo != null)
                    {
                        // Partial save - only update checkbox properties, preserve existing ChatbotSource
                        Dev2Logger.Info($"SaveChatbotSettings: Received ChatbotSettingsTo - updating only checkbox properties", GlobalConstants.WarewolfInfo);
                        Config.Chatbot.IncludeSystemLog = chatbotSettingsTo.IncludeSystemLog;
                        Config.Chatbot.IncludeResourcesXaml = chatbotSettingsTo.IncludeResourcesXaml;
                        Config.Chatbot.IncludeResourcesJson = chatbotSettingsTo.IncludeResourcesJson;
                        Config.Chatbot.NumberOfLogLines = chatbotSettingsTo.NumberOfLogLines;
                        Dev2Logger.Info($"SaveChatbotSettings: Set values to: IncludeSystemLog={chatbotSettingsTo.IncludeSystemLog}, IncludeResourcesXaml={chatbotSettingsTo.IncludeResourcesXaml}, IncludeResourcesJson={chatbotSettingsTo.IncludeResourcesJson}, NumberOfLogLines={chatbotSettingsTo.NumberOfLogLines}", GlobalConstants.WarewolfInfo);
                    }
                }

                msg.Message = new StringBuilder();
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

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ChatbotSettings ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public string HandlesType() => nameof(Warewolf.Service.SaveChatbotSettings);
    }
}
