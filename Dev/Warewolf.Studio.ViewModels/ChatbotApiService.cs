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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

			// Detect if this is an Anthropic endpoint and use the correct auth from the start
			if (IsAnthropicEndpoint(source.CompletionsEndpoint))
			{
				Dev2.Common.Dev2Logger.Info("Detected Anthropic endpoint, using x-api-key authentication", "Warewolf Info");
				return await SendWithAuthAsync(chatMessages, source, "x-api-key", "", "anthropic-version=2023-06-01");
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
		
		// For Anthropic, extract system messages as a separate parameter
		string systemMessage = null;
		IEnumerable<ChatCompletionMessage> messagesToSend = chatMessages;
		
		if (IsAnthropicEndpoint(source.CompletionsEndpoint))
		{
			var systemMessages = chatMessages.Where(m => m.Role.Equals("system", StringComparison.OrdinalIgnoreCase)).ToList();
			if (systemMessages.Any())
			{
				// Combine all system messages into one
				systemMessage = string.Join("\n\n", systemMessages.Select(m => m.Content));
				// Remove system messages from the messages array
				messagesToSend = chatMessages.Where(m => !m.Role.Equals("system", StringComparison.OrdinalIgnoreCase));
			}
		}
		
		var messagesArray = messagesToSend.Select(m => new { role = m.Role, content = m.Content }).ToArray();

		var response = await SendWithParameterNegotiationAsync(source, modelToUse, messagesArray, systemMessage, IsAnthropicEndpoint(source.CompletionsEndpoint), authHeaderName, authHeaderPrefix, additionalHeaders);

			var responseContent = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				Dev2.Common.Dev2Logger.Error($"Chatbot API Error: {response.StatusCode} - {responseContent}", "Warewolf Error");
				
				// Check if this is an authentication error by status code
				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
				{
					throw new HttpRequestException($"Authentication failed: API returned {response.StatusCode}: {responseContent}");
				}
				
				throw new HttpRequestException($"API returned {response.StatusCode}: {responseContent}");
			}

            return ParseResponseContent(responseContent);
        }

	/// <summary>
	/// Sends the request, automatically retrying with different payload parameters when the API
	/// rejects max_completion_tokens or temperature as unsupported.
	/// </summary>
	private async Task<HttpResponseMessage> SendWithParameterNegotiationAsync(ChatbotSource source, string model, object[] messagesArray, string systemMessage, bool isAnthropicEndpoint, string authHeaderName, string authHeaderPrefix, string additionalHeaders)
	{
		// Anthropic requires max_tokens (not max_completion_tokens), so skip the negotiation for Anthropic
		var useMaxCompletionTokens = !isAnthropicEndpoint; // Use max_tokens for Anthropic, max_completion_tokens for others
		
		var payload = CreatePayload(model, messagesArray, systemMessage, useMaxCompletionTokens: useMaxCompletionTokens, includeTemperature: true);
		var json = JsonConvert.SerializeObject(payload);

		var request = CreateHttpRequestMessage(source, json, authHeaderName, authHeaderPrefix, additionalHeaders);
		var response = await _httpClient.SendAsync(request);

		if (response.IsSuccessStatusCode)
		{
			return response;
		}

		var errorContent = await response.Content.ReadAsStringAsync();

		// Only retry with max_tokens if we tried max_completion_tokens first (non-Anthropic endpoints)
		if (!isAnthropicEndpoint && errorContent.Contains("max_completion_tokens") && errorContent.Contains("not supported"))
		{
			Dev2.Common.Dev2Logger.Info("Retrying with max_tokens instead of max_completion_tokens", "Warewolf Info");

			payload = CreatePayload(model, messagesArray, systemMessage, useMaxCompletionTokens: false, includeTemperature: true);
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

			var useMaxCompletionTokensParam = !isAnthropicEndpoint && !errorContent.Contains("max_tokens");

			payload = CreatePayload(model, messagesArray, systemMessage, useMaxCompletionTokens: useMaxCompletionTokensParam, includeTemperature: false);
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

            // Safely truncate response for error message
            var truncatedResponse = TruncateStringSafely(responseContent, 200, "...");
            throw new HttpRequestException($"Unexpected API response format. Response: {truncatedResponse}");
        }

        /// <summary>
        /// Safely truncates a string to a maximum length, ensuring no multi-byte UTF-8 characters are split.
        /// </summary>
        private static string TruncateStringSafely(string text, int maxLength, string suffix)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }

            // Convert to UTF-8 bytes
            var encoding = System.Text.Encoding.UTF8;
            var bytes = encoding.GetBytes(text);

            // Calculate max bytes (reserve space for suffix)
            var suffixBytes = encoding.GetBytes(suffix);
            var maxBytes = maxLength * 3; // UTF-8 can use up to 3 bytes per character
            var targetBytes = maxBytes - suffixBytes.Length;

            if (targetBytes <= 0 || bytes.Length <= targetBytes)
            {
                return text;
            }

            // Truncate at byte boundary
            var truncatedBytes = new byte[targetBytes];
            System.Array.Copy(bytes, truncatedBytes, targetBytes);

            // Decode and remove any incomplete characters at the end
            var truncated = encoding.GetString(truncatedBytes);
            
            // Remove any replacement characters (?) that indicate incomplete multi-byte sequences
            truncated = truncated.TrimEnd('\uFFFD');

            return truncated + suffix;
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

	private static object CreatePayload(string model, object[] messages, string systemMessage, bool useMaxCompletionTokens, bool includeTemperature)
	{
		// Build payload dynamically to include system message only if provided
		var payload = new Dictionary<string, object>
		{
			{ "model", model },
			{ "messages", messages }
		};

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

		/// <summary>
		/// Detects if the endpoint is an Anthropic API endpoint.
		/// </summary>
		private static bool IsAnthropicEndpoint(string endpoint)
		{
			if (string.IsNullOrWhiteSpace(endpoint))
			{
				return false;
			}

			var lowerEndpoint = endpoint.ToLower();
			return lowerEndpoint.Contains("anthropic.com") || lowerEndpoint.Contains("claude");
		}

		/// <inheritdoc />
		public async Task<string> SendMessageStreamingAsync(IList<ChatCompletionMessage> chatMessages, ChatbotSource source, Action<string> onTokenReceived, CancellationToken cancellationToken = default)
		{
			if (chatMessages == null)
			{
				throw new ArgumentNullException(nameof(chatMessages));
			}
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (onTokenReceived == null)
			{
				throw new ArgumentNullException(nameof(onTokenReceived));
			}

			// Detect if this is an Anthropic endpoint and use the correct auth from the start
			if (IsAnthropicEndpoint(source.CompletionsEndpoint))
			{
				Dev2.Common.Dev2Logger.Info("Detected Anthropic endpoint for streaming, using x-api-key authentication", "Warewolf Info");
				return await SendStreamingWithAuthAsync(chatMessages, source, "x-api-key", "", "anthropic-version=2023-06-01", onTokenReceived, cancellationToken);
			}

			try
			{
				return await SendStreamingWithAuthAsync(chatMessages, source, "Authorization", "Bearer ", null, onTokenReceived, cancellationToken);
			}
			catch (HttpRequestException ex) when (IsAuthenticationError(ex))
			{
				Dev2.Common.Dev2Logger.Info("Bearer streaming auth failed, retrying with x-api-key", "Warewolf Info");

                try
                {
                    return await SendStreamingWithAuthAsync(chatMessages, source, "x-api-key", "", "anthropic-version=2023-06-01", onTokenReceived, cancellationToken);
                }
                catch (HttpRequestException)
                {
                    throw new HttpRequestException($"Streaming authentication failed with both Bearer and x-api-key methods. Original error: {ex.Message}", ex);
                }
            }
        }

	private async Task<string> SendStreamingWithAuthAsync(IList<ChatCompletionMessage> chatMessages, ChatbotSource source, string authHeaderName, string authHeaderPrefix, string additionalHeaders, Action<string> onTokenReceived, CancellationToken cancellationToken)
	{
	var modelToUse = !string.IsNullOrEmpty(source.SelectedModel) ? source.SelectedModel : "gpt-4o-mini";
		
	// For Anthropic, extract system messages as a separate parameter
	string systemMessage = null;
	IEnumerable<ChatCompletionMessage> messagesToSend = chatMessages;
		
	if (IsAnthropicEndpoint(source.CompletionsEndpoint))
	{
	var systemMessages = chatMessages.Where(m => m.Role.Equals("system", StringComparison.OrdinalIgnoreCase)).ToList();
	if (systemMessages.Any())
	{
	// Combine all system messages into one
	systemMessage = string.Join("\n\n", systemMessages.Select(m => m.Content));
	// Remove system messages from the messages array
	messagesToSend = chatMessages.Where(m => !m.Role.Equals("system", StringComparison.OrdinalIgnoreCase));
	}
	}
		
	var messagesArray = messagesToSend.Select(m => new { role = m.Role, content = m.Content }).ToArray();

	// Anthropic requires max_tokens (not max_completion_tokens)
	var isAnthropicEndpoint = IsAnthropicEndpoint(source.CompletionsEndpoint);
	var useMaxCompletionTokens = !isAnthropicEndpoint;
	var payload = CreateStreamingPayload(modelToUse, messagesArray, systemMessage, useMaxCompletionTokens: useMaxCompletionTokens);
		var json = JsonConvert.SerializeObject(payload);
		var request = CreateHttpRequestMessage(source, json, authHeaderName, authHeaderPrefix, additionalHeaders);
		var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

		// If max_completion_tokens not supported, retry with max_tokens
		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync();
			
			if (!isAnthropicEndpoint && errorContent.Contains("max_completion_tokens") && errorContent.Contains("not supported"))
			{
				Dev2.Common.Dev2Logger.Info("Streaming: Retrying with max_tokens instead of max_completion_tokens", "Warewolf Info");
				
				payload = CreateStreamingPayload(modelToUse, messagesArray, systemMessage, useMaxCompletionTokens: false);
				json = JsonConvert.SerializeObject(payload);
				request = CreateHttpRequestMessage(source, json, authHeaderName, authHeaderPrefix, additionalHeaders);
				response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			}
			
			if (!response.IsSuccessStatusCode)
			{
				errorContent = await response.Content.ReadAsStringAsync();
				Dev2.Common.Dev2Logger.Error($"Chatbot Streaming API Error: {response.StatusCode} - {errorContent}", "Warewolf Error");
				throw new HttpRequestException($"API returned {response.StatusCode}: {errorContent}");
			}
		}

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "";

            // If the server doesn't support streaming and returns a normal JSON response, parse it
            if (contentType.Contains("application/json"))
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var fullText = ParseResponseContent(responseContent);
                onTokenReceived(fullText);
                return fullText;
            }

            // Parse SSE stream
            return await ReadSseStreamAsync(response, onTokenReceived, cancellationToken);
        }

        private async Task<string> ReadSseStreamAsync(HttpResponseMessage response, Action<string> onTokenReceived, CancellationToken cancellationToken)
        {
            var fullResponse = new StringBuilder();

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // SSE format: lines starting with "data: "
                    if (!line.StartsWith("data: "))
                    {
                        continue;
                    }

                    var data = line.Substring(6); // Remove "data: " prefix

                    // Check for stream termination signals
                    if (data == "[DONE]" || data.Trim().Length == 0)
                    {
                        continue;
                    }

                    try
                    {
                        var token = ExtractTokenFromSseData(data);
                        if (token != null)
                        {
                            fullResponse.Append(token);
                            onTokenReceived(token);
                        }
                    }
                    catch (JsonException)
                    {
                        // Skip malformed SSE data chunks
                        Dev2.Common.Dev2Logger.Debug($"Skipping malformed SSE chunk: {TruncateStringSafely(data, 100, "...")}", "Warewolf Debug");
                    }
                }
            }

            return fullResponse.ToString();
        }

        private static string ExtractTokenFromSseData(string jsonData)
        {
            var obj = JObject.Parse(jsonData);

            // OpenAI format: choices[0].delta.content
            var content = obj.SelectToken("choices[0].delta.content")?.Value<string>();
            if (content != null)
            {
                return content;
            }

            // Anthropic format: type=content_block_delta, delta.text
            var type = obj.SelectToken("type")?.Value<string>();
            if (type == "content_block_delta")
            {
                return obj.SelectToken("delta.text")?.Value<string>();
            }

            return null;
        }

	private static object CreateStreamingPayload(string model, object[] messages, string systemMessage, bool useMaxCompletionTokens)
	{
		// Build payload dynamically to include system message only if provided
		var payload = new Dictionary<string, object>
		{
			{ "model", model },
			{ "messages", messages },
			{ "temperature", ChatTemperature },
			{ "stream", true }
		};

		if (!string.IsNullOrWhiteSpace(systemMessage))
		{
			payload["system"] = systemMessage;
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
	}
}
