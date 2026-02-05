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
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SendChatbotMessage : IEsbManagementEndpoint
    {
        private const int TimeoutSeconds = 30;
        private const int MaxMessageLength = 10000;

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
                if (message.Length > MaxMessageLength)
                {
                    return CreateErrorResponse(serializer, $"Message is too long. Maximum length is {MaxMessageLength} characters");
                }

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

                // Get chatbot source
                var chatbotSource = ResourceCatalog.Instance.GetResource<ChatbotSource>(GlobalConstants.ServerWorkspaceID, settings.ChatbotSource.Value);
                if (chatbotSource == null)
                {
                    return CreateErrorResponse(serializer, "Selected chatbot source not found or invalid.");
                }

                // Validate endpoint and model
                if (string.IsNullOrWhiteSpace(chatbotSource.CompletionsEndpoint))
                {
                    return CreateErrorResponse(serializer, "Chatbot source does not have a completions endpoint configured.");
                }

                if (string.IsNullOrWhiteSpace(chatbotSource.SelectedModel))
                {
                    return CreateErrorResponse(serializer, "No AI model selected. Please select a model in chatbot settings.");
                }

                // Send message to AI provider
                var response = SendMessageToProvider(chatbotSource, message, conversationHistory, settings);

                // Return success response
                var result = new
                {
                    Response = response,
                    Error = (string)null
                };

                return serializer.SerializeToBuilder(result);
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

        private static string SendMessageToProvider(ChatbotSource source, string message, List<ConversationMessage> conversationHistory, Warewolf.Configuration.ChatbotSettingsData settings)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

                // Build messages array
                var messages = BuildMessagesArray(message, conversationHistory, settings);

                // Build request body
                var requestBody = new
                {
                    model = source.SelectedModel,
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 2000
                };

                var requestJson = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                // Try Bearer authentication first (OpenAI, Azure OpenAI, GitHub Models, Grok)
                try
                {
                    return SendWithAuth(client, source.CompletionsEndpoint, content, "Authorization", $"Bearer {source.ApiKey}", null);
                }
                catch (HttpRequestException ex) when (IsAuthenticationError(ex))
                {
                    Dev2Logger.Info("Bearer authentication failed, retrying with x-api-key", GlobalConstants.WarewolfInfo);
                    
                    // Retry with Anthropic-style authentication
                    return SendWithAuth(client, source.CompletionsEndpoint, content, "x-api-key", source.ApiKey, "anthropic-version=2023-06-01");
                }
            }
        }

        private static string SendWithAuth(HttpClient client, string endpoint, HttpContent content, string authHeaderName, string authHeaderValue, string additionalHeaders)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add(authHeaderName, authHeaderValue);
#pragma warning disable CC0021 // Use nameof
            client.DefaultRequestHeaders.Add("User-Agent", "Warewolf");
#pragma warning restore CC0021 // Use nameof

            // Add any additional headers if specified
            if (!string.IsNullOrWhiteSpace(additionalHeaders))
            {
                var headerPairs = additionalHeaders.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var headerPair in headerPairs)
                {
                    var parts = headerPair.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        var headerName = parts[0].Trim();
                        var headerValue = parts[1].Trim();
                        if (!string.IsNullOrWhiteSpace(headerName) && !string.IsNullOrWhiteSpace(headerValue))
                        {
                            client.DefaultRequestHeaders.Add(headerName, headerValue);
                        }
                    }
                }
            }

            var response = client.PostAsync(endpoint, content).Result;

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                throw new HttpRequestException($"AI service error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = response.Content.ReadAsStringAsync().Result;
            return ParseResponse(responseContent);
        }

        private static List<object> BuildMessagesArray(string currentMessage, List<ConversationMessage> conversationHistory, Warewolf.Configuration.ChatbotSettingsData settings)
        {
            var messages = new List<object>();

            // Add system message
            var systemMessage = "You are a helpful AI assistant for a workflow automation platform.";
            
            // Add additional context if enabled
            var contextParts = new List<string>();
            
            if (settings.IncludeSystemLog)
            {
                // TODO: In a future enhancement, retrieve and add recent system log entries
                // For now, we'll skip this to keep the initial implementation simple
            }

            if (settings.SelectedResourceIds != null && settings.SelectedResourceIds.Count > 0)
            {
                // TODO: In a future enhancement, retrieve and add relevant resource definitions
                // For now, we'll skip this to keep the initial implementation simple
            }

            if (contextParts.Any())
            {
                systemMessage += "\n\n" + string.Join("\n\n", contextParts);
            }

            messages.Add(new { role = "system", content = systemMessage });

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

                    // Map message type to API role
                    var role = MapMessageTypeToRole(historyMessage.Type);
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

        private static string ParseResponse(string responseJson)
        {
            try
            {
                var responseObject = JObject.Parse(responseJson);

                // OpenAI-compatible format
                var choices = responseObject["choices"] as JArray;
                if (choices != null && choices.Count > 0)
                {
                    var firstChoice = choices[0] as JObject;
                    var message = firstChoice?["message"] as JObject;
                    var content = message?["content"]?.ToString();

                    if (!string.IsNullOrEmpty(content))
                    {
                        return content;
                    }
                }

                // Anthropic format (if different)
                var content2 = responseObject["content"]?[0]?["text"]?.ToString();
                if (!string.IsNullOrEmpty(content2))
                {
                    return content2;
                }

                throw new Exception("Could not parse response content from AI service");
            }
            catch (JsonException ex)
            {
                Dev2Logger.Error("Failed to parse AI response", ex, GlobalConstants.WarewolfError);
                throw new Exception($"Failed to parse AI service response: {ex.Message}");
            }
        }

        private static bool IsAuthenticationError(HttpRequestException ex)
        {
            if (ex.Message == null)
            {
                return false;
            }

            var message = ex.Message.ToLower();
            return message.Contains("401") || message.Contains("unauthorized") ||
                   message.Contains("403") || message.Contains("forbidden") ||
                   message.Contains("authentication") || (message.Contains("invalid") && (message.Contains("key") || message.Contains("token")));
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
