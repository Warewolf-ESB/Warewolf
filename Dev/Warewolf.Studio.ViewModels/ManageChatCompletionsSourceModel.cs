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
    public class ManageChatCompletionsSourceModel : IManageChatCompletionsSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageChatCompletionsSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }
        }

        #region Implementation of IManageChatCompletionsSourceModel

        public void TestConnection(IChatCompletionsSource resource)
        {
            // Test the chat completions API endpoint by calling the models endpoint
            try
            {
                var modelsEndpoint = ReconstructModelsEndpoint(resource.CompletionsEndpoint);
                
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {resource.ApiKey}");
                    client.DefaultRequestHeaders.Add("User-Agent", "Warewolf");
                    
                    var response = client.GetAsync(modelsEndpoint).Result;
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        throw new Exception($"Chat Completions API connection failed: {response.StatusCode} - {content}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to Chat Completions API: {ex.Message}", ex);
            }
        }

        private string ReconstructModelsEndpoint(string completionsEndpoint)
        {
            if (string.IsNullOrEmpty(completionsEndpoint))
            {
                throw new ArgumentException("Completions endpoint cannot be null or empty", nameof(completionsEndpoint));
            }

            // Remove "chat/completions" from the endpoint and replace with "models"
            // Handle various possible formats:
            // - https://api.example.com/v1/chat/completions -> https://api.example.com/v1/models
            // - https://api.example.com/chat/completions -> https://api.example.com/models
            
            var uri = new Uri(completionsEndpoint);
            var path = uri.AbsolutePath;
            
            // Replace "chat/completions" with "models"
            if (path.Contains("chat/completions"))
            {
                path = path.Replace("chat/completions", "models");
            }
            else if (path.EndsWith("/completions"))
            {
                // Handle case where it might just be "/completions"
                path = path.Substring(0, path.LastIndexOf("/completions")) + "/models";
            }
            else if (path.EndsWith("/chat"))
            {
                // Handle case where it might be "/chat"
                path = path.Substring(0, path.LastIndexOf("/chat")) + "/models";
            }
            else
            {
                // If no recognizable pattern, just append /models
                path = path.TrimEnd('/') + "/models";
            }
            
            var modelsEndpoint = $"{uri.Scheme}://{uri.Authority}{path}";
            return modelsEndpoint;
        }

        public void Save(IChatCompletionsSource toSource)
        {
            _updateRepository.Save(toSource);
        }

        public string ServerName { get; set; }

        public IChatCompletionsSource FetchSource(Guid id)
        {
            var xaml = _queryProxy.FetchResourceXaml(id);
            var source = new Dev2.Data.ServiceModel.ChatCompletionsSource(xaml.ToXElement());

            var def = new ChatCompletionsSourceDefinition
            {
                Id = source.ResourceID,
                Name = source.ResourceName,
                Path = source.GetSavePath(),
                ApiKey = source.ApiKey,
                CompletionsEndpoint = source.CompletionsEndpoint
            };
            return def;
        }

        #endregion
    }
}
