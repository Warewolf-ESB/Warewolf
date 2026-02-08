/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetChatbotSettings : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            if (values is null)
            {
                throw new InvalidDataContractException(ErrorResource.NoParameter);
            }
            
            var settings = Config.Chatbot.Get();

            var removeApiKey = false;
            if (values.TryGetValue("RemoveApiKey", out StringBuilder removeApiKeyValue))
            {
                bool.TryParse(removeApiKeyValue?.ToString(), out removeApiKey);
            }

            if (removeApiKey)
            {
                settings = settings.Clone();
                if (settings.ChatbotSource != null)
                {
                    settings.ChatbotSource.Payload = "";
                }
                return serializer.SerializeToBuilder(settings);
            }

            // Check if caller wants plaintext/unencrypted settings
            var shouldDecrypt = false;
            if (values.TryGetValue("DecryptDataSource", out StringBuilder decryptValue))
            {
                bool.TryParse(decryptValue?.ToString(), out shouldDecrypt);
            }

            // Decrypt the ChatbotSource payload if requested
            if (shouldDecrypt && settings.ChatbotSource?.Payload != null)
            {
                settings.ChatbotSource.Payload = DpapiWrapper.DecryptIfEncrypted(settings.ChatbotSource.Payload);
            }

            return serializer.SerializeToBuilder(settings);
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><RemoveApiKey ColumnIODirection=\"Input\"/><DecryptDataSource ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => nameof(Warewolf.Service.GetChatbotSettings);
    }
}
