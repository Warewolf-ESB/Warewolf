/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SendChatbotMessage : IEsbManagementEndpoint
    {
        private const int TimeoutSeconds = 30;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Send Chatbot Message Service", GlobalConstants.WarewolfInfo);

                // Extract parameters
                values.TryGetValue(Warewolf.Service.SendChatbotMessage.Message, out StringBuilder messageBuilder);
                values.TryGetValue(Warewolf.Service.SendChatbotMessage.ConversationHistory, out StringBuilder conversationHistoryBuilder);

                // Validate message
                if (messageBuilder == null || string.IsNullOrWhiteSpace(messageBuilder.ToString()))
                {
                    return CreateErrorResponse(serializer, "Message parameter is required and cannot be empty");
                }

                var message = messageBuilder.ToString();

                // Parse conversation history (optional)
                List<ConversationMessage> conversationHistory = null;
                if (conversationHistoryBuilder != null && !string.IsNullOrWhiteSpace(conversationHistoryBuilder.ToString()))
                {
                    try
                    {
                        conversationHistory = JsonConvert.DeserializeObject<List<ConversationMessage>>(conversationHistoryBuilder.ToString());
                    }
                    catch (Exception ex)
                    {
                        Dev2Logger.Warn($"Failed to parse conversation history: {ex.Message}", GlobalConstants.WarewolfWarn);
                        conversationHistory = null;
                    }
                }

                // Get chatbot settings
                var settings = Config.Chatbot.Get();

                // Validate configuration
                if (settings.ChatbotSource == null || settings.ChatbotSource.Value == Guid.Empty)
                {
                    return CreateErrorResponse(serializer, "Chatbot is not configured. Please configure a chatbot source in settings.");
                }

                // Parse chatbot source definition from payload
                ChatbotSourceDefinition chatbotSourceDef = null;
                if (!string.IsNullOrWhiteSpace(settings.ChatbotSource.Payload))
                {
                    try
                    {
                        chatbotSourceDef = JsonConvert.DeserializeObject<ChatbotSourceDefinition>(settings.ChatbotSource.Payload);
                    }
                    catch (Exception ex)
                    {
                        Dev2Logger.Warn($"Failed to parse chatbot source definition from payload: {ex.Message}", GlobalConstants.WarewolfWarn);
                    }
                }

                // Fallback to getting from ResourceCatalog if payload parsing failed
                if (chatbotSourceDef == null)
                {
                    var chatbotSource = ResourceCatalog.Instance.GetResource<ChatbotSource>(GlobalConstants.ServerWorkspaceID, settings.ChatbotSource.Value);
                    if (chatbotSource == null)
                    {
                        return CreateErrorResponse(serializer, "Selected chatbot source not found or invalid.");
                    }

                    chatbotSourceDef = new ChatbotSourceDefinition
                    {
                        Id = chatbotSource.ResourceID,
                        Name = chatbotSource.ResourceName,
                        ApiKey = chatbotSource.ApiKey,
                        CompletionsEndpoint = chatbotSource.CompletionsEndpoint,
                        ModelsEndpoint = chatbotSource.ModelsEndpoint,
                        SelectedModel = chatbotSource.SelectedModel
                    };
                }

                // Validate endpoint and model
                if (string.IsNullOrWhiteSpace(chatbotSourceDef.CompletionsEndpoint))
                {
                    return CreateErrorResponse(serializer, "Chatbot source does not have a completions endpoint configured.");
                }

                if (string.IsNullOrWhiteSpace(chatbotSourceDef.SelectedModel))
                {
                    return CreateErrorResponse(serializer, "No AI model selected. Please select a model in chatbot settings.");
                }

                // Decrypt API key if encrypted
                if (!string.IsNullOrWhiteSpace(chatbotSourceDef.ApiKey))
                {
                    chatbotSourceDef.ApiKey = DpapiWrapper.DecryptIfEncrypted(chatbotSourceDef.ApiKey);
                }

                // Build the messages array: system prompt (with context) + conversation history + current message
                var contextBuilder = new ChatbotContextBuilder();
                var systemPrompt = contextBuilder.BuildSystemPrompt(settings);
                var messages = BuildMessagesArray(systemPrompt, message, conversationHistory);

                // Send to AI provider
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
                    var apiService = new ChatbotApiService(client);
                    var response = apiService.SendMessage(chatbotSourceDef, messages);

                    var result = new
                    {
                        Response = response,
                        Error = (string)null
                    };

                    return serializer.SerializeToBuilder(result);
                }
            }
            catch (HttpRequestException ex)
            {
                Dev2Logger.Error("SendChatbotMessage HTTP Error", ex, GlobalConstants.WarewolfError);

                var errorMessage = ex.Message;
                if (errorMessage.Contains("401") || errorMessage.ToLower().Contains("unauthorized"))
                {
                    errorMessage = "Authentication failed. Please verify your API key in the chatbot source configuration.";
                }
                else if (errorMessage.Contains("429") || errorMessage.ToLower().Contains("rate limit"))
                {
                    errorMessage = "Rate limit exceeded. Please try again later.";
                }
                else if (errorMessage.ToLower().Contains("timeout"))
                {
                    errorMessage = "Request timed out. The AI service took too long to respond.";
                }
                else
                {
                    errorMessage = $"Failed to connect to AI service: {errorMessage}";
                }

                return CreateErrorResponse(serializer, errorMessage);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("SendChatbotMessage Error", ex, GlobalConstants.WarewolfError);
                return CreateErrorResponse(serializer, $"An error occurred: {ex.Message}");
            }
        }

        private static List<object> BuildMessagesArray(string systemPrompt, string currentMessage, List<ConversationMessage> conversationHistory)
        {
            var messages = new List<object>();

            messages.Add(new { role = "system", content = systemPrompt });

            // Add conversation history if provided
            if (conversationHistory != null && conversationHistory.Any())
            {
                foreach (var historyMessage in conversationHistory)
                {
                    // Skip error messages
                    if (historyMessage.Type == "error")
                    {
                        continue;
                    }

                    var role = MapMessageTypeToRole(historyMessage.Type);

                    // Skip system messages from history — we build our own system prompt from settings
                    if (role == "system")
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(role))
                    {
                        messages.Add(new { role = role, content = historyMessage.Content });
                    }
                }
            }

            // Add current user message
            messages.Add(new { role = "user", content = currentMessage });

            return messages;
        }

        private static string MapMessageTypeToRole(string messageType)
        {
            switch (messageType?.ToLower())
            {
                case "user":
                    return "user";
                case "bot":
                case "assistant":
                    return "assistant";
                case "system":
                    return "system";
                default:
                    return null;
            }
        }

        private static StringBuilder CreateErrorResponse(Dev2JsonSerializer serializer, string errorMessage)
        {
            var result = new
            {
                Response = "",
                Error = errorMessage
            };

            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(
            HandlesType(),
            "<DataList><Message ColumnIODirection=\"Input\"/><ConversationHistory ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public string HandlesType() => nameof(SendChatbotMessage);

        // Helper class for deserializing conversation history
        private class ConversationMessage
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }

            [JsonProperty("timestamp")]
            public string Timestamp { get; set; }
        }
    }
}
