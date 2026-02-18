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
using System.Net.Http;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Handles HTTP communication with external AI provider APIs.
    /// Supports OpenAI-compatible endpoints, Anthropic (Claude), and Google Gemini.
    /// Manages authentication negotiation and provider-specific payload formats.
    /// </summary>
    internal class ChatbotApiService
    {
        private const int MaxCompletionTokens = 2000;
        private const double ChatTemperature = 0.7;

        private readonly HttpClient _client;

        public ChatbotApiService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Sends a list of messages to the configured AI provider and returns the response text.
        /// </summary>
        public string SendMessage(ChatbotSourceDefinition source, List<object> messages)
        {
            var endpoint = source.CompletionsEndpoint;

            if (IsAnthropicEndpoint(endpoint))
            {
                return SendToAnthropic(source, messages);
            }

            if (IsGoogleGeminiEndpoint(endpoint))
            {
                return SendToGemini(source, messages);
            }

            return SendToOpenAI(source, messages);
        }

        private string SendToOpenAI(ChatbotSourceDefinition source, List<object> messages)
        {
            // Try max_completion_tokens first (newer OpenAI parameter)
            var payload = CreatePayload(source.SelectedModel, messages, null, useMaxCompletionTokens: true, includeTemperature: true);
            var response = PostWithAuth(source.CompletionsEndpoint, payload, "Authorization", $"Bearer {source.ApiKey}", null);

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
                response = PostWithAuth(source.CompletionsEndpoint, payload, "Authorization", $"Bearer {source.ApiKey}", null);

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
                response = PostWithAuth(source.CompletionsEndpoint, payload, "Authorization", $"Bearer {source.ApiKey}", null);

                if (response.IsSuccessStatusCode)
                {
                    return ReadAndParseResponse(response);
                }

                errorContent = response.Content.ReadAsStringAsync().Result;
            }

            throw new HttpRequestException($"AI service error: {response.StatusCode} - {errorContent}");
        }

        private string SendToAnthropic(ChatbotSourceDefinition source, List<object> messages)
        {
            // Anthropic requires system messages as a top-level "system" field, not in the messages array
            string systemMessage = null;
            var nonSystemMessages = new List<object>();

            foreach (var msg in messages)
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
            return SendWithAuth(source.CompletionsEndpoint, payload, "x-api-key", source.ApiKey, "anthropic-version=2023-06-01");
        }

        private string SendToGemini(ChatbotSourceDefinition source, List<object> messages)
        {
            // Gemini uses a different payload format with "contents" and "parts"
            var contents = new List<object>();

            foreach (var msg in messages)
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

            var requestBody = new { contents = contents };

            // Gemini uses API key as query parameter and model in URL path
            const string baseUrl = "https://generativelanguage.googleapis.com/v1beta";
            var modelName = source.SelectedModel;
            var endpoint = $"{baseUrl}/{modelName}:generateContent?key={source.ApiKey}";

            return SendWithAuth(endpoint, requestBody, null, null, null);
        }

        /// <summary>
        /// Sends payload and throws on failure. Used by providers that don't need parameter negotiation.
        /// </summary>
        private string SendWithAuth(string endpoint, object payload, string authHeaderName, string authHeaderValue, string additionalHeaders)
        {
            var response = PostWithAuth(endpoint, payload, authHeaderName, authHeaderValue, additionalHeaders);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                throw new HttpRequestException($"AI service error: {response.StatusCode} - {errorContent}");
            }

            return ReadAndParseResponse(response);
        }

        /// <summary>
        /// Posts a JSON payload and returns the raw HttpResponseMessage.
        /// Used by providers that need parameter negotiation (retry on unsupported parameters).
        /// </summary>
        private HttpResponseMessage PostWithAuth(string endpoint, object payload, string authHeaderName, string authHeaderValue, string additionalHeaders)
        {
            _client.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrWhiteSpace(authHeaderName))
            {
                _client.DefaultRequestHeaders.Add(authHeaderName, authHeaderValue);
            }
#pragma warning disable CC0021 // Use nameof
            _client.DefaultRequestHeaders.Add("User-Agent", "Warewolf");
#pragma warning restore CC0021 // Use nameof

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
                            _client.DefaultRequestHeaders.Add(headerName, headerValue);
                        }
                    }
                }
            }

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return _client.PostAsync(endpoint, content).Result;
        }

        private static string ReadAndParseResponse(HttpResponseMessage response)
        {
            var responseContent = response.Content.ReadAsStringAsync().Result;
            return ParseResponse(responseContent);
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

        internal static string ParseResponse(string responseJson)
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

        internal static bool IsAnthropicEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return false;
            }
            var lower = endpoint.ToLower();
            return lower.Contains("anthropic.com") || lower.Contains("claude");
        }

        internal static bool IsGoogleGeminiEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return false;
            }
            var lower = endpoint.ToLower();
            return lower.Contains("generativelanguage.googleapis.com") || lower.Contains("gemini");
        }
    }
}
