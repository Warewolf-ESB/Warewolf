#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
                    var publicCanExecute = AuthorizationService.IsAuthorized(GlobalConstants.GenericPrincipal, AuthorizationContext.Execute, resource);
                    var publicCanView = AuthorizationService.IsAuthorized(GlobalConstants.GenericPrincipal, AuthorizationContext.View, resource);
                    if (publicCanExecute && publicCanView)
                    {
                        apiJson.Apis.Add(CreateSingleApiForResource(resource, true));
                    }
                }
                else
                {
                    var canExecute = AuthorizationService.IsAuthorized(AuthorizationContext.Execute, resource);
                    var canView = AuthorizationService.IsAuthorized(AuthorizationContext.View, resource);
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