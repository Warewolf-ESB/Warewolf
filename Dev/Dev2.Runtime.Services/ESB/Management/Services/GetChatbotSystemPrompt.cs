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
using Warewolf.Configuration;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Builds and returns the chatbot system prompt on the server side,
    /// using the current saved settings (or overrides passed in the request).
    /// </summary>
    public class GetChatbotSystemPrompt : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            if (values is null)
            {
                throw new InvalidDataContractException(ErrorResource.NoParameter);
            }

            var settings = Config.Chatbot.Get();

            var result = new ExecuteMessage { HasError = false };
            result.SetMessage(ChatbotContextBuilder.BuildSystemPrompt(settings));

            return serializer.SerializeToBuilder(result);
        }

        public override DynamicService CreateServiceEntry() =>
            EsbManagementServiceEntry.CreateESBManagementServiceEntry(
                HandlesType(),
                "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => nameof(Warewolf.Service.GetChatbotSystemPrompt);
    }
}
