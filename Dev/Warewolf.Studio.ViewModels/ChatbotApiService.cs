#pragma warning disable
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
using System.Threading.Tasks;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
using Newtonsoft.Json;

namespace Warewolf.Studio.ViewModels
{
    /// <summary>
    /// Handles HTTP communication with external chatbot APIs (OpenAI-compatible and Anthropic).
    /// Manages authentication negotiation, payload parameter compatibility, and response parsing.
    /// </summary>
    public class ChatbotApiService : IChatbotApiService
    {
        /// <summary>Maximum number of tokens the chatbot API should generate in a single response.</summary>
        private const int MaxCompletionTokens = 2_000;

        /// <summary>Temperature parameter for chatbot API responses controlling randomness (0.0 = deterministic, 1.0 = creative).</summary>
        private const double ChatTemperature = 0.7;

        private static readonly HttpClient _httpClient = new HttpClient();

        // DTOs for strongly-typed API response deserialization
        private class OpenAiChatResponse
        {
            [JsonProperty("choices")]
            public ChatChoice[] Choices { get; set; }
        }

        private class ChatChoice
        {
            [JsonProperty("message")]
            public ChatResponseMessage Message { get; set; }
        }

        private class ChatResponseMessage
        {
            [JsonProperty("content")]
            public string Content { get; set; }
        }

        private class AnthropicChatResponse
        {
            [JsonProperty("content")]
            public ContentBlock[] Content { get; set; }
        }

        private class ContentBlock
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }
        }

        /// <inheritdoc />
        public async Task<string> SendMessageAsync(IList<ChatCompletionMessage> chatMessages, ChatbotSource source)
        {
            if (chatMessages == null)
            {
                throw new ArgumentNullException(nameof(chatMessages));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // Try with default Bearer authentication first
            try
            {
                return await SendWithAuthAsync(chatMessages, source, "Authorization", "Bearer ", null);
            }
            catch (HttpRequestException ex) when (IsAuthenticationError(ex))
            {
                Dev2.Common.Dev2Logger.Info("Bearer authentication failed, retrying with x-api-key authentication", "Warewolf Info");

                // Retry with Claude-style authentication (x-api-key header + anthropic-version)
                try
                {
                    return await SendWithAuthAsync(chatMessages, source, "x-api-key", "", "anthropic-version=2023-06-01");
                }
                catch (HttpRequestException)
                {
                    // If both fail, throw the original error
                    throw new HttpRequestException($"Authentication failed with both Bearer and x-api-key methods. Original error: {ex.Message}", ex);
                }
            }
        }

        private async Task<string> SendWithAuthAsync(IList<ChatCompletionMessage> chatMessages, ChatbotSource source, string authHeaderName, string authHeaderPrefix, string additionalHeaders)
        {
            var modelToUse = !string.IsNullOrEmpty(source.SelectedModel) ? source.SelectedModel : "gpt-4o-mini";
            var messagesArray = chatMessages.Select(m => new { role = m.Role, content = m.Content }).ToArray();

            var response = await SendWithParameterNegotiationAsync(source, modelToUse, messagesArray, authHeaderName, authHeaderPrefix, additionalHeaders);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Dev2.Common.Dev2Logger.Error($"Chatbot API Error: {response.StatusCode} - {responseContent}", "Warewolf Error");
                throw new HttpRequestException($"API returned {response.StatusCode}: {responseContent}");
            }

            return ParseResponseContent(responseContent);
        }

        /// <summary>
        /// Sends the request, automatically retrying with different payload parameters when the API
        /// rejects max_completion_tokens or temperature as unsupported.
        /// </summary>
        private async Task<HttpResponseMessage> SendWithParameterNegotiationAsync(ChatbotSource source, string model, object[] messagesArray, string authHeaderName, string authHeaderPrefix, string additionalHeaders)
        {
            // Try with max_completion_tokens first (newer API standard)
            var payload = CreatePayload(model, messagesArray, useMaxCompletionTokens: true, includeTemperature: true);
            var json = JsonConvert.SerializeObject(payload);

            var request = CreateHttpRequestMessage(source, json, authHeaderName, authHeaderPrefix, additionalHeaders);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var errorContent = await response.Content.ReadAsStringAsync();

            // Retry with max_tokens if max_completion_tokens is not supported
            if (errorContent.Contains("max_completion_tokens") && errorContent.Contains("not supported"))
            {
                Dev2.Common.Dev2Logger.Info("Retrying with max_tokens instead of max_completion_tokens", "Warewolf Info");

                payload = CreatePayload(model, messagesArray, useMaxCompletionTokens: false, includeTemperature: true);
                json = JsonConvert.SerializeObject(payload);
                request = CreateHttpRequestMessage(source, json, authHeaderName, authHeaderPrefix, additionalHeaders);
                response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                errorContent = await response.Content.ReadAsStringAsync();
            }

            // Retry without temperature if not supported
            if (errorContent.Contains("temperature") && errorContent.Contains("not support"))
            {
                Dev2.Common.Dev2Logger.Info("Retrying without temperature parameter", "Warewolf Info");

                var useMaxCompletionTokensParam = !errorContent.Contains("max_tokens");

                payload = CreatePayload(model, messagesArray, useMaxCompletionTokens: useMaxCompletionTokensParam, includeTemperature: false);
                json = JsonConvert.SerializeObject(payload);
                request = CreateHttpRequestMessage(source, json, authHeaderName, authHeaderPrefix, additionalHeaders);
                response = await _httpClient.SendAsync(request);
            }

            return response;
        }

        private static string ParseResponseContent(string responseContent)
        {
            // Try OpenAI-compatible response format first
            try
            {
                var openAiResponse = JsonConvert.DeserializeObject<OpenAiChatResponse>(responseContent);
                if (openAiResponse?.Choices != null && openAiResponse.Choices.Length > 0)
                {
                    var botResponse = openAiResponse.Choices[0]?.Message?.Content;
                    if (!string.IsNullOrEmpty(botResponse))
                    {
                        return botResponse;
                    }
                }
            }
            catch (JsonException)
            {
                // Not OpenAI format, try Anthropic format
            }

            // Try Anthropic response format: { "content": [{ "type": "text", "text": "..." }] }
            try
            {
                var anthropicResponse = JsonConvert.DeserializeObject<AnthropicChatResponse>(responseContent);
                if (anthropicResponse?.Content != null && anthropicResponse.Content.Length > 0)
                {
                    var botResponse = anthropicResponse.Content[0]?.Text;
                    if (!string.IsNullOrEmpty(botResponse))
                    {
                        return botResponse;
                    }
                }
            }
            catch (JsonException)
            {
                // Not Anthropic format either
            }

            throw new HttpRequestException($"Unexpected API response format. Response: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
        }

        private static HttpRequestMessage CreateHttpRequestMessage(ChatbotSource source, string jsonBody, string authHeaderName, string authHeaderPrefix, string additionalHeaders)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, source.CompletionsEndpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            // Set authentication header only if API key is provided
            if (!string.IsNullOrWhiteSpace(source.ApiKey))
            {
                request.Headers.Add(authHeaderName, authHeaderPrefix + source.ApiKey);
            }

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
                            request.Headers.Add(headerName, headerValue);
                        }
                    }
                }
            }

            return request;
        }

        private static object CreatePayload(string model, object[] messages, bool useMaxCompletionTokens, bool includeTemperature)
        {
            if (useMaxCompletionTokens)
            {
                if (includeTemperature)
                {
                    return new
                    {
                        model = model,
                        messages = messages,
                        temperature = ChatTemperature,
                        max_completion_tokens = MaxCompletionTokens
                    };
                }

                return new
                {
                    model = model,
                    messages = messages,
                    max_completion_tokens = MaxCompletionTokens
                };
            }

            if (includeTemperature)
            {
                return new
                {
                    model = model,
                    messages = messages,
                    temperature = ChatTemperature,
                    max_tokens = MaxCompletionTokens
                };
            }

            return new
            {
                model = model,
                messages = messages,
                max_tokens = MaxCompletionTokens
            };
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
                   message.Contains("authentication") || message.Contains("invalid") && (message.Contains("key") || message.Contains("token"));
        }
    }
}
