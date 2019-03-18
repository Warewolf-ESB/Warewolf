#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.Interfaces;
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

        public ApisJson BuildForPath(string path, bool isPublic)
        {
            var apiJson = new ApisJson
            {
                Name = EnvironmentVariables.PublicWebServerUri,
                Description = "",
                Created = DateTime.Today.Date,
                Modified = DateTime.Today.Date,
                SpecificationVersion = "0.15",
                Apis = new List<SingleApi>()
            };
            IList<IResource> resourceList;
            if(string.IsNullOrEmpty(path))
            {
                apiJson.Url = EnvironmentVariables.PublicWebServerUri + "apis.json";
                resourceList = ResourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(resource => resource.ResourceType=="WorkflowService").ToList();
            }
            else
            {
                var webPath = path.Replace("\\", "/");
                var searchPath = path.Replace("/", "\\");
                apiJson.Url = EnvironmentVariables.PublicWebServerUri + webPath + "/apis.json";
                resourceList = ResourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(resource => resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).Contains(searchPath) && resource.ResourceType == "WorkflowService").ToList();
            }
            foreach(var resource in resourceList)
            {
                if (isPublic)
                {
                    var publicCanExecute = AuthorizationService.IsAuthorized(GlobalConstants.GenericPrincipal, AuthorizationContext.Execute, resource.ResourceID.ToString());
                    var publicCanView = AuthorizationService.IsAuthorized(GlobalConstants.GenericPrincipal, AuthorizationContext.View, resource.ResourceID.ToString());
                    if (publicCanExecute && publicCanView)
                    {
                        apiJson.Apis.Add(CreateSingleApiForResource(resource, true));
                    }
                }
                else
                {
                    var canExecute = AuthorizationService.IsAuthorized(AuthorizationContext.Execute, resource.ResourceID.ToString());
                    var canView = AuthorizationService.IsAuthorized(AuthorizationContext.View, resource.ResourceID.ToString());
                    if (canView && canExecute)
                    {
                        apiJson.Apis.Add(CreateSingleApiForResource(resource, false));
                    }
                }
               
            }
            return apiJson;
        }

        SingleApi CreateSingleApiForResource(IResource resource,bool isPublic)
        {

            var webPath = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).Replace("\\","/");
            var accessPath = isPublic?"public/":"secure/";
            var singleApi = new SingleApi
            {
                Name = resource.ResourceName,
                BaseUrl = EnvironmentVariables.PublicWebServerUri + accessPath + webPath + ".json",
                Properties = new List<PropertyApi>()
            };
            var propertyApi = new PropertyApi
            {
                Type = "Swagger",
                Value = EnvironmentVariables.PublicWebServerUri + accessPath + webPath + ".api"
            };
            singleApi.Properties.Add(propertyApi);
            return singleApi;
        }
    }
}