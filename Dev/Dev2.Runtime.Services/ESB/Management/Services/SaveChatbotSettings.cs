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
                    Config.Chatbot.LoadResourcesAsXaml = chatbotSettingsData.LoadResourcesAsXaml;
                    Config.Chatbot.NumberOfLogLines = chatbotSettingsData.NumberOfLogLines;
                    Config.Chatbot.SelectedResourceIds = chatbotSettingsData.SelectedResourceIds;
                    Config.Chatbot.UserMessageColor = chatbotSettingsData.UserMessageColor;
                    Config.Chatbot.UserMessageTextColor = chatbotSettingsData.UserMessageTextColor;
                    Config.Chatbot.BotMessageColor = chatbotSettingsData.BotMessageColor;
                    Config.Chatbot.BotMessageTextColor = chatbotSettingsData.BotMessageTextColor;
                    Config.Chatbot.SlidingWindowSummaryLength = chatbotSettingsData.SlidingWindowSummaryLength;
                    Config.Chatbot.EnableSlidingWindowTrimming = chatbotSettingsData.EnableSlidingWindowTrimming;
                }
                else
                {
                    // If that fails, try as ChatbotSettingsTo (from SettingsWriteService)
                    var chatbotSettingsTo = serializer.Deserialize<Dev2.Services.Chatbot.ChatbotSettingsTo>(settings);
                    if (chatbotSettingsTo != null)
                    {
                        // Partial save - only update non-source properties, preserve existing ChatbotSource
                        Dev2Logger.Info($"SaveChatbotSettings: Received ChatbotSettingsTo - updating only checkbox properties", GlobalConstants.WarewolfInfo);
                        Config.Chatbot.IncludeSystemLog = chatbotSettingsTo.IncludeSystemLog;
                        Config.Chatbot.LoadResourcesAsXaml = chatbotSettingsTo.LoadResourcesAsXaml;
                        Config.Chatbot.NumberOfLogLines = chatbotSettingsTo.NumberOfLogLines;
                        Config.Chatbot.SelectedResourceIds = chatbotSettingsTo.SelectedResourceIds;
                        Config.Chatbot.UserMessageColor = chatbotSettingsTo.UserMessageColor;
                        Config.Chatbot.UserMessageTextColor = chatbotSettingsTo.UserMessageTextColor;
                        Config.Chatbot.BotMessageColor = chatbotSettingsTo.BotMessageColor;
                        Config.Chatbot.BotMessageTextColor = chatbotSettingsTo.BotMessageTextColor;
                        Config.Chatbot.SlidingWindowSummaryLength = chatbotSettingsTo.SlidingWindowSummaryLength;
                        Config.Chatbot.EnableSlidingWindowTrimming = chatbotSettingsTo.EnableSlidingWindowTrimming;
                        Dev2Logger.Info($"SaveChatbotSettings: Set values to: IncludeSystemLog={chatbotSettingsTo.IncludeSystemLog}, LoadResourcesAsXaml={chatbotSettingsTo.LoadResourcesAsXaml}, NumberOfLogLines={chatbotSettingsTo.NumberOfLogLines}, EnableSlidingWindowTrimming={chatbotSettingsTo.EnableSlidingWindowTrimming}, SlidingWindowSummaryLength={chatbotSettingsTo.SlidingWindowSummaryLength}", GlobalConstants.WarewolfInfo);
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
