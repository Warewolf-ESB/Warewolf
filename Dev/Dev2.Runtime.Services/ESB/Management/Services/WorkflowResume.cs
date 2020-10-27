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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Web;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Warewolf.Storage;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class WorkflowResume : WorkflowManagementEndpointAbstract
    {
        public override string HandlesType() => nameof(WorkflowResume);

        protected override ExecuteMessage ExecuteImpl(Dev2JsonSerializer serializer, Guid resourceId, Dictionary<string, StringBuilder> values)
        {
            values.TryGetValue("versionNumber", out StringBuilder versionNumber);
            if (versionNumber == null)
            {
                throw new InvalidDataContractException("no version Number passed");
            }
            values.TryGetValue("environment", out StringBuilder environmentString);
            if (environmentString == null)
            {
                throw new InvalidDataContractException("no environment passed");
            }
            values.TryGetValue("startActivityId", out StringBuilder startActivityIdString);
            if (startActivityIdString == null)
            {
                startActivityIdString = new StringBuilder(resourceId.ToString());
            }
            if (!Guid.TryParse(startActivityIdString.ToString(), out Guid startActivityId))
            {
                throw new InvalidDataContractException("startActivityId is not a valid GUID.");
            }
            var decodedEnv = HttpUtility.UrlDecode(environmentString.ToString());
            var executionEnv = new ExecutionEnvironment();
            executionEnv.FromJson(decodedEnv);

            Int32.TryParse(versionNumber.ToString(), out int parsedVersionNumber);
            var dataObject = new DsfDataObject("", Guid.NewGuid())
            {
                ResourceID = resourceId,
                Environment = executionEnv,                                                
                VersionNumber = parsedVersionNumber,
                ExecutingUser = Thread.CurrentPrincipal,
                IsDebug = true
            };
            var dynamicService = ResourceCatalog.Instance.GetService(GlobalConstants.ServerWorkspaceID, resourceId, "");
            if (dynamicService is null)
            {
                return new ExecuteMessage { HasError = true, Message = new StringBuilder($"Error resuming. ServiceAction is null for Resource ID:{resourceId}") };
            }
            var sa = dynamicService.Actions.FirstOrDefault();
            if (sa is null)
            {
                return new ExecuteMessage { HasError = true, Message = new StringBuilder($"Error resuming. ServiceAction is null for Resource ID:{resourceId}") };
            }
            var container = CustomContainer.Get<IResumableExecutionContainerFactory>().New(startActivityId, sa, dataObject);
            container.Execute(out ErrorResultTO errors, 0);
            if (errors.HasErrors())
            {
                return new ExecuteMessage { HasError = true, Message = new StringBuilder(errors.MakeDisplayReady()) };
            }
            return new ExecuteMessage { HasError = false, Message = new StringBuilder("Execution Completed.") };
        }

    }
}
