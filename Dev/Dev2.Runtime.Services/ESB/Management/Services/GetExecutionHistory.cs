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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Queue;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetExecutionHistory : DefaultEsbManagementEndpoint
    {
        public IQueueResource SelectedQueue { get; private set; }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                var serializer = new Dev2JsonSerializer();

                if (values == null)
                {
                    throw new InvalidDataContractException(ErrorResource.NoParameter);
                }
                string queueName = null;
                values.TryGetValue("QueueName", out StringBuilder tmp);
                if (tmp != null)
                {
                    queueName = tmp.ToString();
                }

                if (string.IsNullOrEmpty(queueName))
                {
                    var message = new ExecuteMessage();
                    message.HasError = true;
                    message.SetMessage("No Queue History found");
                    Dev2Logger.Debug("No Queue History found", GlobalConstants.WarewolfDebug);
                    return serializer.SerializeToBuilder(message);
                }
                var historyResults = new List<IExecutionHistory>();
                //TODO this is where I will get the data from the execution log and build the List<IExecutionHistory>() from results
                return serializer.SerializeToBuilder(historyResults);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;
        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList></DataList>");

        public override string HandlesType() => "GetExecutionHistory";
    }
}
