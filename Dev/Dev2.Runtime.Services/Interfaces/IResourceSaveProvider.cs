/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

#pragma warning disable

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceSaveProvider
    {
        ResourceCatalogResult SaveResources(Guid serverWorkspaceID, List<DuplicateResourceTO> resourceAndContents, bool overrideExisting);
        ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string savedPath);
        ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string savedPath, string reason);
        ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string savedPath, string reason, string user);
        ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, string savedPath);
        ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, string savedPath, string reason, string user);
        Action<IResource> ResourceSaved { get; set; }
        Action<Guid, IList<ICompileMessageTO>> SendResourceMessages { get; set; }
        ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath);
        ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath, string reason, string user);
        string SetResourceFilePath(Guid workspaceID, IResource resource, ref string savedPath);
        ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting);
        ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting, string savedPath);
        ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting, string savedPath, string reason);
    }
}