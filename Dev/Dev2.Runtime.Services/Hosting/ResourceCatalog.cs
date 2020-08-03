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
using System.Activities;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.DynamicServices.Objects.Base;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.ResourceManagement;
using Dev2.Common.Interfaces.Wrappers;
using Warewolf;
using Dev2.Services.Security;
using Warewolf.Data;
using Warewolf.Services;

namespace Dev2.Runtime.Hosting
{
    public class ResourceCatalog : IResourceCatalog, IDisposable
    {
        readonly object _loadLock = new object();
        static readonly object _lazyLock = new object();
        ResourceCatalogBuilder Builder { get; set; }

        ResourceCatalogPluginContainer _catalogPluginContainer;
        static readonly Lazy<IResourceCatalog> _instance = new Lazy<IResourceCatalog>(() =>
        {
            lock (_lazyLock)
            {
                var resourceCatalog = CustomContainer.Get<IResourceCatalog>();
                if (resourceCatalog is null)
                {
                    resourceCatalog = new ResourceCatalog(EsbManagementServiceLocator.GetServices());
                    CustomContainer.Register<IResourceCatalog>(resourceCatalog);
                }
                return resourceCatalog;
            }

        }, LazyThreadSafetyMode.PublicationOnly);

        public static IResourceCatalog Instance => _instance.Value;

        public ResourceCatalog()
            : this(null)
        {
        }

        public ResourceCatalog(IEnumerable<DynamicService> managementServices)
        {
        }
        public void Initialize(IEnumerable<DynamicService> managementServices)
        {
            InitializeWorkspaceResources();
            _serverVersionRepository = new ServerVersionRepository(new VersionStrategy(), this, _directoryWrapper, EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper(), new FilePathWrapper());
            _catalogPluginContainer = new ResourceCatalogPluginContainer(_serverVersionRepository, WorkspaceResources, managementServices);
            _catalogPluginContainer.Build(this);
        }

        IServerVersionRepository _serverVersionRepository;
        public IContextualResourceCatalog NewContextualResourceCatalog(IAuthorizationService authService, Guid workspaceId)
        {
            var catalog = new ContextualResourceCatalog(this, authService, workspaceId);
            return catalog;
        }

        public IWarewolfWorkflow GetWorkflow(Guid workflowId)
        {
            var workflowContents = GetResourceContents(GlobalConstants.ServerWorkspaceID, workflowId);
            var result = new Workflow(workflowContents.ToXElement());
            return result;
        }

        readonly IDirectory _directoryWrapper = new DirectoryWrapper();
        public void CleanUpOldVersionControlStructure()
        {
            _serverVersionRepository.CleanUpOldVersionControlStructure(_directoryWrapper);
        }

        [ExcludeFromCodeCoverage]//used by tests for constructor injection
        public ResourceCatalog(IEnumerable<DynamicService> managementServices, IServerVersionRepository serverVersionRepository)
        {
            InitializeWorkspaceResources();
            var versioningRepository = serverVersionRepository;
            _catalogPluginContainer = new ResourceCatalogPluginContainer(versioningRepository, WorkspaceResources, managementServices);
            _catalogPluginContainer.Build(this);
        }

        void InitializeWorkspaceResources()
        {
            WorkspaceResources = new ConcurrentDictionary<Guid, List<IResource>>();
        }
        public ConcurrentDictionary<Guid, ManagementServiceResource> ManagementServices => _catalogPluginContainer.LoadProvider.ManagementServices;
        public ConcurrentDictionary<Guid, object> WorkspaceLocks => _catalogPluginContainer.LoadProvider.WorkspaceLocks;
        public List<DuplicateResource> DuplicateResources
        {
            get
            {
                return _catalogPluginContainer.LoadProvider.DuplicateResources;
            }
            set
            {
                _catalogPluginContainer.LoadProvider.DuplicateResources = value;
            }
        }
        public ConcurrentDictionary<Guid, List<IResource>> WorkspaceResources { get; private set; }

        public int GetResourceCount(Guid workspaceID) => _catalogPluginContainer.LoadProvider.GetResourceCount(workspaceID);
        public IResource GetResource(Guid workspaceID, string resourceName) => _catalogPluginContainer.LoadProvider.GetResource(workspaceID, resourceName, "Unknown", null);
        public IResource GetResource(Guid workspaceID, string resourceName, string resourceType, string version)
            => _catalogPluginContainer.LoadProvider.GetResource(workspaceID, resourceName, resourceType, version);
        public IResource GetResource(Guid workspaceID, Guid resourceId, string resourceType, string version)
            => _catalogPluginContainer.LoadProvider.GetResource(workspaceID, resourceId, resourceType, version);
        public IResource GetResource(Guid workspaceID, Guid resourceID, string version)
            => _catalogPluginContainer.LoadProvider.GetResource(workspaceID,resourceID,version);
        public StringBuilder GetResourceContents(IResource resource) => _catalogPluginContainer.LoadProvider.GetResourceContents(resource);
        public StringBuilder GetResourceContents(Guid workspaceID, Guid resourceID) => _catalogPluginContainer.LoadProvider.GetResourceContents(workspaceID, resourceID);
        public IEnumerable GetModels(Guid workspaceID, enSourceType sourceType) => _catalogPluginContainer.LoadProvider.GetModels(workspaceID, sourceType);
        public T[] FindByType<T>() => _catalogPluginContainer.LoadProvider.FindByType<T>();
        public object[] FindByType(string typeName) => _catalogPluginContainer.LoadProvider.FindByType(typeName);
        public IList<Resource> GetResourceList(Guid workspaceId, Dictionary<string, string> filterParams) => _catalogPluginContainer.LoadProvider.GetResourceList(workspaceId, filterParams);
        public List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, string resourceName) where TServiceType : DynamicServiceObjectBase
                                 => _catalogPluginContainer.LoadProvider.GetDynamicObjects<TServiceType>(workspaceID, resourceName, false);
        public List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, string resourceName, bool useContains) where TServiceType : DynamicServiceObjectBase
                                 => _catalogPluginContainer.LoadProvider.GetDynamicObjects<TServiceType>(workspaceID, resourceName, useContains);
        public List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, Guid resourceID) where TServiceType : DynamicServiceObjectBase
            => _catalogPluginContainer.LoadProvider.GetDynamicObjects<TServiceType>(workspaceID, resourceID);
        public List<DynamicServiceObjectBase> GetDynamicObjects(IResource resource) => _catalogPluginContainer.LoadProvider.GetDynamicObjects(resource);
        public List<IResource> GetResources(Guid workspaceID) => _catalogPluginContainer.LoadProvider.GetResources(workspaceID);
        public IEnumerable<T> GetResources<T>(Guid workspaceId) where T : IWarewolfResource => _catalogPluginContainer.LoadProvider.GetResources<T>(workspaceId);
        public virtual IResource GetResource(Guid workspaceID, Guid resourceID) => _catalogPluginContainer.LoadProvider.GetResource(workspaceID, resourceID);
        public virtual T GetResource<T>(Guid workspaceID, Guid serviceID) where T : Resource, new() => _catalogPluginContainer.LoadProvider.GetResource<T>(workspaceID, serviceID);
        public T GetResource<T>(Guid workspaceID, string resourceName) where T : Resource, new() => _catalogPluginContainer.LoadProvider.GetResource<T>(workspaceID, resourceName);
        public string GetResourcePath(Guid workspaceID, Guid id) => _catalogPluginContainer.LoadProvider.GetResourcePath(workspaceID, id);
        public List<Guid> GetDependants(Guid workspaceID, Guid? resourceId) => _catalogPluginContainer.LoadProvider.GetDependants(workspaceID, resourceId);
        public List<ResourceForTree> GetDependentsAsResourceForTrees(Guid workspaceID, Guid resourceId) => _catalogPluginContainer.LoadProvider.GetDependentsAsResourceForTrees(workspaceID, resourceId);
        public IList<IResource> GetResourceList(Guid workspaceId) => _catalogPluginContainer.LoadProvider.GetResources(workspaceId);
        public IList<IResource> GetResourceList<T>(Guid workspaceId) where T : Resource, new() => _catalogPluginContainer.LoadProvider.GetResourceList<T>(workspaceId);
        public List<IResource> GetResourcesBasedOnType(string type, List<IResource> workspaceResources, Func<IResource, bool> func) => _catalogPluginContainer.LoadProvider.GetResourcesBasedOnType(type, workspaceResources, func);

        public List<DynamicServiceObjectBase> GetDynamicObjects(IEnumerable<IResource> resources) => _catalogPluginContainer.LoadProvider.GetDynamicObjects(resources);

        public void LoadWorkspace(Guid workspaceID)
        {
            try
            {
                var @lock = ResourceCatalogImpl.Common.GetWorkspaceLock(workspaceID);
                if (_loading)
                {
                    return;
                }
                _loading = true;
                lock (@lock)
                {
                    WorkspaceResources.AddOrUpdate(workspaceID,
                        id => LoadWorkspaceImpl(workspaceID),
                        (id, resources) => LoadWorkspaceImpl(workspaceID));
                }

            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error Loading Resources.", e, GlobalConstants.WarewolfError);
                throw;
            }
            finally
            {
                _loading = false;
            }
        }

        List<IResource> LoadWorkspaceImpl(Guid workspaceID)
        {
            var workspacePath = workspaceID == GlobalConstants.ServerWorkspaceID ? EnvironmentVariables.ResourcePath : EnvironmentVariables.GetWorkspacePath(workspaceID);
            IList<IResource> userServices = new List<IResource>();
            if (Directory.Exists(workspacePath))
            {
                var folders = Directory.EnumerateDirectories(workspacePath, "*", SearchOption.AllDirectories);
                var allFolders = folders.ToList();
                allFolders.Add(workspacePath);
                userServices = LoadWorkspaceViaBuilder(workspacePath, workspaceID == GlobalConstants.ServerWorkspaceID, allFolders.ToArray());
            }
            var result = userServices.Union(ManagementServices.Values);
            var resources = result.ToList();

            return resources;
        }

        public void LoadServerActivityCache()
        {
            if (!_parsers.ContainsKey(GlobalConstants.ServerWorkspaceID))
            {
                BuildResourceActivityCache(GetResources(GlobalConstants.ServerWorkspaceID));
            }
        }

        public IResourceActivityCache GetResourceActivityCache(Guid workspaceID)
        {
            if (_parsers.ContainsKey(workspaceID))
            {
                if (_parsers.TryGetValue(workspaceID, out IResourceActivityCache resourceCache))
                {
                    return resourceCache;
                }
            }
            return null;
        }
        void BuildResourceActivityCache(IEnumerable<IResource> userServices)
        {
            if (_parsers.ContainsKey(GlobalConstants.ServerWorkspaceID))
            {
                return;
            }
            foreach (var resource in userServices)
            {
                AddToActivityCache(resource);
            }
        }

        public void AddToActivityCache(IResource resource)
        {
            IResourceActivityCache parser = null;
            if (_parsers != null && !_parsers.TryGetValue(GlobalConstants.ServerWorkspaceID, out parser))
            {
                parser = new ResourceActivityCache(CustomContainer.Get<IActivityParser>(), new ConcurrentDictionary<Guid, IDev2Activity>());
                _parsers.AddOrUpdate(GlobalConstants.ServerWorkspaceID, parser, (key, cache) =>
                {
                    if (_parsers.TryGetValue(key, out IResourceActivityCache existingCache))
                    {
                        return existingCache;
                    }
                    return cache;
                });
            }
            if (parser != null && !parser.HasActivityInCache(resource.ResourceID))
            {
                var service = GetService(GlobalConstants.ServerWorkspaceID, resource.ResourceID, resource.ResourceName);
                if (service != null)
                {
                    var sa = service.Actions.FirstOrDefault();
                    MapServiceActionDependencies(GlobalConstants.ServerWorkspaceID, sa);
                    ServiceActionRepo.Instance.AddToCache(resource.ResourceID, service);
                    var activity = GetActivity(sa);
                    parser.Parse(activity, resource.ResourceID);
                }
            }
        }

        public DynamicActivity GetActivity(ServiceAction sa)
        {
            var act = sa.PopActivity();
            var theActivity = act.Value as DynamicActivity;
            return theActivity;
        }

        public DynamicService GetService(Guid workspaceID, Guid resourceID, string resourceName)
        {
            DynamicService serviceAction = null;
            if (resourceID != Guid.Empty)
            {
                var services = GetDynamicObjects<DynamicService>(workspaceID, resourceID);
                serviceAction = services.FirstOrDefault();
            }
            else
            {
                if (!string.IsNullOrEmpty(resourceName))
                {
                    var services = GetDynamicObjects<DynamicService>(workspaceID, resourceName);
                    serviceAction = services.FirstOrDefault();
                }
            }
            return serviceAction;
        }

        public void MapServiceActionDependencies(Guid workspaceID, ServiceAction serviceAction)
        {

            serviceAction.Service = GetService(workspaceID, serviceAction.ServiceID, serviceAction.ServiceName);
            if (!string.IsNullOrWhiteSpace(serviceAction.SourceName))
            {
                var sources = GetDynamicObjects<Source>(workspaceID, serviceAction.SourceName);
                var source = sources.FirstOrDefault();

                serviceAction.Source = source;
            }
        }


        // Travis.Frisinger - 02.05.2013 
        // 
        // Removed the Async operation with file stream as it would fail to use the correct stream from time to time
        // causing the integration test suite to fail. By moving the operation into a Parallel.ForEach approach this 
        // appears to have nearly the same impact with better stability.
        // ResourceCatalogBuilder now contains the refactored async logic ;)

        /// <summary>
        /// Loads the workspace via builder.
        /// </summary>
        /// <param name="workspacePath">The workspace path.</param>
        /// <param name="getDuplicates"></param>
        /// <param name="folders">The folders.</param>
        /// <returns></returns>
        public IList<IResource> LoadWorkspaceViaBuilder(string workspacePath, bool getDuplicates, params string[] folders)
        {
            Builder = new ResourceCatalogBuilder();
            Builder.TryBuildCatalogFromWorkspace(workspacePath, folders);
            var resources = Builder.ResourceList;
            if (getDuplicates)
            {
                DuplicateResources = Builder.DuplicateResources;
            }
            return resources;
        }

        public IList<IResource> LoadExamplesViaBuilder(string releasePath)
        {
            Builder = new ResourceCatalogBuilder();
            Builder.BuildReleaseExamples(releasePath);
            var resources = Builder.ResourceList;

            return resources;
        }

        public IList<DuplicateResource> GetDuplicateResources() => DuplicateResources;
        public ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string savedPath) => _catalogPluginContainer.SaveProvider.SaveResource(workspaceID, resourceXml, savedPath, "", "");
        public ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string savedPath, string reason) => _catalogPluginContainer.SaveProvider.SaveResource(workspaceID, resourceXml, savedPath, reason, "");
        public ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string savedPath, string reason, string user) => _catalogPluginContainer.SaveProvider.SaveResource(workspaceID, resourceXml, savedPath, reason, user);
        public string SetResourceFilePath(Guid workspaceID, IResource resource, ref string savedPath) => _catalogPluginContainer.SaveProvider.SetResourceFilePath(workspaceID, resource, ref savedPath);
        public ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, string savedPath) => _catalogPluginContainer.SaveProvider.SaveResource(workspaceID, resource, savedPath, "", "");
        public ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, string savedPath, string reason, string user) => _catalogPluginContainer.SaveProvider.SaveResource(workspaceID, resource, savedPath, reason, user);
        public ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath) => _catalogPluginContainer.SaveProvider.SaveResource(workspaceID, resource, contents, savedPath, "", "");
        public ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath, string reason, string user) => _catalogPluginContainer.SaveProvider.SaveResource(workspaceID, resource, contents, savedPath, reason, user);

        public Action<IResource> ResourceSaved
        {
            get
            {
                return _catalogPluginContainer.SaveProvider.ResourceSaved;
            }
            set
            {
                _catalogPluginContainer.SaveProvider.ResourceSaved = value;
            }
        }

        public Action<Guid, IList<ICompileMessageTO>> SendResourceMessages
        {
            get
            {
                return _catalogPluginContainer.SaveProvider.SendResourceMessages;
            }
            set
            {
                _catalogPluginContainer.SaveProvider.SendResourceMessages = value;
            }
        }

        internal ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents) => (_catalogPluginContainer.SaveProvider).SaveImpl(workspaceID, resource, contents, true, "", "");
        internal ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath) => (_catalogPluginContainer.SaveProvider).SaveImpl(workspaceID, resource, contents, true, savedPath, "");
        internal ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath, string reason) => (_catalogPluginContainer.SaveProvider).SaveImpl(workspaceID, resource, contents, true, savedPath, reason);

        public ResourceCatalogResult DeleteResource(Guid workspaceID, string resourceName, string type) => _catalogPluginContainer.DeleteProvider.DeleteResource(workspaceID, resourceName, type, true);
        public ResourceCatalogResult DeleteResource(Guid workspaceID, string resourceName, string type, bool deleteVersions) => _catalogPluginContainer.DeleteProvider.DeleteResource(workspaceID, resourceName, type, deleteVersions);
        public ResourceCatalogResult DeleteResource(Guid workspaceID, Guid resourceID, string type) => _catalogPluginContainer.DeleteProvider.DeleteResource(workspaceID, resourceID, type, true);
        public ResourceCatalogResult DeleteResource(Guid workspaceID, Guid resourceID, string type, bool deleteVersions) => _catalogPluginContainer.DeleteProvider.DeleteResource(workspaceID, resourceID, type, deleteVersions);

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath) => _catalogPluginContainer.SyncProvider.SyncTo(sourceWorkspacePath, targetWorkspacePath, true, true, null);

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite) => _catalogPluginContainer.SyncProvider.SyncTo(sourceWorkspacePath, targetWorkspacePath, overwrite, true, null);

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite, bool delete) => _catalogPluginContainer.SyncProvider.SyncTo(sourceWorkspacePath, targetWorkspacePath, overwrite, delete, null);

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite, bool delete, IList<string> filesToIgnore) => _catalogPluginContainer.SyncProvider.SyncTo(sourceWorkspacePath, targetWorkspacePath, overwrite, delete, filesToIgnore);

        public ResourceCatalogResult RenameResource(Guid workspaceID, Guid? resourceID, string newName, string resourcePath) => _catalogPluginContainer.RenameProvider.RenameResource(workspaceID, resourceID, newName, resourcePath);

        public ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory) => _catalogPluginContainer.RenameProvider.RenameCategory(workspaceID, oldCategory, newCategory);

        public ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory, List<IResource> resourcesToUpdate) => _catalogPluginContainer.RenameProvider.RenameCategory(workspaceID, oldCategory, newCategory, resourcesToUpdate);

        public StringBuilder ToPayload(IResource resource)
        {
            var result = new StringBuilder();

            if (resource.ResourceType == "ReservedService")
            {
                result.AppendFormat("<Service Name=\"{0}\" ResourceType=\"{1}\" />", resource.ResourceName, resource.ResourceType);
            }
            else
            {
                var contents = GetResourceContents(resource);
                if (contents != null)
                {
                    contents = contents.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
                    result.Append(contents);
                }
            }

            return result;
        }

        public void RemoveFromResourceActivityCache(Guid workspaceID, IResource resource)
        {
            if (_parsers != null && _parsers.TryGetValue(workspaceID, out IResourceActivityCache parser) && resource != null)
            {
                parser.RemoveFromCache(resource.ResourceID);
            }

        }

        public void Dispose()
        {
            lock (_loadLock)
            {
                WorkspaceLocks.Clear();
            }
            lock (_loadLock)
            {
                WorkspaceResources.Clear();
            }
            _parsers = new ConcurrentDictionary<Guid, IResourceActivityCache>();
        }

        static ConcurrentDictionary<Guid, IResourceActivityCache> _parsers = new ConcurrentDictionary<Guid, IResourceActivityCache>();
        bool _loading;

        public IDev2Activity Parse(Guid workspaceID, Guid resourceID) => Parse(workspaceID, resourceID, "");

        public IDev2Activity Parse(Guid workspaceID, Guid resourceID, string executionId)
        {
            return Parse(workspaceID, resourceID, executionId, null);
        }
        public IDev2Activity Parse(Guid workspaceID, Guid resourceID, string executionId, IResource resourceOverride)
        {

            IResourceActivityCache parser = null;
            Dev2Logger.Debug($"Fetching Execution Plan for {resourceID} for workspace {workspaceID}", string.IsNullOrEmpty(executionId) ? GlobalConstants.WarewolfDebug : executionId);
            // get workspace cache entries
            if (_parsers != null && !_parsers.TryGetValue(workspaceID, out parser))
            {
                parser = new ResourceActivityCache(CustomContainer.Get<IActivityParser>(), new ConcurrentDictionary<Guid, IDev2Activity>());
                _parsers.AddOrUpdate(workspaceID, parser, (key, cache) =>
                {
                    if (_parsers.TryGetValue(key, out IResourceActivityCache existingCache))
                    {
                        return existingCache;
                    }
                    return cache;
                });
            }
            // get activity cache entry from workspace cache entry
            if (parser != null && parser.HasActivityInCache(resourceID) && resourceOverride == null)
            {
                var dev2Activity = parser.GetActivity(resourceID);
                if (dev2Activity != null)
                {
                    return dev2Activity;
                }

            }
            // load resource
            var resource = resourceOverride;
            if (resourceOverride is null)
            {
                resource = GetResource(workspaceID, resourceID);
            }
            // get first activity for resource and initialize it
            var service = GetService(workspaceID, resourceID, resource.ResourceName);
            if (service != null)
            {
                var sa = service.Actions.FirstOrDefault();
                MapServiceActionDependencies(workspaceID, sa);
                ServiceActionRepo.Instance.AddToCache(resourceID, service);
                var activity = GetActivity(sa);
                if (parser != null)
                {
                    return parser.Parse(activity, resourceID);
                }
            }

            return null;
        }

        public void Reload()
        {
            LoadWorkspace(GlobalConstants.ServerWorkspaceID);
            _parsers.TryRemove(GlobalConstants.ServerWorkspaceID, out IResourceActivityCache removedCache);
            LoadServerActivityCache();
        }

        public ResourceCatalogDuplicateResult DuplicateResource(Guid resourceId, string sourcePath, string destinationPath) => _catalogPluginContainer.DuplicateProvider.DuplicateResource(resourceId, sourcePath, destinationPath);
        public ResourceCatalogDuplicateResult DuplicateFolder(string sourcePath, string destinationPath, string newName, bool fixRefences) => _catalogPluginContainer.DuplicateProvider.DuplicateFolder(sourcePath, destinationPath, newName, fixRefences);
        public ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting) => (_catalogPluginContainer.SaveProvider).SaveImpl(workspaceID, resource, contents, true, "", "");
        public ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting, string savedPath) => (_catalogPluginContainer.SaveProvider).SaveImpl(workspaceID, resource, contents, true, savedPath, "");
        public ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting, string savedPath, string reason) => (_catalogPluginContainer.SaveProvider).SaveImpl(workspaceID, resource, contents, true, savedPath, reason);
        ResourceCatalogResult IResourceCatalog.SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents) => (_catalogPluginContainer.SaveProvider).SaveImpl(workspaceID, resource, contents, true, "", "");
        ResourceCatalogResult IResourceCatalog.SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath) => (_catalogPluginContainer.SaveProvider).SaveImpl(workspaceID, resource, contents, true, savedPath, "");
        ResourceCatalogResult IResourceCatalog.SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath, string reason) => (_catalogPluginContainer.SaveProvider).SaveImpl(workspaceID, resource, contents, true, savedPath, reason);
        public int GetLatestVersionNumberForResource(Guid resourceId)
        {
            return _serverVersionRepository.GetLatestVersionNumber(resourceId);
        }
    }
}
