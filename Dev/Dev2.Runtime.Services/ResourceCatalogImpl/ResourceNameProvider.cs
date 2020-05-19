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
using Dev2.Common;
using Dev2.Runtime.Interfaces;
using Warewolf.Services;

namespace Dev2.Runtime.ResourceCatalogImpl
{
    public class ResourceNameProvider : IResourceNameProvider
    {
        private IResourceCatalog _resourceCatalog;

        private IResourceCatalog ResourceCatalog
        {
            get => _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }

        public string GetResourceNameById(Guid resourceId)
        {
            var resource = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
            return resource.ResourceName;
        }
    }
}