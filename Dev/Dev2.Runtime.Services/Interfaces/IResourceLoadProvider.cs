/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.DynamicServices.Objects.Base;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Interfaces
{
    // PBI 953 - 2013.05.16 - TWR - Created
    public interface IResourceLoadProvider
    {
        T GetResource<T>(Guid workspaceID, Guid serviceID) where T : Resource, new();
        T GetResource<T>(Guid workspaceID, string resourceName) where T : Resource, new();
        string GetResourcePath(Guid workspaceID,Guid id);
        IList<IResource> GetResourceList<T>(Guid workspaceId) where T : Resource, new();
        IList<Resource> GetResourceList(Guid workspaceId, Dictionary<string, string> filterParams);
        List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, string resourceName, bool useContains = false) where TServiceType : DynamicServiceObjectBase;
        List<DynamicServiceObjectBase> GetDynamicObjects(IResource resource);
        List<DynamicServiceObjectBase> GetDynamicObjects(IEnumerable<IResource> resources);
        List<Guid> GetDependants(Guid workspaceID, Guid? resourceId);
        List<ResourceForTree> GetDependentsAsResourceForTrees(Guid workspaceID, Guid resourceId);
        IList<IResource> GetResourceList(Guid workspaceId);
        int GetResourceCount(Guid workspaceID);
        IResource GetResource(Guid workspaceID, string resourceName, string resourceType = "Unknown", string version = null);
        IResource GetResource(Guid workspaceID, Guid resourceID);
        StringBuilder GetResourceContents(Guid workspaceID, Guid resourceID);
        StringBuilder GetResourceContents(IResource resource);
        List<IResource> GetResources(Guid workspaceID);
        IEnumerable GetModels(Guid workspaceID, enSourceType sourceType);
        List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, Guid resourceID) where TServiceType : DynamicServiceObjectBase;
        ConcurrentDictionary<Guid, ManagementServiceResource> ManagementServices { get; }
        ConcurrentDictionary<Guid, object> WorkspaceLocks { get; }
        List<DuplicateResource> DuplicateResources { get; set; }

        List<IResource> GetResourcesBasedOnType(string type, List<IResource> workspaceResources, Func<IResource, bool> func);
    }
}
