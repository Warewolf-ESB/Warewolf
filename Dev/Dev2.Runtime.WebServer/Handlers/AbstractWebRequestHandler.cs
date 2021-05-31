/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Executor;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Warewolf.Execution;
using Warewolf.Security;

namespace Dev2.Runtime.WebServer.Handlers
{
    public abstract class AbstractWebRequestHandler : IRequestHandler
    {
        string _location;
        protected readonly IResourceCatalog _resourceCatalog;
        protected readonly ITestCatalog _testCatalog;
        protected readonly ITestCoverageCatalog _testCoverageCatalog;
        protected readonly IServiceTestExecutor _serviceTestExecutor;
        protected readonly ISecuritySettings _securitySettings;
        protected readonly IAuthorizationService _authorizationService;
        protected readonly IWorkspaceRepository _workspaceRepository;
        protected readonly IDataObjectFactory _dataObjectFactory;
        protected readonly IEsbChannelFactory _esbChannelFactory;
        protected readonly IJwtManager _jwtManager;
        public string Location => _location ?? (_location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        public abstract void ProcessRequest(ICommunicationContext ctx);

        protected AbstractWebRequestHandler(IResourceCatalog resourceCatalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog, IEsbChannelFactory esbChannelFactory, ISecuritySettings securitySettings)
            : this(resourceCatalog, testCatalog, testCoverageCatalog, null, WorkspaceRepository.Instance, ServerAuthorizationService.Instance, new DataObjectFactory(), esbChannelFactory, securitySettings, new JwtManager(securitySettings))
        {
        }
        protected AbstractWebRequestHandler(IResourceCatalog resourceCatalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog, IServiceTestExecutor serviceTestExecutor, IWorkspaceRepository workspaceRepository, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory, IEsbChannelFactory esbChannelFactory, ISecuritySettings securitySettings)
            : this(resourceCatalog, testCatalog, testCoverageCatalog, serviceTestExecutor, workspaceRepository, authorizationService, dataObjectFactory, esbChannelFactory, securitySettings, new JwtManager(securitySettings))
        {
        }

        protected AbstractWebRequestHandler(IResourceCatalog resourceCatalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog, IServiceTestExecutor serviceTestExecutor, IWorkspaceRepository workspaceRepository, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory, IEsbChannelFactory esbChannelFactory, ISecuritySettings securitySettings, IJwtManager jwtManager)
        {
            _resourceCatalog = resourceCatalog;
            _testCatalog = testCatalog;
            _testCoverageCatalog = testCoverageCatalog;
            _serviceTestExecutor = serviceTestExecutor;
            _workspaceRepository = workspaceRepository;
            _authorizationService = authorizationService;
            _dataObjectFactory = dataObjectFactory;
            _esbChannelFactory = esbChannelFactory;
            _securitySettings = securitySettings;
            _jwtManager = jwtManager;
        }


#pragma warning disable CC0044
        protected IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers) => CreateForm(webRequest, serviceName, workspaceId, headers, null);
#pragma warning restore CC0044

        protected IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user)
        {
            var a = new Executor.Executor(_workspaceRepository, _resourceCatalog, _testCatalog, _testCoverageCatalog, _serviceTestExecutor, _authorizationService, _dataObjectFactory, _esbChannelFactory, _jwtManager);
            var response = a.TryExecute(webRequest, serviceName, workspaceId, headers, user);
            return response ?? a.BuildResponse(webRequest, serviceName);
        }
    }
}