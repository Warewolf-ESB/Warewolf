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
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchChatbotModels : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            try
            {
                if (values == null)
                {
                    throw new InvalidDataContractException(ErrorResource.NoParameter);
                }

                values.TryGetValue("ChatbotSourceId", out StringBuilder sourceIdBuilder);

                if (sourceIdBuilder == null || !Guid.TryParse(sourceIdBuilder.ToString(), out Guid sourceId))
                {
                    throw new ArgumentException("ChatbotSourceId is required and must be a valid GUID");
                }

                Dev2Logger.Info($"Fetch Chatbot Models for source: {sourceId}", GlobalConstants.WarewolfInfo);

                var chatbotSource = ResourceCatalog.Instance.GetResource<ChatbotSource>(GlobalConstants.ServerWorkspaceID, sourceId);

                if (chatbotSource == null)
                {
                    throw new Exception($"ChatbotSource with ID {sourceId} not found");
                }

                if (string.IsNullOrWhiteSpace(chatbotSource.ModelsEndpoint))
                {
                    throw new Exception("ModelsEndpoint is not configured for this ChatbotSource");
                }

                var models = FetchModelsFromProvider(chatbotSource);

                return serializer.SerializeToBuilder(new ExecuteMessage 
                { 
                    HasError = false, 
                    Message = serializer.SerializeToBuilder(models) 
                });
            }
            catch (Exception err)
            {
                Dev2Logger.Error("FetchChatbotModels Error", err, GlobalConstants.WarewolfError);
                return serializer.SerializeToBuilder(new ExecuteMessage 
                { 
                    HasError = true, 
                    Message = new StringBuilder(err.Message) 
                });
            }
        }

        private static List<ChatbotModelDefinition> FetchModelsFromProvider(ChatbotSource source)
        {
            // Anthropic doesn't have a models list endpoint - return known models directly
            if (IsAnthropicEndpoint(source.ModelsEndpoint) || IsAnthropicEndpoint(source.CompletionsEndpoint))
            {
                return GetAnthropicModels();
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(120);

                if (IsGoogleGeminiEndpoint(source.ModelsEndpoint))
                {
                    // Gemini uses API key as query parameter
                    var endpoint = source.ModelsEndpoint;
                    var separator = endpoint.Contains("?") ? "&" : "?";
                    endpoint = $"{endpoint}{separator}key={source.ApiKey}";
                    return FetchModelsWithAuth(client, endpoint, null, null, null);
                }

                // Default: OpenAI-compatible (OpenAI, XAI, GitHub Models, Azure OpenAI, etc.)
                return FetchModelsWithAuth(client, source.ModelsEndpoint, "Authorization", $"Bearer {source.ApiKey}", null);
            }
        }

        private static List<ChatbotModelDefinition> FetchModelsWithAuth(HttpClient client, string endpoint, string authHeaderName, string authHeaderValue, string additionalHeaders)
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

            var response = client.GetAsync(endpoint).Result;

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }

            var content = response.Content.ReadAsStringAsync().Result;
            Dev2Logger.Info($"[FetchChatbotModels] Raw response from {endpoint}: {content}", GlobalConstants.WarewolfInfo);
            return ParseModelsResponse(content);
        }

        private static List<ChatbotModelDefinition> ParseModelsResponse(string jsonContent)
        {
            var models = new List<ChatbotModelDefinition>();

            try
            {
                var json = JToken.Parse(jsonContent);

                // OpenAI / Azure OpenAI format: { "data": [ {...}, {...} ] }
                if (json is JObject obj && obj["data"] is JArray dataArray)
                {
                    Dev2Logger.Info($"[FetchChatbotModels] Matched OpenAI/data-array format, {dataArray.Count} items", GlobalConstants.WarewolfInfo);
                    foreach (var modelToken in dataArray)
                    {
                        var resolvedId = ResolveModelId(modelToken);
                        Dev2Logger.Info($"[FetchChatbotModels] Token keys: {string.Join(", ", ((JObject)modelToken).Properties().Select(p => p.Name))} -> resolvedId: '{resolvedId}'", GlobalConstants.WarewolfInfo);
                        models.Add(new ChatbotModelDefinition
                        {
                            Id = resolvedId,
                            DisplayName = modelToken["display_name"]?.ToString() ?? modelToken["displayName"]?.ToString(),
                            Object = modelToken["object"]?.ToString() ?? "model",
                            Created = modelToken["created"]?.ToObject<long>() ?? 0,
                            OwnedBy = modelToken["owned_by"]?.ToString() ?? string.Empty
                        });
                    }
                }
				// Direct array format: [ {...}, {...} ]
#pragma warning disable CC0021 // Use nameof
				else if (json is JArray array)
                {
                    Dev2Logger.Info($"[FetchChatbotModels] Matched direct-array format, {array.Count} items", GlobalConstants.WarewolfInfo);
                    foreach (var modelToken in array)
                    {
                        var resolvedId = ResolveModelId(modelToken);
                        Dev2Logger.Info($"[FetchChatbotModels] Token keys: {string.Join(", ", ((JObject)modelToken).Properties().Select(p => p.Name))} -> resolvedId: '{resolvedId}'", GlobalConstants.WarewolfInfo);
                        models.Add(new ChatbotModelDefinition
                        {
                            Id = resolvedId,
                            DisplayName = modelToken["display_name"]?.ToString() ?? modelToken["displayName"]?.ToString(),
                            Object = modelToken["object"]?.ToString() ?? "model",
                            Created = modelToken["created"]?.ToObject<long>() ?? 0,
                            OwnedBy = modelToken["owned_by"]?.ToString() ?? string.Empty
                        });
                    }
                }
                // { "models": [ ... ] } format — covers Google Gemini ("name"/"displayName")
                // and LM Studio ("key"/"display_name") and similar providers
                else if (json is JObject geminiObj && geminiObj["models"] is JArray geminiModels)
                {
                    Dev2Logger.Info($"[FetchChatbotModels] Matched models-array format, {geminiModels.Count} items", GlobalConstants.WarewolfInfo);
                    foreach (var modelToken in geminiModels)
                    {
                        // Gemini uses "name", LM Studio uses "key"; fall back through all candidates
                        var modelId = modelToken["name"]?.ToString();
                        if (string.IsNullOrWhiteSpace(modelId)) modelId = modelToken["key"]?.ToString();
                        if (string.IsNullOrWhiteSpace(modelId)) modelId = modelToken["id"]?.ToString();

                        // Prefer a human-friendly display name where available
                        var displayName = modelToken["displayName"]?.ToString();
                        if (string.IsNullOrWhiteSpace(displayName)) displayName = modelToken["display_name"]?.ToString();

                        var ownedBy = modelToken["publisher"]?.ToString()
                                   ?? modelToken["owned_by"]?.ToString()
                                   ?? string.Empty;

                        Dev2Logger.Info($"[FetchChatbotModels] models-array token: id='{modelId}', displayName='{displayName}', ownedBy='{ownedBy}'", GlobalConstants.WarewolfInfo);

                        models.Add(new ChatbotModelDefinition
                        {
                            Id = modelId ?? string.Empty,
                            DisplayName = displayName,
                            Object = "model",
                            Created = 0,
                            OwnedBy = ownedBy
                        });
                    }
                }
                else
                {
                    Dev2Logger.Warn($"[FetchChatbotModels] No format matched. JSON type: {json.Type}, root keys: {(json is JObject jo ? string.Join(", ", jo.Properties().Select(p => p.Name)) : "N/A")}", GlobalConstants.WarewolfInfo);
                }
#pragma warning restore CC0021 // Use nameof
			}
            catch (JsonException ex)
            {
                Dev2Logger.Error("Failed to parse models response", ex, GlobalConstants.WarewolfError);
                throw new Exception($"Failed to parse models response: {ex.Message}", ex);
            }

            var result = models.Where(m => !string.IsNullOrWhiteSpace(m.Id)).ToList();
            Dev2Logger.Info($"[FetchChatbotModels] Parsed {models.Count} models, {result.Count} with non-empty Id", GlobalConstants.WarewolfInfo);
            return result;
        }

        /// <summary>
        /// Resolves a model identifier from a JSON token, trying multiple field names
        /// to support OpenAI ("id"), Ollama ("name"/"model"), and other provider formats.
        /// </summary>
        private static string ResolveModelId(JToken modelToken)
        {
            var id = modelToken["id"]?.ToString();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            // Ollama format uses "name" (e.g. "llama3:latest") or "model"
            var name = modelToken["name"]?.ToString();
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            var model = modelToken["model"]?.ToString();
            if (!string.IsNullOrWhiteSpace(model))
            {
                return model;
            }

            return string.Empty;
        }

        private static List<ChatbotModelDefinition> GetAnthropicModels()
        {
            // Anthropic doesn't have a models list API, return known models
            return new List<ChatbotModelDefinition>
            {
                new ChatbotModelDefinition { Id = "claude-3-opus-20240229", Object = "model", OwnedBy = "anthropic", Created = 0 },
                new ChatbotModelDefinition { Id = "claude-3-sonnet-20240229", Object = "model", OwnedBy = "anthropic", Created = 0 },
                new ChatbotModelDefinition { Id = "claude-3-haiku-20240307", Object = "model", OwnedBy = "anthropic", Created = 0 },
                new ChatbotModelDefinition { Id = "claude-2.1", Object = "model", OwnedBy = "anthropic", Created = 0 },
                new ChatbotModelDefinition { Id = "claude-2.0", Object = "model", OwnedBy = "anthropic", Created = 0 }
            };
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

        public override DynamicService CreateServiceEntry() => 
            EsbManagementServiceEntry.CreateESBManagementServiceEntry(
                HandlesType(), 
                "<DataList><ChatbotSourceId ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
            );

        public override string HandlesType() => nameof(FetchChatbotModels);
    }
}
