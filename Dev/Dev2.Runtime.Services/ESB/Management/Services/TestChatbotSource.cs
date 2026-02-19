/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestChatbotSource : IEsbManagementEndpoint
    {
        public const string ChatbotSource = "ChatbotSource";

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Test Chatbot Source", GlobalConstants.WarewolfInfo);
                msg.HasError = false;
                values.TryGetValue(ChatbotSource, out StringBuilder resourceDefinition);

                var chatbotSourceDefinition = serializer.Deserialize<ChatbotSourceDefinition>(resourceDefinition);

                // For Google Gemini endpoints the authentication requirements differ.
                // The Models/ListModels endpoint may require an OAuth2 access token (Bearer) and
                // using the plain API key in an Authorization header will not work. Older code
                // attempted Bearer with the stored ApiKey which is incorrect for Gemini model listing.
                // Try a Gemini-specific request pattern first (API key as query param) and
                // provide a helpful error if the server requires OAuth2 tokens instead.
                try
                {
                    if (IsGoogleGeminiEndpoint(chatbotSourceDefinition.ModelsEndpoint))
                    {
                        // Try using API key as a query parameter (some Gemini endpoints accept this for certain calls)
                        TestGeminiConnection(chatbotSourceDefinition);
                        msg.HasError = false;
                        msg.Message = new StringBuilder("Connection successful");
                    }
                    else
                    {
                        // Try with Bearer authentication first for non-Gemini endpoints
                        try
                        {
                            TestConnectionWithAuth(chatbotSourceDefinition, "Authorization", $"Bearer {chatbotSourceDefinition.ApiKey}", null);
                            msg.HasError = false;
                            msg.Message = new StringBuilder("Connection successful");
                        }
                        catch (HttpRequestException ex) when (IsAuthenticationError(ex))
                        {
                            Dev2Logger.Info("Bearer authentication failed, retrying with x-api-key authentication", GlobalConstants.WarewolfInfo);

                            // Retry with Claude-style authentication
                            try
                            {
                                TestConnectionWithAuth(chatbotSourceDefinition, "x-api-key", chatbotSourceDefinition.ApiKey, "anthropic-version=2023-06-01");
                                msg.HasError = false;
                                msg.Message = new StringBuilder("Connection successful");
                            }
                            catch (Exception)
                            {
                                msg.HasError = true;
                                msg.Message = new StringBuilder($"Authentication failed with both Bearer and x-api-key methods. Original error: {ex.Message}");
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    // If Gemini explicitly complains about unsupported token types, give clearer guidance
                    var lower = ex.Message?.ToLower() ?? string.Empty;
                    if (lower.Contains("access_token_type_unsupported") || lower.Contains("expected oauth") || lower.Contains("unauthenticated"))
                    {
                        msg.HasError = true;
                        msg.Message = new StringBuilder($"Authentication failed for Google Gemini models endpoint. The API is indicating that an OAuth2 access token is required rather than an API key. Original error: {ex.Message}");
                    }
                    else
                    {
                        msg.HasError = true;
                        msg.Message = new StringBuilder($"Failed to connect to Chatbot API: {ex.Message}");
                    }
                }
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder($"Failed to connect to Chatbot API: {err.Message}");
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
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

        private static bool IsGoogleGeminiEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return false;
            }

            var lower = endpoint.ToLower();
            return lower.Contains("generativelanguage.googleapis.com") || lower.Contains("gemini");
        }

        private static void TestGeminiConnection(ChatbotSourceDefinition chatbotSourceDefinition)
        {
            using (var client = new HttpClient())
            {
                // Gemini model listing may accept API key as query parameter for simple access,
                // otherwise it requires an OAuth2 access token. Try adding the key as a query
                // param and performing a GET.
                var url = chatbotSourceDefinition.ModelsEndpoint;
                // Only append the API key as a query parameter if the endpoint does not
                // already contain a key parameter (case-insensitive check).
                if (url.IndexOf("key=", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    var separator = url.Contains("?") ? "&" : "?";
                    url = url + separator + "key=" + chatbotSourceDefinition.ApiKey;
                }

                // Keep consistency with other methods by using the same pragma to satisfy CC0021
#pragma warning disable CC0021 // Use nameof
                client.DefaultRequestHeaders.Add("User-Agent", "Warewolf");
#pragma warning restore CC0021 // Use nameof

                var response = client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    throw new HttpRequestException($"Chatbot API connection failed: {response.StatusCode} - {content}");
                }
            }
        }

        private static void TestConnectionWithAuth(ChatbotSourceDefinition chatbotSourceDefinition, string authHeaderName, string authHeaderValue, string additionalHeaders)
        {
            using (var client = new HttpClient())
            {
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

                var response = client.GetAsync(chatbotSourceDefinition.ModelsEndpoint).Result;
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    throw new HttpRequestException($"Chatbot API connection failed: {response.StatusCode} - {content}");
                }
            }
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ChatbotSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => nameof(TestChatbotSource);
    }
}
