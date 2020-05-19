/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
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
using Warewolf;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class TokenRequestHandler : AbstractWebRequestHandler
    {
        public TokenRequestHandler()
            : this(ResourceCatalog.Instance)
        {
        }

        public TokenRequestHandler(IResourceCatalog resourceCatalog)
            : this(resourceCatalog, WorkspaceRepository.Instance, ServerAuthorizationService.Instance, new DataObjectFactory())
        {
        }

        protected TokenRequestHandler(IResourceCatalog resourceCatalog, IWorkspaceRepository workspaceRepository, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory)
            : base(resourceCatalog, workspaceRepository, authorizationService, dataObjectFactory)
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

            var data = SubmittedData.GetPostData(ctx);
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
            var response = ExecuteWorkflow(requestTo, OverrideResource.Name, workspaceId, ctx.FetchHeaders(), ctx.Request.User);
            ctx.Send(response);
        }


        private static List<WindowsGroupPermission> WindowsPermissions { get; set; }

        private static INamedGuid OverrideResource { get; set; }
        private static string SecretKey { get; set; }

        protected static void LoadSecuritySettings()
        {
            var securitySettings = SecuritySettings.ReadSettingsFile(new ResourceNameProvider());
            OverrideResource = securitySettings.AuthenticationOverrideWorkflow;
            SecretKey = securitySettings.SecretKey;
            WindowsPermissions = securitySettings.WindowsGroupPermissions;
        }

        protected IResponseWriter ExecuteWorkflow(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user)
        {
            var a = new Executor(_workspaceRepository, _resourceCatalog, _authorizationService, _dataObjectFactory);
            var isValid = a.TryExecute(webRequest, serviceName, workspaceId, headers, user);
            if (isValid != null)
            {
                return new ExceptionResponseWriter(System.Net.HttpStatusCode.InternalServerError, "");
            }

            var response = a.BuildResponse(webRequest, serviceName);
            return response;
        }


        class Executor : ExecutorBase
        {
            public Executor(IWorkspaceRepository workspaceRepository, IResourceCatalog resourceCatalog, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory)
                : base(workspaceRepository, resourceCatalog, authorizationService, dataObjectFactory)
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
                return CreateEncryptedResponse(DefaultExecutionResponse(executionDto));
            }

            private string DefaultExecutionResponse(ExecutionDto executionDto)
            {
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

                var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);
                return executionDtoExtensions.CreatePayloadResponse();
            }

            private static IResponseWriter CreateEncryptedResponse(string payload)
            {
                var rs = new StringResponseWriterFactory();
                if (payload.Length > 0)
                {
                    var encryptedPayload = JwtManager.GenerateToken(payload);
                    encryptedPayload = "{\"token\": \"" + encryptedPayload + "\"}";
                    return rs.New(encryptedPayload, "application/json");
                }

                throw new HttpException(500, "internal server error");
            }
        }
    }
}