using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Dev2.Services.Security;

namespace Dev2.Runtime.WebServer
{
    public class ApisJsonBuilder
    {
        public IAuthorizationService AuthorizationService { get; set; }
        public IResourceCatalog ResourceCatalog { get; set; }

        public ApisJsonBuilder(IAuthorizationService authorizationService, IResourceCatalog resourceCatalog)
        {
            if(authorizationService == null)
            {
                throw new ArgumentNullException("authorizationService");
            }
            if(resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }
            AuthorizationService = authorizationService;
            ResourceCatalog = resourceCatalog;
        }

        public ApisJson BuildForPath(string path)
        {
            var apiJson = new ApisJson
            {
                Name = Environment.MachineName,
                Description = "",
                Created = DateTime.Today.Date,
                Modified = DateTime.Today.Date,
                SpecificationVersion = "0.15",
                Apis = new List<SingleApi>()
            };
            IList<IResource> resourceList;
            if(string.IsNullOrEmpty(path))
            {
                apiJson.Url = EnvironmentVariables.WebServerUri + "apis.json";
                resourceList = ResourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(resource => resource.ResourceType==ResourceType.WorkflowService).ToList();
            }
            else
            {
                var webPath = path.Replace("\\", "/");
                var searchPath = path.Replace("/", "\\");
                apiJson.Url = EnvironmentVariables.WebServerUri + webPath + "/apis.json";
                resourceList = ResourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(resource => resource.ResourcePath.Contains(searchPath) && resource.ResourceType == ResourceType.WorkflowService).ToList();
            }
            foreach(var resource in resourceList)
            {
                var canExecute = AuthorizationService.IsAuthorized(AuthorizationContext.Execute, resource.ResourceID.ToString());
                var canView = AuthorizationService.IsAuthorized(AuthorizationContext.View, resource.ResourceID.ToString());
                if(canView&&canExecute)
                {
                    apiJson.Apis.Add(CreateSingleApiForResource(resource,false));
                }

                var publicCanExecute = AuthorizationService.IsAuthorized(GlobalConstants.GenericPrincipal,AuthorizationContext.Execute, resource.ResourceID.ToString());
                var publicCanView = AuthorizationService.IsAuthorized(GlobalConstants.GenericPrincipal,AuthorizationContext.View, resource.ResourceID.ToString());
                if (publicCanExecute && publicCanView)
                {
                    apiJson.Apis.Add(CreateSingleApiForResource(resource, true));
                }
            }
            return apiJson;
        }

        SingleApi CreateSingleApiForResource(IResource resource,bool isPublic)
        {

            var webPath = resource.ResourcePath.Replace("\\","/");
            var accessPath = isPublic?"public/":"secure/";
            var singleApi = new SingleApi
            {
                Name = resource.ResourceName,
                BaseUrl = EnvironmentVariables.WebServerUri + accessPath + webPath + ".json",
                Properties = new List<PropertyApi>()
            };
            var propertyApi = new PropertyApi
            {
                Type = "Swagger",
                Value = EnvironmentVariables.WebServerUri + accessPath + webPath + ".api"
            };
            singleApi.Properties.Add(propertyApi);
            return singleApi;
        }
    }
}