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
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Interfaces;
using Warewolf.Data;
using Warewolf.Services;
using System.Linq;
using System.Collections.Generic;

namespace Dev2.Runtime.Services.ResourceCatalogImpl
{
    public class ResourceDependentProvider : IResourceDependentProvider
    {
        private IResourceCatalog _resourceCatalog;

        private IResourceCatalog ResourceCatalog
        {
            get => _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }

        public IWarewolfResource GetResourceDependentByResourceId(Guid resourceId)
        {
            return ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);         
            
        }
    }
}