#pragma warning disable
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
using System.Net.Http;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManageChatbotSourceModel : IManageChatbotSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageChatbotSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }
        }

        #region Implementation of IManageChatbotSourceModel

        public void TestConnection(IChatbotSource resource)
        {
            // Test the chat completions API endpoint by calling the models endpoint
            try
            {
                var modelsEndpoint = string.IsNullOrEmpty(resource.ModelsEndpoint) 
                    ? resource.CompletionsEndpoint 
                    : resource.ModelsEndpoint;
                
                using (var client = new HttpClient())
                {
                    // Check if this is a Google Gemini endpoint
                    var isGemini = IsGoogleGeminiEndpoint(modelsEndpoint);
                    
                    if (isGemini)
                    {
                        // Google Gemini uses API key as a query parameter
                        var separator = modelsEndpoint.Contains("?") ? "&" : "?";
                        modelsEndpoint = $"{modelsEndpoint}{separator}key={resource.ApiKey}";
                    }
                    else
                    {
                        // Other providers use Bearer token authentication
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {resource.ApiKey}");
                    }
                    
                    client.DefaultRequestHeaders.Add("User-Agent", "Warewolf");
                    
                    var response = client.GetAsync(modelsEndpoint).Result;
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        throw new Exception($"Chatbot API connection failed: {response.StatusCode} - {content}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to Chatbot API: {ex.Message}", ex);
            }
        }

        private static bool IsGoogleGeminiEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return false;
            }

            var lowerEndpoint = endpoint.ToLower();
            return lowerEndpoint.Contains("generativelanguage.googleapis.com") || lowerEndpoint.Contains("gemini");
        }

        public void Save(IChatbotSource toSource)
        {
            _updateRepository.Save(toSource);
        }

        public string ServerName { get; set; }

        public IChatbotSource FetchSource(Guid id)
        {
            var xaml = _queryProxy.FetchResourceXaml(id);
            var source = new Dev2.Data.ServiceModel.ChatbotSource(xaml.ToXElement());

            var def = new ChatbotSourceDefinition
            {
                Id = source.ResourceID,
                Name = source.ResourceName,
                Path = source.GetSavePath(),
                ApiKey = source.ApiKey,
                CompletionsEndpoint = source.CompletionsEndpoint,
                ModelsEndpoint = source.ModelsEndpoint
            };
            return def;
        }

        #endregion
    }
}
