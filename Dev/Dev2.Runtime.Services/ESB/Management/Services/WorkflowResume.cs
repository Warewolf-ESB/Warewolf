#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Warewolf.Storage;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class WorkflowResume : WorkflowManagementEndpointAbstract
    {
        public override string HandlesType() => nameof(WorkflowResume);

        static string GetUnqualifiedName(string userName)
        {
            if (userName.Contains("\\"))
            {
                return userName.Split('\\').Last().Trim();
            }

            return userName;
        }

        protected override ExecuteMessage ExecuteImpl(Dev2JsonSerializer serializer, Guid resourceId, Dictionary<string, StringBuilder> values)
        {
            var versionNumber = IsValid(values, out var environmentString, out var startActivityId, out var currentUserPrincipal);
            var executingUser = ClaimsPrincipal(resourceId, currentUserPrincipal, out var unqualifiedUserName, out var key);

            var decodedEnv = HttpUtility.UrlDecode(environmentString.ToString());
            var executionEnv = new ExecutionEnvironment();
            executionEnv.FromJson(decodedEnv);
            Int32.TryParse(versionNumber.ToString(), out int parsedVersionNumber);

            var dataObject = new DsfDataObject("", Guid.NewGuid())
            {
                ResourceID = resourceId,
                Environment = executionEnv,
                VersionNumber = parsedVersionNumber,
                ExecutingUser = executingUser,
                IsDebug = true
            };
            var isAuthorized = dataObject.AuthCache.GetOrAdd(key, (requestedKey) => ServerAuthorizationService.Instance.IsAuthorized(executingUser, AuthorizationContext.Execute, resourceId));
            if (!isAuthorized)
            {
                return new ExecuteMessage {HasError = true, Message = new StringBuilder($"Authentication Error resuming. User " + unqualifiedUserName + " is not authorized to execute the workflow.")};
            }

            var dynamicService = ResourceCatalog.Instance.GetService(GlobalConstants.ServerWorkspaceID, resourceId, "");
            if (dynamicService is null)
            {
                return new ExecuteMessage {HasError = true, Message = new StringBuilder($"Error resuming. ServiceAction is null for Resource ID:{resourceId}")};
            }

            var sa = dynamicService.Actions.FirstOrDefault();
            if (sa is null)
            {
                return new ExecuteMessage {HasError = true, Message = new StringBuilder($"Error resuming. ServiceAction is null for Resource ID:{resourceId}")};
            }

            var container = CustomContainer.Get<IResumableExecutionContainerFactory>().New(startActivityId, sa, dataObject);
            container.Execute(out ErrorResultTO errors, 0);
            if (errors.HasErrors())
            {
                return new ExecuteMessage {HasError = true, Message = new StringBuilder(errors.MakeDisplayReady())};
            }

            return new ExecuteMessage {HasError = false, Message = new StringBuilder("Execution Completed.")};
        }

        private static ClaimsPrincipal ClaimsPrincipal(Guid resourceId, StringBuilder currentUserPrincipal, out string unqualifiedUserName, out (ClaimsPrincipal executingUser, AuthorizationContext Execute, string) key)
        {
            ClaimsPrincipal executingUser;
            unqualifiedUserName = GetUnqualifiedName(currentUserPrincipal.ToString()).Trim();

            try
            {
                executingUser = new WindowsPrincipal(new WindowsIdentity(unqualifiedUserName));
            }
            catch
            {
                var genericIdentity = new GenericIdentity(unqualifiedUserName);
                executingUser = new GenericPrincipal(genericIdentity, new string[0]);
            }

            key = (executingUser, AuthorizationContext.Execute, resourceId.ToString());
            return executingUser;
        }

        private static StringBuilder IsValid(Dictionary<string, StringBuilder> values, out StringBuilder environmentString, out Guid startActivityId, out StringBuilder currentuserprincipal)
        {
            values.TryGetValue("versionNumber", out StringBuilder versionNumber);
            if (versionNumber == null)
            {
                throw new InvalidDataContractException("no version Number passed");
            }

            values.TryGetValue("environment", out environmentString);
            if (environmentString == null)
            {
                throw new InvalidDataContractException("no environment passed");
            }

            values.TryGetValue("startActivityId", out StringBuilder startActivityIdString);
            if (startActivityIdString == null)
            {
                throw new InvalidDataContractException("no startActivityId passed.");
            }

            if (!Guid.TryParse(startActivityIdString.ToString(), out startActivityId))
            {
                throw new InvalidDataContractException("startActivityId is not a valid GUID.");
            }

            values.TryGetValue("currentuserprincipal", out currentuserprincipal);
            if (currentuserprincipal == null)
            {
                throw new InvalidDataContractException("no executing user principal passed");
            }

            return versionNumber;
        }
    }
}