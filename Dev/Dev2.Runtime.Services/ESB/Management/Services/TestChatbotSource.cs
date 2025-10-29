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
                
                // Test the connection by calling the models endpoint
                var modelsEndpoint = ReconstructModelsEndpoint(chatbotSourceDefinition.CompletionsEndpoint);
                
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {chatbotSourceDefinition.ApiKey}");
                    client.DefaultRequestHeaders.Add("User-Agent", "Warewolf");
                    
                    var response = client.GetAsync(modelsEndpoint).Result;
                    
                    if (response.IsSuccessStatusCode)
                    {
                        msg.HasError = false;
                        msg.Message = new StringBuilder("Connection successful");
                    }
                    else
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        msg.HasError = true;
                        msg.Message = new StringBuilder($"Chatbot API connection failed: {response.StatusCode} - {content}");
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

        private static string ReconstructModelsEndpoint(string completionsEndpoint)
        {
            if (string.IsNullOrEmpty(completionsEndpoint))
            {
                throw new ArgumentException("Completions endpoint cannot be null or empty", nameof(completionsEndpoint));
            }

            // Remove "chat/completions" from the endpoint and replace with "models"
            var uri = new Uri(completionsEndpoint);
            var path = uri.AbsolutePath;
            
            // Replace "chat/completions" with "models"
            if (path.Contains("chat/completions"))
            {
                path = path.Replace("chat/completions", "models");
            }
            else if (path.EndsWith("/completions"))
            {
                path = path.Substring(0, path.LastIndexOf("/completions")) + "/models";
            }
            else if (path.EndsWith("/chat"))
            {
                path = path.Substring(0, path.LastIndexOf("/chat")) + "/models";
            }
            else
            {
                path = path.TrimEnd('/') + "/models";
            }
            
            var modelsEndpoint = $"{uri.Scheme}://{uri.Authority}{path}";
            return modelsEndpoint;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ChatbotSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => nameof(TestChatbotSource);
    }
}
