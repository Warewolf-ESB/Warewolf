#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Security.Principal;
using System.Text;
using System.Web;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;
using Warewolf.Storage;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class WorkflowResume : WorkflowManagementEndpointAbstract
    {
        private IAuthorizationService _authorizationService;
        private IResourceCatalog _resourceCatalog;

        public override string HandlesType() => nameof(WorkflowResume);

        protected override ExecuteMessage ExecuteImpl(Dev2JsonSerializer serializer, Guid resourceId, Dictionary<string, StringBuilder> values)
        {
            try
            {
                var versionNumber = IsValid(values, out var environmentString, out var startActivityId, out var currentUserPrincipal);
                var executingUser = BuildClaimsPrincipal(DpapiWrapper.DecryptIfEncrypted(currentUserPrincipal.ToString()));
                var environment = DpapiWrapper.DecryptIfEncrypted(environmentString.ToString());

                var decodedEnv = HttpUtility.UrlDecode(environment);
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

                if(!CanExecute(dataObject))
                {
                    var errorMessage = string.Format(ErrorResource.AuthenticationError, executingUser.Identity.Name);
                    Dev2Logger.Error(errorMessage, GlobalConstants.WarewolfError);
                    return new ExecuteMessage { HasError = true, Message = new StringBuilder(errorMessage) };
                }

                var dynamicService = ResourceCatalogInstance.GetService(GlobalConstants.ServerWorkspaceID, resourceId, "");
                if(dynamicService is null)
                {
                    return new ExecuteMessage { HasError = true, Message = new StringBuilder($"Error resuming. ServiceAction is null for Resource ID:{resourceId}") };
                }

                var sa = dynamicService.Actions.FirstOrDefault();
                if(sa is null)
                {
                    return new ExecuteMessage { HasError = true, Message = new StringBuilder($"Error resuming. ServiceAction is null for Resource ID:{resourceId}") };
                }
                Dev2Logger.Info($"ServiceName: {sa.ServiceName}", GlobalConstants.WarewolfInfo);

                // ReSharper disable once RedundantAssignment
                var errorResultTO = new ErrorResultTO();
                var container = CustomContainer.Get<IResumableExecutionContainerFactory>().New(startActivityId, sa, dataObject);
                Dev2Logger.Info($"Container.Execute", GlobalConstants.WarewolfInfo);
                container.Execute(out ErrorResultTO errors, 0);
                errorResultTO = errors;

                if(errorResultTO.HasErrors())
                {
                    return new ExecuteMessage { HasError = true, Message = new StringBuilder(errorResultTO.MakeDisplayReady()) };
                }

                return new ExecuteMessage { HasError = false, Message = new StringBuilder("Execution Completed.") };
            }
            catch(Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                return new ExecuteMessage { HasError = true, Message = new StringBuilder(err.Message) };
            }
        }

        public IResourceCatalog ResourceCatalogInstance { get => _resourceCatalog ?? ResourceCatalog.Instance; set => _resourceCatalog = value; }

        public IAuthorizationService AuthorizationService { get => _authorizationService ?? ServerAuthorizationService.Instance; set => _authorizationService = value; }

        private bool CanExecute(DsfDataObject dataObject)
        {
            var key = (dataObject.ExecutingUser, AuthorizationContext.Execute, dataObject.ResourceID.ToString());
            var isAuthorized = dataObject.AuthCache.GetOrAdd(key, (requestedKey) => AuthorizationService.IsAuthorized(dataObject.ExecutingUser, AuthorizationContext.Execute, dataObject.Resource));
            return isAuthorized;
        }

        static string GetUnqualifiedName(string userName)
        {
            if(userName.Contains("\\"))
            {
                return userName.Split('\\').Last().Trim();
            }

            return userName;
        }

        private static IPrincipal BuildClaimsPrincipal(string currentUserPrincipal)
        {
            var unqualifiedUserName = GetUnqualifiedName(currentUserPrincipal).Trim();
            var genericIdentity = new GenericIdentity(unqualifiedUserName);
            return new GenericPrincipal(genericIdentity, new string [0]);
        }

        private static StringBuilder IsValid(Dictionary<string, StringBuilder> values, out StringBuilder environmentString, out Guid startActivityId, out StringBuilder currentuserprincipal)
        {
            values.TryGetValue("versionNumber", out StringBuilder versionNumber);
            if(versionNumber == null)
            {
                throw new InvalidDataContractException("no version Number passed");
            }

            values.TryGetValue("environment", out environmentString);
            if(environmentString == null)
            {
                throw new InvalidDataContractException("no environment passed");
            }

            values.TryGetValue("startActivityId", out StringBuilder startActivityIdString);
            if(startActivityIdString == null)
            {
                throw new InvalidDataContractException("no startActivityId passed.");
            }

            if(!Guid.TryParse(startActivityIdString.ToString(), out startActivityId))
            {
                throw new InvalidDataContractException("startActivityId is not a valid GUID.");
            }

            values.TryGetValue("currentuserprincipal", out currentuserprincipal);
            if(currentuserprincipal == null)
            {
                throw new InvalidDataContractException("no executing user principal passed");
            }

            return versionNumber;
        }
    }
}