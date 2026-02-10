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
using System.IO;
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
using Newtonsoft.Json.Linq;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SendChatbotMessage : IEsbManagementEndpoint
    {
        private const int TimeoutSeconds = 30;
        private const int MaxCompletionTokens = 2000;
        private const double ChatTemperature = 0.7;

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
                    
                    // Convert ChatbotSource to ChatbotSourceDefinition
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

                // Send message to AI provider
                var response = SendMessageToProvider(chatbotSourceDef, message, conversationHistory, settings);

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

        private static string SendMessageToProvider(ChatbotSourceDefinition source, string message, List<ConversationMessage> conversationHistory, Warewolf.Configuration.ChatbotSettingsData settings)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);

                var endpoint = source.CompletionsEndpoint;

                if (IsAnthropicEndpoint(endpoint))
                {
                    return SendToAnthropic(client, source, message, conversationHistory, settings);
                }

                if (IsGoogleGeminiEndpoint(endpoint))
                {
                    return SendToGemini(client, source, message, conversationHistory, settings);
                }

                // Default: OpenAI-compatible (OpenAI, XAI, GitHub Models, Azure OpenAI, etc.)
                return SendToOpenAI(client, source, message, conversationHistory, settings);
            }
        }

        private static string SendToOpenAI(HttpClient client, ChatbotSourceDefinition source, string message, List<ConversationMessage> conversationHistory, Warewolf.Configuration.ChatbotSettingsData settings)
        {
            var messages = BuildMessagesArray(message, conversationHistory, settings);

            // Try max_completion_tokens first (newer OpenAI parameter)
            var payload = CreatePayload(source.SelectedModel, messages, null, useMaxCompletionTokens: true, includeTemperature: true);
            var response = PostWithAuth(client, source.CompletionsEndpoint, payload, "Authorization", $"Bearer {source.ApiKey}", null);

            if (response.IsSuccessStatusCode)
            {
                return ReadAndParseResponse(response);
            }

            var errorContent = response.Content.ReadAsStringAsync().Result;

            // Retry with max_tokens if max_completion_tokens is not supported
            if (errorContent.Contains("max_completion_tokens") && errorContent.Contains("not supported"))
            {
                Dev2Logger.Info("Retrying with max_tokens instead of max_completion_tokens", GlobalConstants.WarewolfInfo);

                payload = CreatePayload(source.SelectedModel, messages, null, useMaxCompletionTokens: false, includeTemperature: true);
                response = PostWithAuth(client, source.CompletionsEndpoint, payload, "Authorization", $"Bearer {source.ApiKey}", null);

                if (response.IsSuccessStatusCode)
                {
                    return ReadAndParseResponse(response);
                }

                errorContent = response.Content.ReadAsStringAsync().Result;
            }

            // Retry without temperature if not supported
            if (errorContent.Contains("temperature") && (errorContent.Contains("not support") || errorContent.Contains("does not support") || errorContent.Contains("unsupported")))
            {
                Dev2Logger.Info("Retrying without temperature parameter", GlobalConstants.WarewolfInfo);

                var useMaxCompletionTokensRetry = !errorContent.Contains("max_tokens");
                payload = CreatePayload(source.SelectedModel, messages, null, useMaxCompletionTokens: useMaxCompletionTokensRetry, includeTemperature: false);
                response = PostWithAuth(client, source.CompletionsEndpoint, payload, "Authorization", $"Bearer {source.ApiKey}", null);

                if (response.IsSuccessStatusCode)
                {
                    return ReadAndParseResponse(response);
                }

                errorContent = response.Content.ReadAsStringAsync().Result;
            }

            throw new HttpRequestException($"AI service error: {response.StatusCode} - {errorContent}");
        }

        private static string SendToAnthropic(HttpClient client, ChatbotSourceDefinition source, string message, List<ConversationMessage> conversationHistory, Warewolf.Configuration.ChatbotSettingsData settings)
        {
            // Anthropic requires system messages as a top-level "system" field, not in the messages array
            var allMessages = BuildMessagesArray(message, conversationHistory, settings);
            string systemMessage = null;
            var nonSystemMessages = new List<object>();

            foreach (var msg in allMessages)
            {
                var json = JObject.FromObject(msg);
                if (json["role"]?.ToString() == "system")
                {
                    systemMessage = (systemMessage == null ? "" : systemMessage + "\n\n") + json["content"]?.ToString();
                }
                else
                {
                    nonSystemMessages.Add(msg);
                }
            }

            // Anthropic always uses max_tokens (not max_completion_tokens)
            var payload = CreatePayload(source.SelectedModel, nonSystemMessages, systemMessage, useMaxCompletionTokens: false, includeTemperature: true);
            return SendWithAuth(client, source.CompletionsEndpoint, payload, "x-api-key", source.ApiKey, "anthropic-version=2023-06-01");
        }

        private static string SendToGemini(HttpClient client, ChatbotSourceDefinition source, string message, List<ConversationMessage> conversationHistory, Warewolf.Configuration.ChatbotSettingsData settings)
        {
            // Gemini uses a different payload format with "contents" and "parts"
            var allMessages = BuildMessagesArray(message, conversationHistory, settings);
            var contents = new List<object>();

            foreach (var msg in allMessages)
            {
                var json = JObject.FromObject(msg);
                var role = json["role"]?.ToString();

                // Skip system messages — Gemini handles them differently
                if (role == "system")
                {
                    continue;
                }

                // Gemini uses "model" instead of "assistant"
                var geminiRole = role == "assistant" ? "model" : role;

                contents.Add(new
                {
                    role = geminiRole,
                    parts = new[] { new { text = json["content"]?.ToString() } }
                });
            }

            // Gemini payload is just the contents array — no model/max_tokens/temperature in body
            var requestBody = new { contents = contents };

			// Gemini uses API key as query parameter and model in URL path
			const string baseUrl = "https://generativelanguage.googleapis.com/v1beta";
			var modelName = source.SelectedModel;
            var endpoint = $"{baseUrl}/{modelName}:generateContent?key={source.ApiKey}";

            return SendWithAuth(client, endpoint, requestBody, null, null, null);
        }

        private static Dictionary<string, object> CreatePayload(string model, object messages, string systemMessage, bool useMaxCompletionTokens, bool includeTemperature)
        {
#pragma warning disable CC0021 // Use nameof
			var payload = new Dictionary<string, object>
            {
                { "model", model },
                { "messages", messages }
            };
#pragma warning restore CC0021 // Use nameof

			if (!string.IsNullOrWhiteSpace(systemMessage))
            {
                payload["system"] = systemMessage;
            }

            if (includeTemperature)
            {
                payload["temperature"] = ChatTemperature;
            }

            if (useMaxCompletionTokens)
            {
                payload["max_completion_tokens"] = MaxCompletionTokens;
            }
            else
            {
                payload["max_tokens"] = MaxCompletionTokens;
            }

            return payload;
        }

        /// <summary>
        /// Sends payload and throws on failure. Used by providers that don't need parameter negotiation.
        /// </summary>
        private static string SendWithAuth(HttpClient client, string endpoint, object payload, string authHeaderName, string authHeaderValue, string additionalHeaders)
        {
            var response = PostWithAuth(client, endpoint, payload, authHeaderName, authHeaderValue, additionalHeaders);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                throw new HttpRequestException($"AI service error: {response.StatusCode} - {errorContent}");
            }

            return ReadAndParseResponse(response);
        }

        /// <summary>
        /// Posts a JSON payload and returns the raw HttpResponseMessage for inspection.
        /// Used by providers that need parameter negotiation (retry on unsupported parameters).
        /// </summary>
        private static HttpResponseMessage PostWithAuth(HttpClient client, string endpoint, object payload, string authHeaderName, string authHeaderValue, string additionalHeaders)
        {
            client.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrWhiteSpace(authHeaderName))
            {
                client.DefaultRequestHeaders.Add(authHeaderName, authHeaderValue);
            }
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

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return client.PostAsync(endpoint, content).Result;
        }

        private static string ReadAndParseResponse(HttpResponseMessage response)
        {
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
                try
                {
                    var logEntries = ReadRecentLogEntries(settings.NumberOfLogLines);
                    if (logEntries.Any())
                    {
                        var logContext = "Recent System Log Entries:\n" + string.Join("\n", logEntries);
                        contextParts.Add(logContext);
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Warn($"Failed to retrieve system log entries: {ex.Message}", GlobalConstants.WarewolfWarn);
                }
            }

            if (settings.SelectedResourceIds != null && settings.SelectedResourceIds.Count > 0)
            {
                try
                {
                    var resourceDefinitions = GetResourceDefinitions(settings.SelectedResourceIds, settings.LoadResourcesAsXaml);
                    if (resourceDefinitions.Any())
                    {
                        var resourceContext = "Available Workflow Definitions:\n" + string.Join("\n\n", resourceDefinitions);
                        contextParts.Add(resourceContext);
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Warn($"Failed to retrieve resource definitions: {ex.Message}", GlobalConstants.WarewolfWarn);
                }
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
                    
                    // Skip system messages from history - we build our own system prompt from settings
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

        private static string ParseResponse(string responseJson)
        {
            try
            {
                var responseObject = JObject.Parse(responseJson);

                // OpenAI-compatible format: choices[0].message.content
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

                // Anthropic format: content[0].text
                var anthropicContent = responseObject["content"]?[0]?["text"]?.ToString();
                if (!string.IsNullOrEmpty(anthropicContent))
                {
                    return anthropicContent;
                }

                // Google Gemini format: candidates[0].content.parts[0].text
                var geminiContent = responseObject.SelectToken("candidates[0].content.parts[0].text")?.ToString();
                if (!string.IsNullOrEmpty(geminiContent))
                {
                    return geminiContent;
                }

                throw new Exception("Could not parse response content from AI service");
            }
            catch (JsonException ex)
            {
                Dev2Logger.Error("Failed to parse AI response", ex, GlobalConstants.WarewolfError);
                throw new Exception($"Failed to parse AI service response: {ex.Message}");
            }
        }

        private static bool IsAnthropicEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return false;
            }
            var lower = endpoint.ToLower();
            return lower.Contains("anthropic.com") || lower.Contains("claude");
        }

        private static bool IsGoogleGeminiEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return false;
            }
            var lower = endpoint.ToLower();
            return lower.Contains("generativelanguage.googleapis.com") || lower.Contains("gemini");
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

        private static List<string> ReadRecentLogEntries(int numberOfLines)
        {
            var logEntries = new List<string>();
            var serverLogPath = EnvironmentVariables.ServerLogFile;
            
            if (string.IsNullOrWhiteSpace(serverLogPath) || !File.Exists(serverLogPath))
            {
                return logEntries;
            }

            try
            {
                using (var fileStream = new FileStream(serverLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    var allLines = new List<string>();
                    while (!streamReader.EndOfStream)
                    {
                        allLines.Add(streamReader.ReadLine());
                    }

                    if (allLines.Count > numberOfLines)
                    {
                        logEntries = allLines.Skip(allLines.Count - numberOfLines).ToList();
                    }
                    else
                    {
                        logEntries = allLines;
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Warn($"Failed to read log file: {ex.Message}", GlobalConstants.WarewolfWarn);
            }

            return logEntries;
        }

        private static List<string> GetResourceDefinitions(List<Guid> resourceIds, bool loadAsXaml)
        {
            var definitions = new List<string>();

            foreach (var resourceId in resourceIds)
            {
                try
                {
                    var resource = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
                    if (resource != null)
                    {
                        if (loadAsXaml)
                        {
                            var resourceXml = ResourceCatalog.Instance.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
                            if (resourceXml != null && resourceXml.Length > 0)
                            {
                                definitions.Add($"Resource: {resource.ResourceName} (ID: {resourceId})\n{resourceXml}");
                            }
                        }
                        else
                        {
                            var resourceInfo = $"Resource: {resource.ResourceName}\nType: {resource.ResourceType}\nPath: {resource.GetResourcePath(GlobalConstants.ServerWorkspaceID)}";
                            definitions.Add(resourceInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Warn($"Failed to load resource {resourceId}: {ex.Message}", GlobalConstants.WarewolfWarn);
                }
            }

            return definitions;
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
