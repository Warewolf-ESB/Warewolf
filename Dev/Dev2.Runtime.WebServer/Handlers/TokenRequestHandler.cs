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
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ResourceCatalogImpl;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Web;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Warewolf;
using Warewolf.Security;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class TokenRequestHandler : AbstractWebRequestHandler
    {
        public TokenRequestHandler()
            : this(ResourceCatalog.Instance)
        {
        }

        private TokenRequestHandler(IResourceCatalog resourceCatalog)
            : this(resourceCatalog, WorkspaceRepository.Instance, ServerAuthorizationService.Instance, new DataObjectFactory(), new DefaultEsbChannelFactory(), new SecuritySettings())
        {
        }

        protected TokenRequestHandler(IResourceCatalog resourceCatalog, IWorkspaceRepository workspaceRepository, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory, IEsbChannelFactory esbChannelFactory, ISecuritySettings securitySettings)
            : this(resourceCatalog, workspaceRepository, authorizationService, dataObjectFactory, esbChannelFactory, securitySettings, new JwtManager(securitySettings))
        {
        }

        private TokenRequestHandler(IResourceCatalog resourceCatalog, IWorkspaceRepository workspaceRepository, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory, IEsbChannelFactory esbChannelFactory, ISecuritySettings securitySettings, IJwtManager jwtManager)
            : base(resourceCatalog, TestCatalog.Instance, TestCoverageCatalog.Instance, workspaceRepository, authorizationService, dataObjectFactory, esbChannelFactory, securitySettings, jwtManager)
        {
        }


        public override void ProcessRequest(ICommunicationContext ctx)
        {
            LoadSecuritySettings();

            var serviceName = ctx.GetServiceName();
            var instanceId = ctx.GetInstanceID();
            var bookmark = ctx.GetBookmark();
            var workspaceId = ctx.GetWorkspaceID();

            var requestTo = new WebRequestTO
            {
                ServiceName = serviceName,
                InstanceID = instanceId,
                Bookmark = bookmark,
                WebServerUrl = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Authority}/public/{OverrideResource.Name}",
                Dev2WebServer = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Authority}"
            };

            var data = new SubmittedData().GetPostData(ctx);
            if (!string.IsNullOrEmpty(data))
            {
                requestTo.RawRequestPayload = data;
            }

            var variables = ctx.Request.BoundVariables;
            if (variables != null)
            {
                foreach (string key in variables)
                {
                    requestTo.Variables.Add(key, variables[key]);
                }
            }

            Thread.CurrentPrincipal = ctx.Request.User;
            try
            {
                var response = ExecuteWorkflow(requestTo, $"{OverrideResource.Name}.json", workspaceId,
                    ctx.FetchHeaders(), ctx.Request.User);
                ctx.Send(response);
            }
            catch (Exception e)
            {
                Dev2Logger.Warn($"failed processing login request: {e.Message}", GlobalConstants.WarewolfWarn);
                throw;
            }
        }

        private static INamedGuid OverrideResource { get; set; }

        private void LoadSecuritySettings()
        {
            var securitySettings = _securitySettings.ReadSettingsFile(new ResourceNameProvider());
            OverrideResource = securitySettings.AuthenticationOverrideWorkflow;
        }

        protected IResponseWriter ExecuteWorkflow(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user)
        {
            if (!serviceName.Contains(".json"))
            {
                serviceName += ".json";
            }

            var a = new Executor(_workspaceRepository, _resourceCatalog, _authorizationService, _dataObjectFactory, _esbChannelFactory, _jwtManager);
            a.TryExecute(webRequest, serviceName, workspaceId, headers, user);
            var response = a.BuildResponse(webRequest, serviceName);
            return response;
        }


        class Executor : ExecutorBase
        {
            public Executor(IWorkspaceRepository workspaceRepository, IResourceCatalog resourceCatalog, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory, IEsbChannelFactory esbChannelFactory, IJwtManager jwtManager)
                : base(workspaceRepository, resourceCatalog, authorizationService, dataObjectFactory, esbChannelFactory, jwtManager)
            {
            }

            public override IResponseWriter BuildResponse(WebRequestTO webRequest, string serviceName)
            {
                var formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                var executionDto = new ExecutionDto
                {
                    WebRequestTO = webRequest,
                    ServiceName = serviceName,
                    DataObject = _dataObject,
                    DataListIdGuid = _executionDataListId,
                    WorkspaceID = _workspaceGuid,
                    Resource = _resource,
                    DataListFormat = formatter,
                    PayLoad = _executePayload ?? string.Empty,
                    Serializer = _serializer,
                };
                var allErrors = new ErrorResultTO();

                var currentErrors = executionDto.DataObject.Environment?.Errors?.Union(executionDto.DataObject.Environment?.AllErrors);
                if (currentErrors != null)
                {
                    foreach (var error in currentErrors)
                    {
                        if (error.Length > 0)
                        {
                            allErrors.AddError(error, true);
                        }
                    }
                }

                executionDto.Request = _esbExecuteRequest;
                executionDto.ErrorResultTO = allErrors;

                if (executionDto.DataObject.ExecutionException is AccessDeniedException)
                {
                    throw executionDto.DataObject.ExecutionException;
                }

                var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);
                var resp = executionDtoExtensions.CreateResponse();
                var content = resp.Content;
                if (!content.Contains("UserGroups"))
                {
                    ThrowInternalServerError();
                }

                var json = JsonConvert.DeserializeObject<UserGroupsResponse>(resp.Content);
                var userGroups = json?.UserGroups.ToList();
                bool hasInvalidOutputs = false;
                hasInvalidOutputs = userGroups.Count == 0;
                foreach (var o in (userGroups))
                {
                    if (string.IsNullOrEmpty(o.Name) || string.IsNullOrWhiteSpace(o.Name))
                    {
                        hasInvalidOutputs = true;
                        break;
                    }
                }

                return hasInvalidOutputs
                    ? ThrowInternalServerError()
                    : CreateEncryptedResponse(resp.Content);
            }

            private static IResponseWriter ThrowInternalServerError()
            {
                Dev2Logger.Warn("invalid login override workflow selected: outputs not valid", GlobalConstants.WarewolfWarn);

                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    StatusCode = HttpStatusCode.InternalServerError
                };
                throw new HttpResponseException(response);
            }

            public class UserGroup
            {
                public string Name { get; set; }
            }

            public class UserGroupsResponse
            {
                public UserGroup[] UserGroups { get; set; }
            }

            private IResponseWriter CreateEncryptedResponse(string payload)
            {
                var rs = new StringResponseWriterFactory();
                if (payload.Length > 0)
                {
                    var encryptedPayload = _jwtManager.GenerateToken(payload);
                    encryptedPayload = "{\"token\": \"" + encryptedPayload + "\"}";
                    return rs.New(encryptedPayload, "application/json");
                }

                return ThrowInternalServerError();
            }
        }
    }
}