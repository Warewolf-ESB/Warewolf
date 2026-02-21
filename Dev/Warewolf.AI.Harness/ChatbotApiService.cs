using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Text;
using Newtonsoft.Json;



namespace Warewolf.AI.Harness
{
    /// <summary>
    /// Handles HTTP communication with external AI provider APIs.
    /// Supports OpenAI-compatible endpoints, Anthropic (Claude), and Google Gemini.
    /// Manages authentication negotiation and provider-specific payload formats.
    /// </summary>
    public class ChatbotApiService
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
        /// Provider is determined first by the explicit Provider field, then by URL heuristics for backward compatibility.
        /// </summary>
        public string SendMessage(ChatbotSourceDefinition source, List<object> messages)
        {
            var provider = ResolveProvider(source);
            var impl = provider switch
            {
                AnthropicProvider.ProviderName => (IChatbotProvider)new AnthropicProvider(this),
                GeminiProvider.ProviderName => (IChatbotProvider)new GeminiProvider(this),
                OpenRouterProvider.ProviderName => (IChatbotProvider)new OpenRouterProvider(this),
                _ => (IChatbotProvider)new OpenAIProvider(this)
            };

            return impl.Send(source, messages);
        }

        /// <summary>
        /// Resolves the provider from the explicit Provider field or falls back to URL heuristics.
        /// </summary>
        internal static string ResolveProvider(ChatbotSourceDefinition source)
        {
            if (!string.IsNullOrWhiteSpace(source.Provider))
            {
                return source.Provider;
            }

            // Fallback: detect from endpoint URL for backward compatibility
            var endpoint = source.CompletionsEndpoint ?? string.Empty;
            if (IsAnthropicEndpoint(endpoint))
            {
                return AnthropicProvider.ProviderName;
            }
            if (IsGoogleGeminiEndpoint(endpoint))
            {
                return GeminiProvider.ProviderName;
            }
            if (IsOpenRouterEndpoint(endpoint))
            {
                return OpenRouterProvider.ProviderName;
            }
            return OpenAIProvider.ProviderName;
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
        /// Sends payload and throws on failure. Used by providers that don't need parameter negotiation.
        /// </summary>
        private string SendWithAuthAndTelemetry(string endpoint, object payload, string authHeaderName, string authHeaderValue, string additionalHeaders)
        {
            var response = PostWithAuthAndTelemetry(endpoint, payload, authHeaderName, authHeaderValue, additionalHeaders);

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


        private HttpResponseMessage PostWithAuthAndTelemetry(
    string endpoint,
    object payload,
    string authHeaderName,
    string authHeaderValue,
    string additionalHeaders)
        {
            _client.DefaultRequestHeaders.Clear();

            if (!string.IsNullOrWhiteSpace(authHeaderName))
                _client.DefaultRequestHeaders.Add(authHeaderName, authHeaderValue);

            _client.DefaultRequestHeaders.Add("User-Agent", "Warewolf");

            if (!string.IsNullOrWhiteSpace(additionalHeaders))
            {
                var headerPairs = additionalHeaders.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var headerPair in headerPairs)
                {
                    var parts = headerPair.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                        _client.DefaultRequestHeaders.Add(parts[0].Trim(), parts[1].Trim());
                }
            }

            var json = JsonConvert.SerializeObject(payload);

            // 🔹 1️⃣ Estimate BEFORE sending
            var estimate = TokenEstimator.EstimateRequestTokens(payload, expectedResponseTokens: 1000);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = _client.PostAsync(endpoint, content).Result;
            stopwatch.Stop();

            var baseEndpoint = new Uri(endpoint).GetLeftPart(UriPartial.Path);
            var telemetry = new LlmTelemetry
            {
                Model = baseEndpoint,
                TimestampUtc = DateTime.UtcNow,
                LatencyMs = stopwatch.ElapsedMilliseconds,
                PromptCharacters = json.Length,
                EstimatedPromptTokens = estimate.EstimatedPromptTokens,
                EstimatedTotalTokens = estimate.EstimatedTotalTokens
            };

            if (response.IsSuccessStatusCode)
            {
                var responseString = response.Content.ReadAsStringAsync().Result;
                dynamic parsed = JsonConvert.DeserializeObject(responseString);

                if (parsed?.usageMetadata != null)
                {
                    telemetry.ActualPromptTokens =
                        parsed.usageMetadata.promptTokenCount ?? 0;

                    telemetry.ActualCompletionTokens =
                        parsed.usageMetadata.candidatesTokenCount ?? 0;

                    telemetry.ActualTotalTokens =
                        parsed.usageMetadata.totalTokenCount ?? 0;
                }

                if (parsed?.candidates != null && parsed.candidates.Count > 0)
                {
                    telemetry.FinishReason =
                        parsed.candidates[0].finishReason;
                }

                // 🔹 2️⃣ Compare Estimate vs Actual
                if (telemetry.ActualPromptTokens > 0)
                {
                    telemetry.EstimationErrorPercentage =
                        CalculateErrorPercentage(
                            telemetry.EstimatedPromptTokens,
                            telemetry.ActualPromptTokens);
                }
            }

            // 🔹 3️⃣ Persist or Log Telemetry
            telemetry.SaveTelemetry();

            return response;
        }

        private double CalculateErrorPercentage(int estimated, int actual)
        {
            if (actual == 0) return 0;

            return Math.Round(
                ((double)(estimated - actual) / actual) * 100,
                2);
        }



        private HttpResponseMessage PostWithAuthAndTelemetry2(
    string endpoint,
    object payload,
    string authHeaderName,
    string authHeaderValue,
    string additionalHeaders)
        {
            _client.DefaultRequestHeaders.Clear();

            if (!string.IsNullOrWhiteSpace(authHeaderName))
            {
                _client.DefaultRequestHeaders.Add(authHeaderName, authHeaderValue);
            }

            _client.DefaultRequestHeaders.Add("User-Agent", "Warewolf");

            if (!string.IsNullOrWhiteSpace(additionalHeaders))
            {
                var headerPairs = additionalHeaders.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var headerPair in headerPairs)
                {
                    var parts = headerPair.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        _client.DefaultRequestHeaders.Add(parts[0].Trim(), parts[1].Trim());
                    }
                }
            }

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = _client.PostAsync(endpoint, content).Result;
            stopwatch.Stop();

            //var telemetry = new LlmTelemetry
            //{
            //    Model = ExtractModelFromEndpoint(endpoint),
            //    TimestampUtc = DateTime.UtcNow,
            //    LatencyMs = stopwatch.ElapsedMilliseconds,
            //    PromptCharacters = json.Length
            //};



            if (response.IsSuccessStatusCode)
            {
                var promptTokens = 0;
                var completionTokens = 0;
                var TotalTokens = 0;
                object reason;

                var responseString = response.Content.ReadAsStringAsync().Result;

                dynamic parsed = JsonConvert.DeserializeObject(responseString);

                if (parsed?.usageMetadata != null)
                {
                    promptTokens = parsed.usageMetadata.promptTokenCount ?? 0;
                    completionTokens = parsed.usageMetadata.candidatesTokenCount ?? 0;
                    TotalTokens = parsed.usageMetadata.totalTokenCount ?? 0;
                }

                if (parsed?.candidates != null && parsed.candidates.Count > 0)
                {
                    reason = parsed.candidates[0].finishReason;
                }
            }

            return response;
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

        internal static bool IsOpenRouterEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return false;
            }
            return endpoint.ToLower().Contains("openrouter.ai");
        }

        /// <summary>
        /// LM Studio's newer REST API uses /api/v1/chat which requires an 'input' field instead of 'messages'.
        /// Transparently rewrite it to the OpenAI-compatible /v1/chat/completions path on the same host.
        /// </summary>
        internal static string NormalizeLmStudioEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return endpoint;
            }
            if (endpoint.EndsWith("/api/v1/chat", StringComparison.OrdinalIgnoreCase))
            {
                return endpoint.Substring(0, endpoint.Length - "/api/v1/chat".Length) + "/v1/chat/completions";
            }
            return endpoint;
        }

        // Expose provider-facing methods for new provider classes to call.
        public string SendToOpenAI_Public(ChatbotSourceDefinition source, List<object> messages)
        {
            return SendToOpenAI_Internal(source, messages);
        }

        public string SendToOpenRouter_Public(ChatbotSourceDefinition source, List<object> messages)
        {
            return SendToOpenRouter_Internal(source, messages);
        }

        public string SendToAnthropic_Public(ChatbotSourceDefinition source, List<object> messages)
        {
            return SendToAnthropic_Internal(source, messages);
        }

        public string SendToGemini_Public(ChatbotSourceDefinition source, List<object> messages)
        {
            return SendToGemini_Internal(source, messages);
        }

        // Internalized original provider implementations
        private string SendToOpenAI_Internal(ChatbotSourceDefinition source, List<object> messages)
        {
            var endpoint = NormalizeLmStudioEndpoint(source.CompletionsEndpoint);

            // When no API key is provided (local LLM servers like LM Studio / Ollama),
            // pass null auth header names so PostWithAuth skips authorization entirely.
            bool hasApiKey = !string.IsNullOrWhiteSpace(source.ApiKey);
            string authName  = hasApiKey ? "Authorization" : null;
            string authValue = hasApiKey ? $"Bearer {source.ApiKey}" : null;

            // Use max_tokens first — universally supported by OpenAI-compatible APIs including OpenRouter
            var payload = CreatePayload(source.SelectedModel, messages, null, useMaxCompletionTokens: false, includeTemperature: true);
            var response = PostWithAuth(endpoint, payload, authName, authValue, null);

            if (response.IsSuccessStatusCode)
            {
                return ReadAndParseResponse(response);
            }

            var errorContent = response.Content.ReadAsStringAsync().Result;

            // Retry without temperature if not supported by this model
            if (errorContent.Contains("temperature") && (errorContent.Contains("not support") || errorContent.Contains("does not support") || errorContent.Contains("unsupported")))
            {
                Dev2Logger.Info("Retrying without temperature parameter", GlobalConstants.WarewolfInfo);

                payload = CreatePayload(source.SelectedModel, messages, null, useMaxCompletionTokens: false, includeTemperature: false);
                response = PostWithAuth(endpoint, payload, authName, authValue, null);

                if (response.IsSuccessStatusCode)
                {
                    return ReadAndParseResponse(response);
                }

                errorContent = response.Content.ReadAsStringAsync().Result;
            }

            throw new HttpRequestException($"AI service error: {response.StatusCode} - {errorContent}");
        }

        private string SendToOpenRouter_Internal(ChatbotSourceDefinition source, List<object> messages)
        {
            // OpenRouter is OpenAI-compatible but does not support temperature for all routed models.
            // Use a minimal payload: messages + model + max_tokens, no temperature.
            var payload = CreatePayload(source.SelectedModel, messages, null, useMaxCompletionTokens: false, includeTemperature: false);
            return SendWithAuth(source.CompletionsEndpoint, payload, "Authorization", $"Bearer {source.ApiKey}", null);
        }

        private string SendToAnthropic_Internal(ChatbotSourceDefinition source, List<object> messages)
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

        private string SendToGemini_Internal(ChatbotSourceDefinition source, List<object> messages)
        {
            // Gemini uses a different payload format with "contents" and "parts"
            string systemPrompt = null;
            var contents = new List<object>();

            foreach (var msg in messages)
            {
                var json = JObject.FromObject(msg);
                var role = json["role"]?.ToString();

                // Gemini supports system instructions via a top-level "system_instruction" field
                if (role == "system")
                {
                    systemPrompt = (systemPrompt == null ? "" : systemPrompt + "\n\n") + json["content"]?.ToString();
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

            object requestBody;
            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                requestBody = new
                {
                    system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                    contents
                };
            }
            else
            {
                requestBody = new { contents };
            }

            // Gemini uses API key as query parameter and model in URL path
            const string baseUrl = "https://generativelanguage.googleapis.com/v1beta";
            var modelName = source.SelectedModel;
            var endpoint = $"{baseUrl}/{modelName}:generateContent?key={source.ApiKey}";

            return SendWithAuthAndTelemetry(endpoint, requestBody, null, null, null);
        }
    }

    public static class TokenEstimator
    {
        // Conservative average for Gemini-style models
        private const double CharsPerToken = 3.5;

        public static TokenEstimate EstimateRequestTokens(
            object requestPayload,
            int expectedResponseTokens = 1000)
        {
            if (requestPayload == null)
                throw new ArgumentNullException(nameof(requestPayload));

            // Serialize full payload exactly as sent
            var json = JsonConvert.SerializeObject(requestPayload);
            var totalChars = Encoding.UTF8.GetByteCount(json);

            // Approximate tokens
            int promptTokens = (int)Math.Ceiling(totalChars / CharsPerToken);
            int totalEstimated = promptTokens + expectedResponseTokens;

            return new TokenEstimate
            {
                CharacterCount = totalChars,
                EstimatedPromptTokens = promptTokens,
                EstimatedResponseTokens = expectedResponseTokens,
                EstimatedTotalTokens = totalEstimated
            };
        }
    }

    public class TokenEstimate
    {
        public int CharacterCount { get; set; }
        public int EstimatedPromptTokens { get; set; }
        public int EstimatedResponseTokens { get; set; }
        public int EstimatedTotalTokens { get; set; }

        public override string ToString()
        {
            return $"Chars: {CharacterCount}, " +
                   $"PromptTokens: {EstimatedPromptTokens}, " +
                   $"ResponseTokens: {EstimatedResponseTokens}, " +
                   $"Total: {EstimatedTotalTokens}";
        }
    }

    public class LlmTelemetry
    {
        public string Model { get; set; }
        public DateTime TimestampUtc { get; set; }

        public long LatencyMs { get; set; }

        public int PromptCharacters { get; set; }

        // Estimated
        public int EstimatedPromptTokens { get; set; }
        public int EstimatedTotalTokens { get; set; }

        // Actual
        public int ActualPromptTokens { get; set; }
        public int ActualCompletionTokens { get; set; }
        public int ActualTotalTokens { get; set; }

        public double EstimationErrorPercentage { get; set; }

        public string FinishReason { get; set; }

        public void SaveTelemetry()
        {


            try
            {
                var logMessage = new StringBuilder();

                logMessage.AppendLine("===== LLM TELEMETRY =====");
                logMessage.AppendLine($"Model: {Model}");
                logMessage.AppendLine($"Timestamp (UTC): {TimestampUtc:O}");
                logMessage.AppendLine($"Latency (ms): {LatencyMs}");
                logMessage.AppendLine($"Prompt Characters: {PromptCharacters}");

                logMessage.AppendLine("---- Token Estimates ----");
                logMessage.AppendLine($"Estimated Prompt Tokens: {EstimatedPromptTokens}");
                logMessage.AppendLine($"Estimated Total Tokens: {EstimatedTotalTokens}");

                logMessage.AppendLine("---- Actual Usage ----");
                logMessage.AppendLine($"Actual Prompt Tokens: {ActualPromptTokens}");
                logMessage.AppendLine($"Actual Completion Tokens: {ActualCompletionTokens}");
                logMessage.AppendLine($"Actual Total Tokens: {ActualTotalTokens}");

                logMessage.AppendLine($"Estimation Error (%): {EstimationErrorPercentage}");
                logMessage.AppendLine($"Finish Reason: {FinishReason}");

                // ⚠️ Large prompt warning
                if (ActualPromptTokens > 200_000)
                {
                    logMessage.AppendLine("WARNING: Very large prompt token usage detected.");
                }

                // ⚠️ Estimation deviation warning
                if (Math.Abs(EstimationErrorPercentage) > 15)
                {
                    logMessage.AppendLine("WARNING: Token estimation deviation exceeds 15%.");
                }

                logMessage.AppendLine("==========================");

                Dev2Logger.Debug(logMessage.ToString(), "SaveTelemetry");
            }
            catch (Exception ex)
            {
                Dev2Logger.Debug($"LLM Telemetry logging failed: {ex.Message}", "SaveTelemetry");
            }
        }

    }
}
