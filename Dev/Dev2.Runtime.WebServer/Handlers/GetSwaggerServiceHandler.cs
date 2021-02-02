#pragma warning disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
            : base(ResourceCatalog.Instance, TestCatalog.Instance, TestCoverageCatalog.Instance,
                new DefaultEsbChannelFactory(), new SecuritySettings())
        {
            _resourceCatalog = catalog;
            _authorizationService = auth;
        }
#pragma warning restore S3010

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }

            var basePath = ctx.Request.BoundVariables["path"];
            if (!bool.TryParse(ctx.Request.BoundVariables["isPublic"], out bool isPublic))
            {
                isPublic = false;
            }

            WriteReponses(basePath, isPublic, ctx);
        }

        void WriteReponses(string basePath, bool isPublic, ICommunicationContext ctx)
        {
            var webPath = basePath.Replace("\\", "/");
            var searchPath = basePath.Replace("/", "\\").Replace(".api", "");
            var resourceList = ResourceCatalog.Instance.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(
                resource =>
                    resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).Contains(searchPath) &&
                    resource.ResourceType == "WorkflowService").ToList();

            var builder = new StringBuilder();
            foreach (var resource in resourceList)
            {
                var resourceName = $"{resource.ResourceName}.api";
                var resourceBasePath = basePath;
                
                if (basePath.EndsWith(resourceName))
                    resourceBasePath = resourceBasePath.Replace(resourceName, "").TrimEnd('/');
                
                var requestVariables = new NameValueCollection
                {
                    {"servicename", $"{resourceBasePath.Replace(".api", "")}/{resource.ResourceName}.api"},
                    {"isPublic", isPublic.ToString()}
                };

                var filePath1 = ctx.Request.Uri.ToString();
                filePath1 = filePath1.Substring(0, filePath1.IndexOf("secure", StringComparison.Ordinal) + 6);

                var filePath2 = resource.FilePath;
                filePath2 = filePath2.Substring(filePath2.IndexOf("Resources", StringComparison.Ordinal) + 10)
                    .Replace(".bite", ".api");

                var uri = new Uri($"{filePath1}/{filePath2}");
                var req = new HttpRequestMessage(HttpMethod.Get, uri) {Content = ctx.Request.Request.Content};
                var context = new WebServerContext(req, requestVariables) {Request = {User = ctx.Request.User}};

                var request = new WebGetRequestHandler(_resourceCatalog, _testCatalog, _testCoverageCatalog);
                request.ProcessRequest(context);
                Task.Run(async () =>
                {
                    var val = await context.Response.Response.Content.ReadAsStringAsync();
                    builder.AppendLine(val);
                }).Wait();
            }

            ctx.Send(new StringResponseWriter(builder.ToString(), "application/json"));
        }
    }
}