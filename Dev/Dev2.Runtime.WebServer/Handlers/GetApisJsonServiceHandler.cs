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