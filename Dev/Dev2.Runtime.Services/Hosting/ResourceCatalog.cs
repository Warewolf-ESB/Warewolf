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
using Dev2.Runtime.ResourceCatalogImpl;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.ResourceManagement;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.Hosting
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class ResourceCatalog : IResourceCatalog, IDisposable
    {
        readonly object _loadLock = new object();
        ResourceCatalogBuilder Builder { get; set; }

        private readonly ResourceCatalogPluginContainer _catalogPluginContainer;
        #region Singleton Instance
        private static readonly Lazy<ResourceCatalog> LazyCat = new Lazy<ResourceCatalog>(() =>
        {
            var c = new ResourceCatalog(EsbManagementServiceLocator.GetServices());
            CompileMessageRepo.Instance.Ping();
            return c;
        }, LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ResourceCatalog Instance => LazyCat.Value;

        #endregion

        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceCatalog" /> class.
        /// <remarks>
        /// DO NOT instantiate directly - use static Instance property instead; this is for testing only!
        /// </remarks>
        /// </summary>
        /// <param name="managementServices">The management services to be loaded.</param>
        public ResourceCatalog(IEnumerable<DynamicService> managementServices = null)
        {            
            InitializeWorkspaceResources();
            // MUST load management services BEFORE server workspace!!
            IServerVersionRepository versioningRepository = new ServerVersionRepository(new VersionStrategy(), this, new DirectoryWrapper(), EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper());
            _catalogPluginContainer = new ResourceCatalogPluginContainer(versioningRepository, WorkspaceResources, managementServices);
            _catalogPluginContainer.Build(this);


        }
        [ExcludeFromCodeCoverage]//Used by tests only
        public ResourceCatalog(IEnumerable<DynamicService> managementServices, IServerVersionRepository serverVersionRepository)
        {            
            InitializeWorkspaceResources();
            // MUST load management services BEFORE server workspace!!
            var versioningRepository = serverVersionRepository;
            _catalogPluginContainer = new ResourceCatalogPluginContainer(versioningRepository, WorkspaceResources, managementServices);
            _catalogPluginContainer.Build(this);
        }
        #endregion

        private void InitializeWorkspaceResources()
        {
            WorkspaceResources = new ConcurrentDictionary<Guid, List<IResource>>();
        }

        #region Properties

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
        #endregion

        #region ResourceLoadProvider

        public int GetResourceCount(Guid workspaceID) => _catalogPluginContainer.LoadProvider.GetResourceCount(workspaceID);
        public IResource GetResource(Guid workspaceID, string resourceName, string resourceType = "Unknown", string version = null) => _catalogPluginContainer.LoadProvider.GetResource(workspaceID, resourceName, resourceType, version);
        //public IResource GetResource(string resourceName, Guid workspaceId) => _catalogPluginContainer.LoadProvider.GetResource(resourceName, workspaceId);
        public StringBuilder GetResourceContents(IResource resource) => _catalogPluginContainer.LoadProvider.GetResourceContents(resource);
        public StringBuilder GetResourceContents(Guid workspaceID, Guid resourceID) => _catalogPluginContainer.LoadProvider.GetResourceContents(workspaceID, resourceID);
        public IEnumerable GetModels(Guid workspaceID, enSourceType sourceType) => _catalogPluginContainer.LoadProvider.GetModels(workspaceID, sourceType);
        public IList<Resource> GetResourceList(Guid workspaceId, Dictionary<string, string> filterParams) => _catalogPluginContainer.LoadProvider.GetResourceList(workspaceId, filterParams);
        public List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, string resourceName, bool useContains = false) where TServiceType : DynamicServiceObjectBase
                                 => _catalogPluginContainer.LoadProvider.GetDynamicObjects<TServiceType>(workspaceID, resourceName, useContains);
        public List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, Guid resourceID) where TServiceType : DynamicServiceObjectBase
            => _catalogPluginContainer.LoadProvider.GetDynamicObjects<TServiceType>(workspaceID, resourceID);
        public List<DynamicServiceObjectBase> GetDynamicObjects(IResource resource) => _catalogPluginContainer.LoadProvider.GetDynamicObjects(resource);
        public List<IResource> GetResources(Guid workspaceID) => _catalogPluginContainer.LoadProvider.GetResources(workspaceID);
        public virtual IResource GetResource(Guid workspaceID, Guid serviceID) => _catalogPluginContainer.LoadProvider.GetResource(workspaceID, serviceID);
        public virtual T GetResource<T>(Guid workspaceID, Guid serviceID) where T : Resource, new() => _catalogPluginContainer.LoadProvider.GetResource<T>(workspaceID, serviceID);
        public T GetResource<T>(Guid workspaceID, string resourceName) where T : Resource, new() => _catalogPluginContainer.LoadProvider.GetResource<T>(workspaceID, resourceName);
        public string GetResourcePath(Guid workspaceID, Guid resourceId) => _catalogPluginContainer.LoadProvider.GetResourcePath(workspaceID, resourceId);
        public List<Guid> GetDependants(Guid workspaceID, Guid? resourceId) => _catalogPluginContainer.LoadProvider.GetDependants(workspaceID, resourceId);
        public List<ResourceForTree> GetDependentsAsResourceForTrees(Guid workspaceID, Guid resourceId) => _catalogPluginContainer.LoadProvider.GetDependentsAsResourceForTrees(workspaceID, resourceId);
        public IList<IResource> GetResourceList(Guid workspaceId) => _catalogPluginContainer.LoadProvider.GetResources(workspaceId);
        public IList<IResource> GetResourceList<T>(Guid workspaceId) where T : Resource, new() => _catalogPluginContainer.LoadProvider.GetResourceList<T>(workspaceId);
        public List<IResource> GetResourcesBasedOnType(string type, List<IResource> workspaceResources, Func<IResource, bool> func) => _catalogPluginContainer.LoadProvider.GetResourcesBasedOnType(type, workspaceResources, func);

        

        public List<DynamicServiceObjectBase> GetDynamicObjects(IEnumerable<IResource> resources) => _catalogPluginContainer.LoadProvider.GetDynamicObjects(resources);
        #endregion

        #region LoadWorkspace


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
                Dev2Logger.Error("Error Loading Resources.", e);
                throw;
            }
            finally
            {
                _loading = false;
            }
        }

        #endregion

        #region LoadWorkspaceImpl

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

        public void LoadResourceActivityCache(Guid workspaceID)
        {
            if (!_parsers.ContainsKey(workspaceID) && workspaceID == GlobalConstants.ServerWorkspaceID)
            {
                BuildResourceActivityCache(workspaceID, GetResources(workspaceID));
            }
        }

        private void BuildResourceActivityCache(Guid workspaceID, IEnumerable<IResource> userServices)
        {
            if (_parsers.ContainsKey(workspaceID))
            {
                return;
            }
            foreach (var resource in userServices)
            {
                Parse(workspaceID, resource.ResourceID);
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

        #endregion

        #region LoadWorkspaceAsync

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
            Builder.BuildCatalogFromWorkspace(workspacePath, folders);
            var resources = Builder.ResourceList;
            if (getDuplicates)
            {
                DuplicateResources = Builder.DuplicateResources;
            }
            return resources;
        }

        public IList<DuplicateResource> GetDuplicateResources()
        {
            
            return DuplicateResources;
        }
        

        #endregion

        #region CopyResource

        public bool CopyResource(Guid resourceID, Guid sourceWorkspaceID, Guid targetWorkspaceID, string userRoles = null) => _catalogPluginContainer.CopyProvider.CopyResource(resourceID, sourceWorkspaceID, targetWorkspaceID, userRoles);
        public bool CopyResource(IResource resource, Guid targetWorkspaceID, string userRoles = null) => _catalogPluginContainer.CopyProvider.CopyResource(resource, targetWorkspaceID, userRoles);

        #endregion

        #region SaveResource

        public ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string savedPath, string reason = "", string user = "") => _catalogPluginContainer.SaveProvider.SaveResource(workspaceID, resourceXml, savedPath, reason, user);
        public string SetResourceFilePath(Guid workspaceID, IResource resource, ref string savedPath) => _catalogPluginContainer.SaveProvider.SetResourceFilePath(workspaceID, resource, ref savedPath);
        public ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, string savedPath, string reason = "", string user = "") => _catalogPluginContainer.SaveProvider.SaveResource(workspaceID, resource, savedPath, reason, user);
        public ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath, string reason = "", string user = "") => _catalogPluginContainer.SaveProvider.SaveResource(workspaceID, resource,contents,savedPath, reason, user);

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

        internal ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting = true, string savedPath = "") => ((ResourceSaveProvider)_catalogPluginContainer.SaveProvider).SaveImpl(workspaceID, resource, contents, overwriteExisting, savedPath);

        #endregion

        #region DeleteResource

        public ResourceCatalogResult DeleteResource(Guid workspaceID, string resourceName, string type, bool deleteVersions = true) => _catalogPluginContainer.DeleteProvider.DeleteResource(workspaceID, resourceName, type, deleteVersions);
        public ResourceCatalogResult DeleteResource(Guid workspaceID, Guid resourceID, string type, bool deleteVersions = true) => _catalogPluginContainer.DeleteProvider.DeleteResource(workspaceID, resourceID, type, deleteVersions);

        #endregion

        #region SyncTo

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite = true, bool delete = true, IList<string> filesToIgnore = null) => _catalogPluginContainer.SyncProvider.SyncTo(sourceWorkspacePath, targetWorkspacePath, overwrite, delete, filesToIgnore);

        #endregion

        #region Rename Resource

        public ResourceCatalogResult RenameResource(Guid workspaceID, Guid? resourceID, string newName, string resourcePath) => _catalogPluginContainer.RenameProvider.RenameResource(workspaceID, resourceID, newName, resourcePath);

        public ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory) => _catalogPluginContainer.RenameProvider.RenameCategory(workspaceID, oldCategory, newCategory);

        public ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory, List<IResource> resourcesToUpdate) => _catalogPluginContainer.RenameProvider.RenameCategory(workspaceID, oldCategory, newCategory, resourcesToUpdate);

        #endregion

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
            IResourceActivityCache parser;
            if (_parsers != null && _parsers.TryGetValue(workspaceID, out parser))
            {
                if (resource != null)
                {
                    parser.RemoveFromCache(resource.ResourceID);
                }
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
            _parsers = new Dictionary<Guid, IResourceActivityCache>();
        }

        private static Dictionary<Guid, IResourceActivityCache> _parsers = new Dictionary<Guid, IResourceActivityCache>();
        bool _loading;

        public IDev2Activity Parse(Guid workspaceID, Guid resourceID)
        {
            IResourceActivityCache parser = null;
            Dev2Logger.Debug($"Fetching Execution Plan for {resourceID} for workspace {workspaceID}");
            if (_parsers != null && !_parsers.TryGetValue(workspaceID, out parser))
            {
                parser = new ResourceActivityCache(CustomContainer.Get<IActivityParser>(), new ConcurrentDictionary<Guid, IDev2Activity>());
                _parsers.Add(workspaceID, parser);
            }
            if (parser != null && parser.HasActivityInCache(resourceID))
            {
                var dev2Activity = parser.GetActivity(resourceID);
                if (dev2Activity != null)
                {
                    return dev2Activity;
                }
                
            }
            var resource = GetResource(workspaceID, resourceID);
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
        }

        

        public ResourceCatalogDuplicateResult DuplicateResource(Guid resourceId, string sourcePath, string destinationPath) => _catalogPluginContainer.DuplicateProvider.DuplicateResource(resourceId, sourcePath, destinationPath);

        public ResourceCatalogDuplicateResult DuplicateFolder(string sourcePath, string destinationPath, string newName, bool fixRefences) => _catalogPluginContainer.DuplicateProvider.DuplicateFolder(sourcePath, destinationPath, newName, fixRefences);
        

    }
}
