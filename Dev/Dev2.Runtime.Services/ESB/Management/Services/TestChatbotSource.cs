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
                
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {chatbotSourceDefinition.ApiKey}");
#pragma warning disable CC0021 // Use nameof
					client.DefaultRequestHeaders.Add("User-Agent", "Warewolf");
#pragma warning restore CC0021 // Use nameof

                    var response = client.GetAsync(chatbotSourceDefinition.ModelsEndpoint).Result;
                    
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

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ChatbotSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => nameof(TestChatbotSource);
    }
}
