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
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);

                // Try Bearer authentication first (OpenAI, Azure OpenAI)
                try
                {
                    return FetchModelsWithAuth(client, source, "Authorization", $"Bearer {source.ApiKey}", null);
                }
                catch (HttpRequestException ex) when (IsAuthenticationError(ex))
                {
                    Dev2Logger.Info("Bearer authentication failed, retrying with x-api-key", GlobalConstants.WarewolfInfo);
                    
                    // Retry with Anthropic-style authentication
                    try
                    {
                        return FetchModelsWithAuth(client, source, "x-api-key", source.ApiKey, "anthropic-version=2023-06-01");
                    }
                    catch (Exception)
                    {
                        // If both methods fail, throw the original exception
                        throw new Exception($"Failed to authenticate with chatbot API: {ex.Message}", ex);
                    }
                }
            }
        }

        private static List<ChatbotModelDefinition> FetchModelsWithAuth(HttpClient client, ChatbotSource source, string authHeaderName, string authHeaderValue, string additionalHeaders)
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

            var response = client.GetAsync(source.ModelsEndpoint).Result;

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }

            var content = response.Content.ReadAsStringAsync().Result;
            return ParseModelsResponse(content, source.ModelsEndpoint);
        }

        private static List<ChatbotModelDefinition> ParseModelsResponse(string jsonContent, string endpoint)
        {
            var models = new List<ChatbotModelDefinition>();

            try
            {
                var json = JObject.Parse(jsonContent);

                // OpenAI / Azure OpenAI format: { "data": [ {...}, {...} ] }
                if (json["data"] is JArray dataArray)
                {
                    foreach (var modelToken in dataArray)
                    {
                        models.Add(new ChatbotModelDefinition
                        {
                            Id = modelToken["id"]?.ToString() ?? string.Empty,
                            Object = modelToken["object"]?.ToString() ?? "model",
                            Created = modelToken["created"]?.ToObject<long>() ?? 0,
                            OwnedBy = modelToken["owned_by"]?.ToString() ?? string.Empty
                        });
                    }
                }
                // Alternative format: direct array [ {...}, {...} ]
                else if (json.Type == JTokenType.Array)
                {
                    var array = JArray.Parse(jsonContent);
                    foreach (var modelToken in array)
                    {
                        models.Add(new ChatbotModelDefinition
                        {
                            Id = modelToken["id"]?.ToString() ?? string.Empty,
                            Object = modelToken["object"]?.ToString() ?? "model",
                            Created = modelToken["created"]?.ToObject<long>() ?? 0,
                            OwnedBy = modelToken["owned_by"]?.ToString() ?? string.Empty
                        });
                    }
                }
                // Anthropic doesn't have a models endpoint, return empty list
                else if (endpoint.Contains("anthropic.com"))
                {
                    Dev2Logger.Warn("Anthropic does not provide a models list endpoint", GlobalConstants.WarewolfWarn);
                    return GetAnthropicModels();
                }
            }
            catch (JsonException ex)
            {
                Dev2Logger.Error("Failed to parse models response", ex, GlobalConstants.WarewolfError);
                throw new Exception($"Failed to parse models response: {ex.Message}", ex);
            }

            return models;
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

        public override DynamicService CreateServiceEntry() => 
            EsbManagementServiceEntry.CreateESBManagementServiceEntry(
                HandlesType(), 
                "<DataList><ChatbotSourceId ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
            );

        public override string HandlesType() => nameof(FetchChatbotModels);
    }
}
