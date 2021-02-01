#pragma warning disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;

namespace Dev2.Runtime.WebServer
{
    public class SwaggerBuilder
    {
        public IAuthorizationService AuthorizationService { get; set; }
        public IResourceCatalog ResourceCatalog { get; set; }

        public SwaggerBuilder(IAuthorizationService authorizationService, IResourceCatalog resourceCatalog)
        {
            if (authorizationService == null)
            {
                throw new ArgumentNullException("authorizationService");
            }

            if (resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }

            AuthorizationService = authorizationService;
            ResourceCatalog = resourceCatalog;
        }

        public List<IResponseWriter> BuildForPath(string path, bool isPublic, ICommunicationContext ctx)
        {
            List<IResponseWriter> responses = new List<IResponseWriter>();
            IList<IResource> resourceList;
            if (string.IsNullOrEmpty(path))
            {
                resourceList = ResourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID)
                    .Where(resource => resource.ResourceType == "WorkflowService").ToList();
            }
            else
            {
                var webPath = path.Replace("\\", "/");
                var searchPath = path.Replace("/", "\\");
                resourceList = ResourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(resource =>
                    resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).Contains(searchPath) &&
                    resource.ResourceType == "WorkflowService").ToList();
            }

            foreach (var resource in resourceList)
            {
                if (isPublic)
                {
                    var publicCanExecute = AuthorizationService.IsAuthorized(GlobalConstants.GenericPrincipal,
                        AuthorizationContext.Execute, resource);
                    var publicCanView = AuthorizationService.IsAuthorized(GlobalConstants.GenericPrincipal,
                        AuthorizationContext.View, resource);
                    if (publicCanExecute && publicCanView)
                    {
                        responses.Add(GetServiceResponse(resource, true, ctx));
                    }
                }
                else
                {
                    var canExecute = AuthorizationService.IsAuthorized(AuthorizationContext.Execute, resource);
                    var canView = AuthorizationService.IsAuthorized(AuthorizationContext.View, resource);
                    if (canView && canExecute)
                    {
                        responses.Add(GetServiceResponse(resource, false, ctx));
                    }
                }
            }

            return responses;
        }

        IResponseWriter GetServiceResponse(IResource resource, bool isPublic, ICommunicationContext ctx)
        {
            var requestTo = new WebRequestTO
            {
                ServiceName = resource.ResourceName + ".api",
                WebServerUrl = ctx.Request.Uri.ToString(),
                Dev2WebServer = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Authority}",
                IsUrlWithTokenPrefix = ctx.Request.IsTokenAuthentication
            };

            return null;//CreateForm(requestTo, ctx.GetServiceName(), ctx.GetWorkspaceID(), ctx.FetchHeaders(), ctx.Request.User);
        }
    }
}