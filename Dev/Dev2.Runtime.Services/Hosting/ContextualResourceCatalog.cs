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
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Warewolf.Data;
using Warewolf.Services;

namespace Dev2.Runtime.Hosting
{
    public class ContextualResourceCatalog : IContextualResourceCatalog
    {
        private readonly IResourceCatalog _resourceCatalog;
        private readonly IAuthorizationService _authService;
        private readonly Guid _serverWorkspaceId;

        public ContextualResourceCatalog(IResourceCatalog resourceCatalog, IAuthorizationService authService, Guid serverWorkspaceId)
        {
            _resourceCatalog = resourceCatalog;
            _authService = authService;
            _serverWorkspaceId = serverWorkspaceId;
        }

        private IEnumerable<IWarewolfResource> GetResources(string path)
        {
            return _resourceCatalog.GetResources(_serverWorkspaceId)
                                   .Where(resource => resource.IsInPath(_serverWorkspaceId, path));
        }

        public IEnumerable<IWarewolfResource> GetExecutableResources(string path)
        {
            return GetResources(path)
                .Where(resource => _authService.IsAuthorized(AuthorizationContext.Execute, resource));
        }
    }

    public static class ResourceExtensionMethods
    {
        public static bool IsInPath(this IResource resource, Guid serverWorkspaceId, string path)
        {
            if (path == "/")
            {
                return true;
            }
            return resource.GetResourcePath(serverWorkspaceId).StartsWith(path);
        }
    }
}