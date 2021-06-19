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
using System.Collections.Specialized;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Decision;
using Dev2.InterfaceImplementors;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Web;
using Dev2.Workspaces;
using Warewolf.Auditing;
using Warewolf.Data;
using Warewolf.Execution;
using Warewolf.Security;

namespace Dev2.Runtime.WebServer.Executor
{
    public abstract class ExecutorBase
    {
            protected string _executePayload;
            protected Guid _workspaceGuid;
            protected Guid _executionDataListId;
            protected IDSFDataObject _dataObject;
            protected IWarewolfResource _resource;
            protected Dev2JsonSerializer _serializer;
            protected bool _canExecute;
            protected EsbExecuteRequest _esbExecuteRequest;
            readonly IAuthorizationService _authorizationService;
            readonly IDataObjectFactory _dataObjectFactory;
            readonly IResourceCatalog _resourceCatalog;
            readonly IWorkspaceRepository _repository;
            readonly ITestCatalog _testCatalog;
            readonly ITestCoverageCatalog _testCoverageCatalog;
            IServiceTestExecutor _serviceTestExecutor;
            readonly IEsbChannelFactory _esbChannelFactory;
            protected readonly IJwtManager _jwtManager;

            protected ExecutorBase(IWorkspaceRepository workspaceRepository, IResourceCatalog resourceCatalog, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory, IEsbChannelFactory esbChannelFactory, IJwtManager jwtManager)
                : this(workspaceRepository, resourceCatalog, TestCatalog.Instance, TestCoverageCatalog.Instance, null, authorizationService, dataObjectFactory, esbChannelFactory, jwtManager)
            {
            }

            protected ExecutorBase(IWorkspaceRepository workspaceRepository, IResourceCatalog resourceCatalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog, IServiceTestExecutor serviceTestExecutor, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory, IEsbChannelFactory esbChannelFactory, IJwtManager jwtManager)
            {
                _repository = workspaceRepository;
                _resourceCatalog = resourceCatalog;
                _testCatalog = testCatalog;
                _testCoverageCatalog = testCoverageCatalog;
                _authorizationService = authorizationService;
                _dataObjectFactory = dataObjectFactory;
                _esbChannelFactory = esbChannelFactory;
                _jwtManager = jwtManager;
                _serviceTestExecutor = serviceTestExecutor;
            }

            Guid EnsureWorkspaceIdValid(string workspaceId)
            {
                if (workspaceId is null)
                {
                    return _repository.ServerWorkspace.ID;
                }

                return !Guid.TryParse(workspaceId, out var workspaceGuid)
                    ? _repository.ServerWorkspace.ID
                    : workspaceGuid;
            }

            void PrepareDataObject(WebRequestTO webRequest, string serviceName, NameValueCollection headers, IPrincipal user, Guid workspaceGuid, out IWarewolfResource resource)
            {
                var uri = string.IsNullOrWhiteSpace(webRequest.WebServerUrl) ? new Uri("https://test/") : new Uri(webRequest.WebServerUrl);
                _dataObject = _dataObjectFactory.New(workspaceGuid, user, serviceName, webRequest);
                _dataObject.OriginalServiceName = serviceName;
                _dataObject.SetHeaders(headers);
                _dataObject.SetupForWebDebug(webRequest);
                webRequest.BindRequestVariablesToDataObject(ref _dataObject);
                _dataObject.SetupForRemoteInvoke(headers);
                _dataObject.SetEmissionType(uri, serviceName, headers);
                _dataObject.SetupForTestExecution(serviceName, headers);
                if (_dataObject.ServiceName == null)
                {
                    _dataObject.ServiceName = serviceName;
                }

                _dataObject.SetResourceNameAndId(_resourceCatalog, serviceName, out resource);
                _dataObject.SetTestResourceIds(_resourceCatalog.NewContextualResourceCatalog(_authorizationService, workspaceGuid), webRequest, serviceName, resource);
                _dataObject.WebUrl = webRequest.WebServerUrl;
                _dataObject.EsbChannel = _esbChannelFactory.New();

                if (_dataObject.Settings is null)
                {
                    _dataObject.Settings = new Dev2WorkflowSettingsTO
                    {
                        ExecutionLogLevel =  Config.Server.ExecutionLogLevel,
                        EnableDetailedLogging = Config.Server.EnableDetailedLogging,
                        LoggerType = LoggerType.JSON,
                        KeepLogsForDays = 2,
                        CompressOldLogFiles = true
                    };
                }

                var stateNotifier = CustomContainer.Get<IStateNotifierFactory>()?.New(_dataObject);
                _dataObject.StateNotifier = stateNotifier;
            }

            internal IResponseWriter TryExecute(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user)
            {
                _workspaceGuid = EnsureWorkspaceIdValid(workspaceId);
                _serializer = new Dev2JsonSerializer();

                PrepareDataObject(webRequest, serviceName, headers, user, _workspaceGuid, out _resource);
                var isTestCoverage = _dataObject.ReturnType == EmitionTypes.Cover || _dataObject.ReturnType == EmitionTypes.CoverJson;
                var isTestRun = (_dataObject.ReturnType == EmitionTypes.TEST ||
                                 _dataObject.ReturnType == EmitionTypes.TRX) && _dataObject.TestName == "*";

                if (isTestRun)
                {
                    if (_serviceTestExecutor == null)
                    {
                        _serviceTestExecutor = new ServiceTestExecutor(serviceName, user, _workspaceGuid, _serializer, _dataObject);
                    }
                    return ExecuteAsTest(user);
                }

                if (isTestCoverage)
                {
                    return ExecuteAsCoverage(webRequest, serviceName, _resource);
                }

                if (_resource is null)
                {
                    var msg = string.Format(Warewolf.Resource.Errors.ErrorResource.ServiceNotFound, serviceName);
                    _dataObject.Environment.AddError(msg);
                    _dataObject.ExecutionException = new Exception(msg);
                    _executionDataListId = GlobalConstants.NullDataListID;
                    return null;
                }

                var workflowCanBeExecutedByGroup = _dataObject.CanExecuteCurrentResource(_resource, _authorizationService);
                _canExecute = workflowCanBeExecutedByGroup;

                _executionDataListId = GlobalConstants.NullDataListID;
                if (!_canExecute)
                {
                    var message = webRequest.IsUrlWithTokenPrefix
                        ? Warewolf.Resource.Errors.ErrorResource.TokenNotAuthorizedToExecuteOuterWorkflowException
                        : Warewolf.Resource.Errors.ErrorResource.UserNotAuthorizedToExecuteOuterWorkflowException;

                    var errorMessage = string.Format(message, _dataObject.ExecutingUser?.Identity.Name, _dataObject.ServiceName);
                    _dataObject.Environment.AddError(errorMessage);
                    _dataObject.ExecutionException = new AccessDeniedException(errorMessage);
                } else
                {
                    if (_dataObject.ReturnType != EmitionTypes.OPENAPI)
                    {
                        Thread.CurrentPrincipal = user;
                        _executionDataListId = DoExecution(webRequest, serviceName, _workspaceGuid, _dataObject, user);
                    }
                }

                return null;
            }

            private Guid DoExecution(WebRequestTO webRequest, string serviceName, Guid workspaceGuid, IDSFDataObject dataObject, IPrincipal userPrinciple)
            {
                _esbExecuteRequest = CreateEsbExecuteRequestFromWebRequest(webRequest, serviceName);
                var executionDataListId = Guid.Empty;

                Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
                {

                    executionDataListId = dataObject.EsbChannel.ExecuteRequest(dataObject, _esbExecuteRequest, workspaceGuid, out _);
                    _executePayload = _esbExecuteRequest.ExecuteResult.ToString();
                });

                return executionDataListId;
            }

            IResponseWriter ExecuteAsTest(IPrincipal userPrinciple)
            {
                var formatter = _serviceTestExecutor.ExecuteTests(_dataObject, userPrinciple, _workspaceGuid, _serializer, _testCatalog, _resourceCatalog, out _executePayload, _testCoverageCatalog);
                return new StringResponseWriter(_executePayload ?? string.Empty, formatter.ContentType);
            }

            private IResponseWriter ExecuteAsCoverage(WebRequestTO webRequest, string serviceName, IWarewolfResource resource)
            {
                try
                {
                    var coverageDataContext = new CoverageDataContext(_dataObject.ResourceID, _dataObject.ReturnType, webRequest.WebServerUrl);
                    coverageDataContext.SetTestCoverageResourceIds(_resourceCatalog.NewContextualResourceCatalog(_authorizationService, _workspaceGuid), webRequest, serviceName, resource);
                    var formatter = ServiceTestCoverageExecutor.GetTestCoverageReports(coverageDataContext, _workspaceGuid, _serializer, _testCoverageCatalog, _testCatalog, _resourceCatalog, out _executePayload);
                    return new StringResponseWriter(_executePayload ?? string.Empty, formatter.ContentType);
                }
                finally
                {
                    Dev2DataListDecisionHandler.Instance.RemoveEnvironment(_dataObject.DataListID);
                    _dataObject.Environment = null;
                }
            }

            public abstract IResponseWriter BuildResponse(WebRequestTO webRequest, string serviceName);

            private static EsbExecuteRequest CreateEsbExecuteRequestFromWebRequest(WebRequestTO webRequest, string serviceName)
            {
                var esbExecuteRequest = new EsbExecuteRequest
                {
                    ServiceName = serviceName,
                };
                foreach (string key in webRequest.Variables)
                {
                    esbExecuteRequest.AddArgument(key, new StringBuilder(webRequest.Variables[key]));
                }

                return esbExecuteRequest;
            }
    }
}
