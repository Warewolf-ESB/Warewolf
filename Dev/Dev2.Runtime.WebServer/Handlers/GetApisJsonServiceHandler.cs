using System;
using System.IO;
using System.Text;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class GetApisJsonServiceHandler : AbstractWebRequestHandler
    {
        static IAuthorizationService _authorizationService;
        static IResourceCatalog _resourceCatalog;

        public GetApisJsonServiceHandler()
            : this(ResourceCatalog.Instance, ServerAuthorizationService.Instance)
        {
        }

#pragma warning disable S3010 // Used by tests for constructor injection
        public GetApisJsonServiceHandler(IResourceCatalog catalog, IAuthorizationService auth)
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
            var result = GetApisJson(basePath,isPublic);
            
            ctx.Send(result);
        }

        static IResponseWriter GetApisJson(string basePath,bool isPublic)
        {
            var apiBuilder = new ApisJsonBuilder(_authorizationService, _resourceCatalog);
            var apis = apiBuilder.BuildForPath(basePath, isPublic);
            var converter = new JsonSerializer();
            var result = new StringBuilder();
            var jsonTextWriter = new JsonTextWriter(new StringWriter(result)) { Formatting = Formatting.Indented };
            converter.Serialize(jsonTextWriter, apis);
            jsonTextWriter.Flush();
            var apisJson = result.ToString();
            var stringResponseWriter = new StringResponseWriter(apisJson, ContentTypes.Json,false);
            return stringResponseWriter;
        }
    }
}