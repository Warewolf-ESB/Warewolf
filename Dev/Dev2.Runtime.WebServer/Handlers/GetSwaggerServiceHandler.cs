#pragma warning disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class GetSwaggerServiceHandler : AbstractWebRequestHandler
    {
        static IAuthorizationService _authorizationService;
        static IResourceCatalog _resourceCatalog;

        public GetSwaggerServiceHandler()
            : this(ResourceCatalog.Instance, ServerAuthorizationService.Instance)
        {
        }

#pragma warning disable S3010 // Used by tests for constructor injection
        public GetSwaggerServiceHandler(IResourceCatalog catalog, IAuthorizationService auth)
            : base(ResourceCatalog.Instance, TestCatalog.Instance, TestCoverageCatalog.Instance, new DefaultEsbChannelFactory(), new SecuritySettings())
        {
            _resourceCatalog = catalog;
            _authorizationService = auth;
            
        }
#pragma warning restore S3010
      
        public override void ProcessRequest(ICommunicationContext ctx)
        {
            if(ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }
            var basePath = ctx.Request.BoundVariables["path"];
            if (!bool.TryParse(ctx.Request.BoundVariables["isPublic"], out bool isPublic))
            {
                isPublic = false;
            }
            var result = ResponseWriter(basePath, isPublic, ctx);
            
            //ctx.Send(result);
        }

        List<IResponseWriter> ResponseWriter(string basePath,bool isPublic, ICommunicationContext ctx)
        {
            var webPath = basePath.Replace("\\", "/");
            var searchPath = basePath.Replace("/", "\\");
            var resourceList = ResourceCatalog.Instance.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(resource =>
                resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).Contains(searchPath) &&
                resource.ResourceType == "WorkflowService").ToList();


            var apiBuilder = new SwaggerBuilder(_authorizationService, _resourceCatalog);
            return apiBuilder.BuildForPath(basePath, isPublic, ctx);
            
            var a = new AbstractWebRequestHandler.Executor(_workspaceRepository, _resourceCatalog, _testCatalog,
                _testCoverageCatalog, _authorizationService, _dataObjectFactory, _esbChannelFactory, _jwtManager);
            
            
            // var converter = new JsonSerializer();
            // var result = new StringBuilder();
            // var jsonTextWriter = new JsonTextWriter(new StringWriter(result)) { Formatting = Formatting.Indented };
            // converter.Serialize(jsonTextWriter, apis);
            // jsonTextWriter.Flush();
            // var apisJson = result.ToString();
            // var stringResponseWriter = new StringResponseWriter(apisJson, ContentTypes.Json,false);
            // return stringResponseWriter;
        }
        
        // private IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId,
        //     NameValueCollection headers, IPrincipal user)
        // {
        //     var a = new AbstractWebRequestHandler.Executor(_workspaceRepository, _resourceCatalog, _testCatalog,
        //         _testCoverageCatalog, _authorizationService, _dataObjectFactory, _esbChannelFactory, _jwtManager);
        //     var response = a.TryExecute(webRequest, serviceName, workspaceId, headers, user);
        //     return response ?? a.BuildResponse(webRequest, serviceName);
        // }
    }
}