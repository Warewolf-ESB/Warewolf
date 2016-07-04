/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml.Linq;
using ChinhDo.Transactions;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.DynamicServices.Objects.Base;
using Dev2.Runtime.Compiler;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ResourceCatalogImpl;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using ServiceStack.Common.Extensions;
using Warewolf.Resource.Errors;
using Warewolf.ResourceManagement;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming
// ReSharper disable LocalizableElement

namespace Dev2.Runtime.Hosting
{

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class ResourceCatalog : IResourceCatalog
    {
        readonly ConcurrentDictionary<string, object> _fileLocks = new ConcurrentDictionary<string, object>();
        readonly object _loadLock = new object();

        readonly ConcurrentDictionary<string, List<DynamicServiceObjectBase>> _frequentlyUsedServices = new ConcurrentDictionary<string, List<DynamicServiceObjectBase>>();
        readonly IServerVersionRepository _versioningRepository;
        private readonly FileWrapper _dev2FileWrapper = new FileWrapper();
        readonly IResourceLoadProvider _resourceLoadProvider;
        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile ResourceCatalog _instance;
        static readonly object SyncRoot = new object();
        public Action<IResource> ResourceSaved;
        public Action<Guid, IList<ICompileMessageTO>> SendResourceMessages;
        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ResourceCatalog Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new ResourceCatalog(EsbManagementServiceLocator.GetServices());
                            CompileMessageRepo.Instance.Ping();

                        }
                    }
                }

                return _instance;
            }
        }


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
            // MUST load management services BEFORE server workspace!!
            _versioningRepository = new ServerVersionRepository(new VersionStrategy(), this, new DirectoryWrapper(), EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper());
           _resourceLoadProvider = new ResourceLoadProvider(managementServices);

        }
        [ExcludeFromCodeCoverage]//Used by tests only
        public ResourceCatalog(IEnumerable<DynamicService> managementServices, IServerVersionRepository serverVersionRepository)
        {
            // MUST load management services BEFORE server workspace!!
            _versioningRepository = serverVersionRepository;
            _resourceLoadProvider = new ResourceLoadProvider(managementServices);
        }
        #endregion

        #region Properties

        public ConcurrentDictionary<string, List<DynamicServiceObjectBase>> FrequentlyUsedServices => _resourceLoadProvider.FrequentlyUsedServices;
        public ConcurrentDictionary<Guid, ManagementServiceResource> ManagementServices => _resourceLoadProvider.ManagementServices;
        public ConcurrentDictionary<Guid, object> WorkspaceLocks => _resourceLoadProvider.WorkspaceLocks;
        public ConcurrentDictionary<Guid, List<IResource>> WorkspaceResources => _resourceLoadProvider.WorkspaceResources;
        #endregion

        #region ResourceLoadProvider

        public int GetResourceCount(Guid workspaceID) => _resourceLoadProvider.GetResourceCount(workspaceID);
        public IResource GetResource(Guid workspaceID, string resourceName, string resourceType = "Unknown", string version = null) => _resourceLoadProvider.GetResource(workspaceID, resourceName, resourceType, version);
        public IResource GetResource(string resourceName, Guid workspaceId) => _resourceLoadProvider.GetResource(resourceName, workspaceId);
        public StringBuilder GetResourceContents(IResource resource) => _resourceLoadProvider.GetResourceContents(resource);
        public StringBuilder GetResourceContents(Guid workspaceID, Guid resourceID) => _resourceLoadProvider.GetResourceContents(workspaceID, resourceID);
        public IEnumerable GetModels(Guid workspaceID, enSourceType sourceType) => _resourceLoadProvider.GetModels(workspaceID, sourceType);
        public IList<Resource> GetResourceList(Guid workspaceId, Dictionary<string, string> filterParams) => _resourceLoadProvider.GetResourceList(workspaceId, filterParams);
        public List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, string resourceName, bool useContains = false) where TServiceType : DynamicServiceObjectBase
                                 => _resourceLoadProvider.GetDynamicObjects<TServiceType>(workspaceID, resourceName, useContains);
        public List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, Guid resourceID) where TServiceType : DynamicServiceObjectBase
            => _resourceLoadProvider.GetDynamicObjects<TServiceType>(workspaceID, resourceID);

        

        public List<DynamicServiceObjectBase> GetDynamicObjects(IResource resource) => _resourceLoadProvider.GetDynamicObjects(resource);
        public List<IResource> GetResources(Guid workspaceID) => _resourceLoadProvider.GetResources(workspaceID);
        public virtual IResource GetResource(Guid workspaceID, Guid serviceID) => _resourceLoadProvider.GetResource(workspaceID, serviceID);
        public virtual T GetResource<T>(Guid workspaceID, Guid serviceID) where T : Resource, new() => _resourceLoadProvider.GetResource<T>(workspaceID, serviceID);
        public T GetResource<T>(Guid workspaceID, string resourceName) where T : Resource, new() => _resourceLoadProvider.GetResource<T>(workspaceID, resourceName);
        public string GetResourcePath(Guid id) => _resourceLoadProvider.GetResourcePath(id);
        public List<Guid> GetDependants(Guid workspaceID, Guid? resourceId) => _resourceLoadProvider.GetDependants(workspaceID, resourceId);
        public List<ResourceForTree> GetDependentsAsResourceForTrees(Guid workspaceID, Guid resourceId) => _resourceLoadProvider.GetDependentsAsResourceForTrees(workspaceID, resourceId);
        public IList<IResource> GetResourceList(Guid workspaceId) => _resourceLoadProvider.GetResources(workspaceId);
        public IList<IResource> GetResourceList<T>(Guid workspaceId) where T : Resource, new() => _resourceLoadProvider.GetResourceList<T>(workspaceId);
        IEnumerable<IResource> GetResources(Guid workspaceID, Func<IResource, bool> filterResources) => GetResources(workspaceID).Where(filterResources);

        #endregion

    
        #region GetPayload

        private static List<IResource> GetResourcesBasedOnType(string type, List<IResource> workspaceResources, Func<IResource, bool> func)
        {
            List<IResource> resources;
            if (string.IsNullOrEmpty(type))
            {
                resources = workspaceResources.FindAll(func.Invoke);
            }
            else
            {
                Dictionary<string, List<IResource>> commands = new Dictionary<string, List<IResource>>()
                {
                    {"WorkflowService", workspaceResources.FindAll(r => func.Invoke(r) && r.IsService)},
                    {"Source", workspaceResources.FindAll(r => func.Invoke(r) && r.IsSource)},
                    {"ReservedService", workspaceResources.FindAll(r => func.Invoke(r) && r.IsReservedService)},
                };

                resources = commands.ContainsKey(type) ? commands[type] : workspaceResources.FindAll(func.Invoke);
            }
            return resources;
        }



        #endregion

        #region LoadWorkspace


        public void LoadWorkspace(Guid workspaceID)
        {
            var @lock = GetWorkspaceLock(workspaceID);
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

            _loading = false;
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
                userServices = LoadWorkspaceViaBuilder(workspacePath, allFolders.ToArray());
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

        void BuildResourceActivityCache(Guid workspaceID, IEnumerable<IResource> userServices)
        {
            if (_parsers.ContainsKey(workspaceID))
            {
                return;
            }
            foreach (var resource in userServices)
            {
                AddOrUpdateToResourceActivityCache(workspaceID, resource);
            }
        }

        void AddOrUpdateToResourceActivityCache(Guid workspaceID, IResource resource)
        {
            Parse(workspaceID, resource.ResourceID);
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
        /// <param name="folders">The folders.</param>
        /// <returns></returns>
        public IList<IResource> LoadWorkspaceViaBuilder(string workspacePath, params string[] folders)
        {
            ResourceCatalogBuilder builder = new ResourceCatalogBuilder();

            builder.BuildCatalogFromWorkspace(workspacePath, folders);


            var resources = builder.ResourceList;
            return resources;
        }

        #endregion

        #region CopyResource

        public bool CopyResource(Guid resourceID, Guid sourceWorkspaceID, Guid targetWorkspaceID, string userRoles = null)
        {
            var resource = GetResource(sourceWorkspaceID, resourceID);
            return CopyResource(resource, targetWorkspaceID, userRoles);
        }

        public bool CopyResource(IResource resource, Guid targetWorkspaceID, string userRoles = null)
        {
            if (resource != null)
            {
                var copy = new Resource(resource);
                var globalResource = GetResource(Guid.Empty, resource.ResourceID);
                if (globalResource != null)
                {

                    copy.VersionInfo = globalResource.VersionInfo;
                }
                var contents = GetResourceContents(resource);
                var saveResult = SaveImpl(targetWorkspaceID, copy, contents);
                return saveResult.Status == ExecStatus.Success;
            }
            return false;
        }

        #endregion

        #region SaveResource

        public ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string userRoles = null, string reason = "", string user = "")
        {
            try
            {
                if (resourceXml == null || resourceXml.Length == 0)
                {
                    throw new ArgumentNullException(nameof(resourceXml));
                }

                var @lock = GetWorkspaceLock(workspaceID);
                lock (@lock)
                {
                    var xml = resourceXml.ToXElement();

                    var resource = new Resource(xml);
                    GlobalConstants.InvalidateCache(resource.ResourceID);
                    Dev2Logger.Info("Save Resource." + resource);
                    _versioningRepository.StoreVersion(resource, user, reason, workspaceID);

                    resource.UpgradeXml(xml, resource);

                    StringBuilder result = xml.ToStringBuilder();

                    return CompileAndSave(workspaceID, resource, result);
                }
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Save Error", err);
                throw;
            }
        }

        public ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, string userRoles = null, string reason = "", string user = "")
        {

            _versioningRepository.StoreVersion(resource, user, reason, workspaceID);

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            var @lock = GetWorkspaceLock(workspaceID);
            lock (@lock)
            {
                if (resource.ResourceID == Guid.Empty)
                {
                    resource.ResourceID = Guid.NewGuid();
                }
                GlobalConstants.InvalidateCache(resource.ResourceID);
                resource.ResourcePath = SanitizePath(resource.ResourcePath);
                var result = resource.ToStringBuilder();
                return CompileAndSave(workspaceID, resource, result);
            }
        }

        public string SanitizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            if (path.ToLower().StartsWith("root\\"))
            {
                path = path.Remove(0, 5);
            }

            if (path.ToLower().Equals("root"))
            {
                path = path.Remove(0, 4);
            }

            if (path.StartsWith("\\"))
            {
                path = path.Remove(0, 1);
            }

            return path.Replace("\\\\", "\\")
                 .Replace("\\\\", "\\");
        }

        #endregion

        #region DeleteResource

        public ResourceCatalogResult DeleteResource(Guid workspaceID, string resourceName, string type, string userRoles = null, bool deleteVersions = true)
        {
            var @lock = GetWorkspaceLock(workspaceID);
            lock (@lock)
            {
                if (resourceName == "*")
                {
                    var noWildcardsAllowedhResult = ResourceCatalogResultBuilder.CreateNoWildcardsAllowedhResult("<Result>Delete resources does not accept wildcards.</Result>.");
                    return noWildcardsAllowedhResult;
                }

                if (string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(type))
                {
                    throw new InvalidDataContractException(ErrorResource.ResourceNameAndTypeMissing);
                }

                var workspaceResources = GetResources(workspaceID);
                var resources = GetResourcesBasedOnType(type, workspaceResources, r => string.Equals(r.ResourceName, resourceName, StringComparison.InvariantCultureIgnoreCase));
                Dictionary<int, ResourceCatalogResult> commands = new Dictionary<int, ResourceCatalogResult>()
                {
                    {
                     0,ResourceCatalogResultBuilder.CreateNoMatchResult($"<Result>{type} '{resourceName}' was not found.</Result>")

                    },
                    {
                        1, DeleteImpl(workspaceID, resources, workspaceResources, deleteVersions)
                    },
                 };
                if (commands.ContainsKey(resources.Count))
                {
                    var resourceCatalogResult = commands[resources.Count];
                    return resourceCatalogResult;
                }

                return ResourceCatalogResultBuilder.CreateDuplicateMatchResult($"<Result>Multiple matches found for {type} '{resourceName}'.</Result>");
            }
        }

        public ResourceCatalogResult DeleteResource(Guid workspaceID, Guid resourceID, string type, bool deleteVersions = true)
        {
            try
            {
                var @lock = GetWorkspaceLock(workspaceID);
                lock (@lock)
                {
                    if (resourceID == Guid.Empty || string.IsNullOrEmpty(type))
                    {
                        throw new InvalidDataContractException(ErrorResource.ResourceNameAndTypeMissing);
                    }

                    var workspaceResources = GetResources(workspaceID);
                    var resources = workspaceResources.FindAll(r => Equals(r.ResourceID, resourceID));

                    var commands = GetDeleteCommands(workspaceID, resourceID, type, deleteVersions, resources, workspaceResources);
                    if (commands.ContainsKey(resources.Count))
                    {
                        var resourceCatalogResult = commands[resources.Count];
                        return resourceCatalogResult;
                    }
                    return ResourceCatalogResultBuilder.CreateDuplicateMatchResult($"<Result>Multiple matches found for {type} '{resourceID}'.</Result>");
                }
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Delete Error", err);
                throw;
            }
        }

        private Dictionary<int, ResourceCatalogResult> GetDeleteCommands(Guid workspaceID, Guid resourceID, string type, bool deleteVersions, IEnumerable<IResource> resources, List<IResource> workspaceResources)
        {
            Dictionary<int, ResourceCatalogResult> commands = new Dictionary<int, ResourceCatalogResult>()
            {
                {
                    0,
                    ResourceCatalogResultBuilder.CreateNoMatchResult($"<Result>{type} '{resourceID}' was not found.</Result>")
                },
                { 1, DeleteImpl(workspaceID, resources, workspaceResources, deleteVersions) },
            };
            return commands;
        }

        private ResourceCatalogResult DeleteImpl(Guid workspaceID, IEnumerable<IResource> resources, List<IResource> workspaceResources, bool deleteVersions = true)
        {

            IResource resource = resources.FirstOrDefault();

            if (workspaceID == Guid.Empty && deleteVersions)
                if (resource != null)
                {
                    var explorerItems = _versioningRepository.GetVersions(resource.ResourceID);
                    explorerItems?.ForEach(a => _versioningRepository.DeleteVersion(resource.ResourceID, a.VersionInfo.VersionNumber));
                }

            workspaceResources.Remove(resource);
            if (resource != null && _dev2FileWrapper.Exists(resource.FilePath))
            {
                _dev2FileWrapper.Delete(resource.FilePath);
            }
            if (resource != null)
            {
                var messages = new List<ICompileMessageTO>
                {
                    new CompileMessageTO
                    {
                        ErrorType = ErrorType.Critical,
                        MessageID = Guid.NewGuid(),
                        MessagePayload = "The resource has been deleted",
                        MessageType = CompileMessageType.ResourceDeleted,
                        ServiceID = resource.ResourceID
                    }
                };
                UpdateDependantResourceWithCompileMessages(workspaceID, resource, messages);
            }
            if (workspaceID == GlobalConstants.ServerWorkspaceID)
            {
                if (resource != null)
                {
                    ServerAuthorizationService.Instance.Remove(resource.ResourceID);
                }
            }
            RemoveFromResourceActivityCache(workspaceID, resource);
            return ResourceCatalogResultBuilder.CreateSuccessResult("Success");
        }

        void RemoveFromResourceActivityCache(Guid workspaceID, IResource resource)
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

        #endregion



        #region SyncTo

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite = true, bool delete = true, IList<string> filesToIgnore = null)
        {
            if (filesToIgnore == null)
            {
                filesToIgnore = new List<string>();
            }
            var source = new DirectoryInfo(sourceWorkspacePath);
            var destination = new DirectoryInfo(targetWorkspacePath);

            if (!source.Exists)
            {
                return;
            }

            if (!destination.Exists)
            {
                destination.Create();
            }

            //
            // Get the files from the source and desitnations folders, excluding the files which are to be ignored
            //
            var sourceFiles = source.GetFiles().Where(f => !filesToIgnore.Contains(f.Name)).ToList();
            var destinationFiles = destination.GetFiles().Where(f => !filesToIgnore.Contains(f.Name)).ToList();

            //
            // Calculate the files which are to be copied from source to destination, this respects the override parameter
            //

            var filesToCopyFromSource = new List<FileInfo>();

            if (overwrite)
            {
                filesToCopyFromSource.AddRange(sourceFiles);
            }
            else
            {
                filesToCopyFromSource.AddRange(sourceFiles
                    // ReSharper disable SimplifyLinqExpression
                    .Where(sf => !destinationFiles.Any(df => string.Compare(df.Name, sf.Name, StringComparison.OrdinalIgnoreCase) == 0)));
                // ReSharper restore SimplifyLinqExpression
            }

            //
            // Calculate the files which are to be deleted from the destination, this respects the delete parameter
            //
            // ReSharper disable once CollectionNeverQueried.Local
            var filesToDeleteFromDestination = new List<FileInfo>();
            if (delete)
            {
                filesToDeleteFromDestination.AddRange(destinationFiles
                    // ReSharper disable SimplifyLinqExpression
                    .Where(sf => !sourceFiles.Any(df => string.Compare(df.Name, sf.Name, StringComparison.OrdinalIgnoreCase) == 0)));
                // ReSharper restore SimplifyLinqExpression
            }

            //
            // Copy files from source to desination
            //
            foreach (var file in filesToCopyFromSource)
            {
                file.CopyTo(Path.Combine(destination.FullName, file.Name), true);
            }

        }

        #endregion

        #region GetDynamicObjects


        public List<DynamicServiceObjectBase> GetDynamicObjects(IEnumerable<IResource> resources) => _resourceLoadProvider.GetDynamicObjects(resources);

        #endregion

        //
        // Private Methods
        //

        #region Enum To Source Resource Conversion

        #endregion

        #region GetResources



        #endregion

        #region GetWorkspaceLock

        object GetWorkspaceLock(Guid workspaceID)
        {
            lock (_loadLock)
            {
                return WorkspaceLocks.GetOrAdd(workspaceID, guid => new object());
            }
        }

        #endregion

        #region GetFileLock

        object GetFileLock(string file)
        {
            return _fileLocks.GetOrAdd(file, o => new object());
        }

        #endregion GetFileLock



        #region CompileAndSave

        ResourceCatalogResult CompileAndSave(Guid workspaceID, IResource resource, StringBuilder contents)
        {
            // Find the service before edits ;)
            DynamicService beforeService = Instance.GetDynamicObjects<DynamicService>(workspaceID, resource.ResourceID).FirstOrDefault();

            ServiceAction beforeAction = null;
            if (beforeService != null)
            {
                beforeAction = beforeService.Actions.FirstOrDefault();
            }

            var result = SaveImpl(workspaceID, resource, contents);

            if (result.Status == ExecStatus.Success)
            {
                if (workspaceID == GlobalConstants.ServerWorkspaceID)
                {
                    CompileTheResourceAfterSave(workspaceID, resource, contents, beforeAction);
                    SavedResourceCompileMessage(workspaceID, resource, result.Message);
                }
                if (ResourceSaved != null)
                {
                    if (workspaceID == GlobalConstants.ServerWorkspaceID)
                    {
                        ResourceSaved(resource);
                    }
                }
            }

            return result;
        }

        #endregion

        #region SaveImpl

        ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting = true)
        {
            ResourceCatalogResult saveResult = null;
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { PerfomSaveResult(out saveResult, workspaceID, resource, contents, overwriteExisting); });
            return saveResult;
        }

        private void PerfomSaveResult(out ResourceCatalogResult saveResult, Guid workspaceID, IResource resource, StringBuilder contents, bool overwriteExisting)
        {
            var fileManager = new TxFileManager();
            using (TransactionScope tx = new TransactionScope())
            {
                try
                {
                    var resources = GetResources(workspaceID);
                    var conflicting = resources.FirstOrDefault(r => resource.ResourceID != r.ResourceID && r.ResourcePath != null && r.ResourcePath.Equals(resource.ResourcePath, StringComparison.InvariantCultureIgnoreCase) && r.ResourceName.Equals(resource.ResourceName, StringComparison.InvariantCultureIgnoreCase));
                    if (conflicting != null && !conflicting.IsNewResource || conflicting != null && !overwriteExisting)
                    {
                        saveResult = ResourceCatalogResultBuilder.CreateDuplicateMatchResult(string.Format(ErrorResource.TypeConflict, conflicting.ResourceType));
                        return;
                    }
                    if (resource.ResourcePath.EndsWith("\\"))
                    {
                        resource.ResourcePath = resource.ResourcePath.TrimEnd('\\');
                    }
                    var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
                    var originalRes = resource.ResourcePath ?? "";
                    int indexOfName = originalRes.LastIndexOf(resource.ResourceName, StringComparison.Ordinal);
                    var resPath = resource.ResourcePath;
                    if (indexOfName >= 0)
                    {
                        resPath = originalRes.Substring(0, originalRes.LastIndexOf(resource.ResourceName, StringComparison.Ordinal));
                    }
                    var directoryName = Path.Combine(workspacePath, resPath ?? string.Empty);

                    resource.FilePath = Path.Combine(directoryName, resource.ResourceName + ".xml");

                    #region Save to disk

                    var xml = SaveToDisk(resource, contents, directoryName, resPath, fileManager);

                    #endregion

                    #region Add to catalog

                    var updated = AddToCatalog(resource, resources, fileManager, xml);

                    #endregion

                    RemoveFromResourceActivityCache(workspaceID, resource);
                    AddOrUpdateToResourceActivityCache(workspaceID, resource);
                    tx.Complete();
                    saveResult = ResourceCatalogResultBuilder.CreateSuccessResult($"{(updated ? "Updated" : "Added")} {resource.ResourceType} '{resource.ResourceName}'");
                }
                catch (Exception)
                {
                    Transaction.Current.Rollback();
                    throw;
                }
            }
        }

        private static bool AddToCatalog(IResource resource, List<IResource> resources, TxFileManager fileManager, XElement xml)
        {
            var index = resources.IndexOf(resource);
            var updated = false;
            if (index != -1)
            {
                var existing = resources[index];
                if (!string.Equals(existing.FilePath, resource.FilePath, StringComparison.CurrentCultureIgnoreCase))
                {
                    fileManager.Delete(existing.FilePath);
                }
                resources.RemoveAt(index);
                updated = true;
            }
            resource.GetInputsOutputs(xml);
            resource.ReadDataList(xml);
            resource.SetIsNew(xml);
            resource.UpdateErrorsBasedOnXML(xml);

            resources.Add(resource);
            return updated;
        }

        private XElement SaveToDisk(IResource resource, StringBuilder contents, string directoryName, string resPath, TxFileManager fileManager)
        {
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (_dev2FileWrapper.Exists(resource.FilePath))
            {
                // Remove readonly attribute if it is set
                var attributes = _dev2FileWrapper.GetAttributes(resource.FilePath);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    _dev2FileWrapper.SetAttributes(resource.FilePath, attributes ^ FileAttributes.ReadOnly);
                }
            }

            XElement xml = contents.ToXElement();
            xml = resource.UpgradeXml(xml, resource);
            if (resource.ResourcePath != null && !resource.ResourcePath.EndsWith(resource.ResourceName))
            {
                var resourcePath = (resPath == "" ? "" : resource.ResourcePath + "\\") + resource.ResourceName;
                resource.ResourcePath = resourcePath;
                xml.SetElementValue("Category", resourcePath);
            }
            StringBuilder result = xml.ToStringBuilder();

            var signedXml = HostSecurityProvider.Instance.SignXml(result);

            lock (GetFileLock(resource.FilePath))
            {
                signedXml.WriteToFile(resource.FilePath, Encoding.UTF8, fileManager);
            }
            return xml;
        }

        void SavedResourceCompileMessage(Guid workspaceID, IResource resource, string saveMessage)
        {
            var savedResourceCompileMessage = new List<ICompileMessageTO>
            {
                new CompileMessageTO
                {
                    ErrorType = ErrorType.None,
                    MessageID = Guid.NewGuid(),
                    MessagePayload = saveMessage,
                    ServiceID = resource.ResourceID,
                    ServiceName = resource.ResourceName,
                    MessageType = CompileMessageType.ResourceSaved,
                    WorkspaceID = workspaceID,
                }
            };

            CompileMessageRepo.Instance.AddMessage(workspaceID, savedResourceCompileMessage);
        }

        public void CompileTheResourceAfterSave(Guid workspaceID, IResource resource, StringBuilder contents, ServiceAction beforeAction)
        {
            if (beforeAction != null)
            {
                // Compile the service 
                ServiceModelCompiler smc = new ServiceModelCompiler();

                var messages = GetCompileMessages(resource, contents, beforeAction, smc);
                if (messages != null)
                {
                    var keys = WorkspaceResources.Keys.ToList();
                    CompileMessageRepo.Instance.AddMessage(workspaceID, messages); //Sends the message for the resource being saved

                    var dependsMessageList = new List<ICompileMessageTO>();
                    keys.ForEach(workspace =>
                    {
                        dependsMessageList.AddRange(UpdateDependantResourceWithCompileMessages(workspace, resource, messages));
                    });
                    SendResourceMessages?.Invoke(resource.ResourceID, dependsMessageList);
                }
            }
        }

        static IList<ICompileMessageTO> GetCompileMessages(IResource resource, StringBuilder contents, ServiceAction beforeAction, ServiceModelCompiler smc)
        {
            List<ICompileMessageTO> messages = new List<ICompileMessageTO>();
            switch (beforeAction.ActionType)
            {
                case enActionType.Workflow:
                    messages.AddRange(smc.Compile(resource.ResourceID, ServerCompileMessageType.WorkflowMappingChangeRule, beforeAction.ResourceDefinition, contents));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return messages;
        }

        //Sends the messages for effected resources
        List<ICompileMessageTO> UpdateDependantResourceWithCompileMessages(Guid workspaceID, IResource resource, IList<ICompileMessageTO> messages)
        {
            var resourceId = resource.ResourceID;
            var dependants = Instance.GetDependentsAsResourceForTrees(workspaceID, resourceId);
            var dependsMessageList = new List<ICompileMessageTO>();
            foreach (var dependant in dependants)
            {
                var affectedResource = GetResource(workspaceID, dependant.ResourceID);
                foreach (var compileMessageTO in messages)
                {
                    compileMessageTO.WorkspaceID = workspaceID;
                    compileMessageTO.UniqueID = dependant.UniqueID;
                    if (affectedResource != null)
                    {
                        compileMessageTO.ServiceName = affectedResource.ResourceName;
                        compileMessageTO.ServiceID = affectedResource.ResourceID;
                    }
                    dependsMessageList.Add(compileMessageTO.Clone());
                }
                if (affectedResource != null)
                {
                    UpdateResourceXml(workspaceID, affectedResource, messages);
                }
            }
            CompileMessageRepo.Instance.AddMessage(workspaceID, dependsMessageList);
            return dependsMessageList;
        }

        void UpdateResourceXml(Guid workspaceID, IResource effectedResource, IList<ICompileMessageTO> compileMessagesTO)
        {
            var resourceContents = GetResourceContents(workspaceID, effectedResource.ResourceID);
            UpdateXmlToDisk(effectedResource, compileMessagesTO, resourceContents);
            var serverResource = GetResource(Guid.Empty, effectedResource.ResourceName);
            if (serverResource != null)
            {
                resourceContents = GetResourceContents(Guid.Empty, serverResource.ResourceID);
                UpdateXmlToDisk(serverResource, compileMessagesTO, resourceContents);
            }
        }

        void UpdateXmlToDisk(IResource resource, IList<ICompileMessageTO> compileMessagesTO, StringBuilder resourceContents)
        {

            var resourceElement = resourceContents.ToXElement();
            if (compileMessagesTO.Count > 0)
            {
                SetErrors(resourceElement, compileMessagesTO);
                UpdateIsValid(resourceElement);
            }
            else
            {
                UpdateIsValid(resourceElement);
            }

            StringBuilder result = resourceElement.ToStringBuilder();

            var signedXml = HostSecurityProvider.Instance.SignXml(result);

            lock (GetFileLock(resource.FilePath))
            {
                var fileManager = new TxFileManager();
                using (TransactionScope tx = new TransactionScope())
                {
                    try
                    {
                        signedXml.WriteToFile(resource.FilePath, Encoding.UTF8, fileManager);
                        tx.Complete();
                    }
                    catch
                    {
                        Transaction.Current.Rollback();
                    }
                }

            }
        }

        void SetErrors(XElement resourceElement, IList<ICompileMessageTO> compileMessagesTO)
        {
            if (compileMessagesTO == null || compileMessagesTO.Count == 0)
            {
                return;
            }
            var errorMessagesElement = GetErrorMessagesElement(resourceElement);
            if (errorMessagesElement == null)
            {
                errorMessagesElement = new XElement("ErrorMessages");
                resourceElement.Add(errorMessagesElement);
            }
            else
            {
                compileMessagesTO.ForEach(to =>
                {
                    IEnumerable<XElement> xElements = errorMessagesElement.Elements("ErrorMessage");
                    XElement firstOrDefault = xElements.FirstOrDefault(element =>
                    {
                        XAttribute xAttribute = element.Attribute("InstanceID");
                        if (xAttribute != null)
                        {
                            return xAttribute.Value == to.UniqueID.ToString();
                        }
                        return false;
                    });
                    firstOrDefault?.Remove();
                });

            }

            foreach (var compileMessageTO in compileMessagesTO)
            {
                var errorMessageElement = new XElement("ErrorMessage");
                errorMessagesElement.Add(errorMessageElement);
                errorMessageElement.Add(new XAttribute("InstanceID", compileMessageTO.UniqueID));
                errorMessageElement.Add(new XAttribute("Message", compileMessageTO.MessageType.GetDescription()));
                errorMessageElement.Add(new XAttribute("ErrorType", compileMessageTO.ErrorType));
                errorMessageElement.Add(new XAttribute("MessageType", compileMessageTO.MessageType));
                errorMessageElement.Add(new XAttribute("FixType", compileMessageTO.ToFixType()));
                errorMessageElement.Add(new XAttribute("StackTrace", ""));
                errorMessageElement.Add(new XCData(compileMessageTO.MessagePayload));
            }
        }

        static XElement GetErrorMessagesElement(XElement resourceElement)
        {
            var errorMessagesElement = resourceElement.Element("ErrorMessages");
            return errorMessagesElement;
        }

        static void UpdateIsValid(XElement resourceElement)
        {
            var isValid = false;
            var isValidAttrib = resourceElement.Attribute("IsValid");
            var errorMessagesElement = resourceElement.Element("ErrorMessages");
            if (errorMessagesElement == null || !errorMessagesElement.HasElements)
            {
                isValid = true;
            }
            if (isValidAttrib == null)
            {
                resourceElement.Add(new XAttribute("IsValid", isValid));
            }
            else
            {
                isValidAttrib.SetValue(isValid);
            }
        }

        #endregion

        #region ToPayload

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

        #endregion

        #region AddResourceAsDynamicServiceObject

        List<DynamicServiceObjectBase> GenerateObjectGraph(IResource resource)
        {
            var xml = GetResourceContents(resource);
            if (xml == null || xml.Length > 0)
            {
                return new ServiceDefinitionLoader().GenerateServiceGraph(xml);
            }

            return null;
        }

        #endregion

        public void LoadFrequentlyUsedServices()
        {
            // do we really need this still - YES WE DO ELSE THERE ARE INSTALL ISSUES WHEN LOADING FROM FRESH ;)

            var serviceNames = new[]
            {
                "XXX"
            };

            foreach (var serviceName in serviceNames)
            {
                var resourceName = serviceName;


                var resource = GetResource(GlobalConstants.ServerWorkspaceID, resourceName);
                var objects = GenerateObjectGraph(resource);
                _frequentlyUsedServices.TryAdd(resourceName, objects);

            }

        }

        

        public ResourceCatalogResult RenameResource(Guid workspaceID, Guid? resourceID, string newName)
        {
            GlobalConstants.HandleEmptyParameters(resourceID, "resourceID");
            GlobalConstants.HandleEmptyParameters(newName, "newName");
            var resourcesToUpdate = Instance.GetResources(workspaceID, resource => resource.ResourceID == resourceID).ToArray();
            try
            {
                if (!resourcesToUpdate.Any())
                {
                    return ResourceCatalogResultBuilder.CreateFailResult($"{ErrorResource.FailedToFindResource} '{resourceID}' to '{newName}'");
                }

                {
                    // ReSharper disable once PossibleInvalidOperationException
                    _versioningRepository.StoreVersion(GetResource(Guid.Empty, resourceID.Value), "unknown", "Rename", workspaceID);
                    //rename and save to workspace
                    var renameResult = UpdateResourceName(workspaceID, resourcesToUpdate[0], newName);
                    if (renameResult.Status != ExecStatus.Success)
                    {
                        return ResourceCatalogResultBuilder.CreateFailResult($"{ErrorResource.FailedToRenameResource} '{resourceID}' to '{newName}'");
                    }
                }
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err);
                return ResourceCatalogResultBuilder.CreateFailResult($"{ErrorResource.FailedToRenameResource} '{resourceID}' to '{newName}'");

            }
            return ResourceCatalogResultBuilder.CreateSuccessResult($"{"Renamed Resource"} '{resourceID}' to '{newName}'");
        }

        ResourceCatalogResult UpdateResourceName(Guid workspaceID, IResource resource, string newName)
        {
            //rename where used
            var oldCategory = resource.ResourcePath;
            string newCategory = "";
            var indexOfCategory = resource.ResourcePath.LastIndexOf(resource.ResourceName, StringComparison.Ordinal);
            if (indexOfCategory > 0)
            {
                newCategory = oldCategory.Substring(0, indexOfCategory) + newName;
            }
            RenameWhereUsed(GetDependentsAsResourceForTrees(workspaceID, resource.ResourceID), workspaceID, resource.ResourcePath, newName);

            //rename resource
            var resourceContents = GetResourceContents(workspaceID, resource.ResourceID);

            var resourceElement = resourceContents.ToXElement();
            //xml name attibute
            var nameAttrib = resourceElement.Attribute("Name");
            string oldName = null;
            if (nameAttrib == null)
            {
                resourceElement.Add(new XAttribute("Name", newName));
            }
            else
            {
                oldName = nameAttrib.Value;
                nameAttrib.SetValue(newName);
            }
            //xaml
            var actionElement = resourceElement.Element("Action");
            var xaml = actionElement?.Element("XamlDefinition");
            xaml?.SetValue(xaml.Value
                .Replace("x:Class=\"" + oldName, "x:Class=\"" + newName)
                .Replace("ToolboxFriendlyName=\"" + oldName, "ToolboxFriendlyName=\"" + newName)
                .Replace("DisplayName=\"" + oldName, "DisplayName=\"" + newName)
                .Replace("Category=\"" + oldCategory, "Category=\"" + newCategory))
                ;
            //xml display name element
            var displayNameElement = resourceElement.Element("DisplayName");
            displayNameElement?.SetValue(newName);
            var categoryElement = resourceElement.Element("Category");
            categoryElement?.SetValue(newCategory);
            var resPath = CalcResPath(resource);

            //delete old resource in local workspace without updating dependants with compile messages
            if (_dev2FileWrapper.Exists(resource.FilePath))
            {
                lock (GetFileLock(resource.FilePath))
                {
                    _dev2FileWrapper.Delete(resource.FilePath);
                }
            }

            resource.ResourcePath = resPath + newName;
            resource.ResourceName = newName;
            //update file path
            if (oldName != null)
            {
                resource.FilePath = resource.FilePath.Replace(oldName, newName);
            }
            //re-create, resign and save to file system the new resource
            StringBuilder contents = resourceElement.ToStringBuilder();

            return SaveImpl(workspaceID, resource, contents);

        }

        static string CalcResPath(IResource resource)
        {
            var originalRes = resource.ResourcePath ?? "";
            int indexOfName = originalRes.LastIndexOf(resource.ResourceName, StringComparison.Ordinal);
            var resPath = resource.ResourcePath;
            if (indexOfName >= 0)
            {
                resPath = originalRes.Substring(0, originalRes.LastIndexOf(resource.ResourceName, StringComparison.Ordinal));
            }
            return resPath;
        }

        private void RenameWhereUsed(IEnumerable<ResourceForTree> dependants, Guid workspaceID, string oldName, string newName)
        {
            foreach (var dependant in dependants)
            {
                var dependantResource = GetResource(workspaceID, dependant.ResourceID);
                //rename where used
                var resourceContents = GetResourceContents(workspaceID, dependantResource.ResourceID);

                var resourceElement = resourceContents.ToXElement();
                //in the xaml only
                var actionElement = resourceElement.Element("Action");
                if (actionElement != null)
                {
                    var xaml = actionElement.Element("XamlDefinition");
                    var newNameWithPath = newName;
                    if (oldName.IndexOf('\\') > 0)
                        newNameWithPath = oldName.Substring(0, 1 + oldName.LastIndexOf("\\", StringComparison.Ordinal)) + newName;
                    xaml?.SetValue(xaml.Value
                        .Replace("DisplayName=\"" + oldName, "DisplayName=\"" + newNameWithPath)
                        .Replace("ServiceName=\"" + oldName, "ServiceName=\"" + newName)
                        .Replace("ToolboxFriendlyName=\"" + oldName, "ToolboxFriendlyName=\"" + newName));
                }
                //delete old resource
                if (_dev2FileWrapper.Exists(dependantResource.FilePath))
                {
                    lock (GetFileLock(dependantResource.FilePath))
                    {
                        _dev2FileWrapper.Delete(dependantResource.FilePath);
                    }
                }
                //update dependancies
                var renameDependent = dependantResource.Dependencies.FirstOrDefault(dep => dep.ResourceName == oldName);
                if (renameDependent != null)
                {
                    renameDependent.ResourceName = newName;
                }
                //re-create, resign and save to file system the new resource
                StringBuilder result = resourceElement.ToStringBuilder();

                SaveImpl(workspaceID, dependantResource, result);
            }
        }

        public ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory)
        {
            GlobalConstants.HandleEmptyParameters(oldCategory, "oldCategory");
            GlobalConstants.HandleEmptyParameters(newCategory, "newCategory");
            try
            {
                var resourcesToUpdate = Instance.GetResources(workspaceID, resource => resource.ResourcePath.StartsWith(oldCategory + "\\", StringComparison.OrdinalIgnoreCase)).ToList();

                return RenameCategory(workspaceID, oldCategory, newCategory, resourcesToUpdate);
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Rename Category error", err);
                return ResourceCatalogResultBuilder.CreateFailResult($"<CompilerMessage>{"Failed to Category"} from '{oldCategory}' to '{newCategory}'</CompilerMessage>");
            }
        }

        public ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory, List<IResource> resourcesToUpdate)
        {
            if (resourcesToUpdate.Count == 0)
            {
                return ResourceCatalogResultBuilder.CreateNoMatchResult($"<CompilerMessage>No Resources found in '{oldCategory}'</CompilerMessage>");
            }
            return PerformUpdate(workspaceID, oldCategory, newCategory, resourcesToUpdate);
        }

        ResourceCatalogResult PerformUpdate(Guid workspaceID, string oldCategory, string newCategory, IEnumerable<IResource> resourcesToUpdate)
        {
            try
            {
                var hasError = false;
                foreach (var resource in resourcesToUpdate)
                {
                    var resourceCatalogResult = UpdateResourcePath(workspaceID, resource, oldCategory, newCategory);
                    if (resourceCatalogResult.Status != ExecStatus.Success)
                    {
                        hasError = true;
                    }
                }

                var failureResult = ResourceCatalogResultBuilder.CreateFailResult($"<CompilerMessage>{"Failed to Category"} from '{oldCategory}' to '{newCategory}'</CompilerMessage>");
                var successResult = ResourceCatalogResultBuilder.CreateSuccessResult($"<CompilerMessage>{"Updated Category"} from '{oldCategory}' to '{newCategory}'</CompilerMessage>");

                return hasError ? failureResult : successResult;
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Rename Category error", err);
                return ResourceCatalogResultBuilder.CreateFailResult($"<CompilerMessage>{"Failed to Category"} from '{oldCategory}' to '{newCategory}'</CompilerMessage>");
            }
        }

        ResourceCatalogResult UpdateResourcePath(Guid workspaceID, IResource resource, string oldCategory, string newCategory)
        {
            var resourceContents = GetResourceContents(workspaceID, resource.ResourceID);
            var oldPath = resource.ResourcePath;
            var cat = oldCategory.Replace("\\", "\\\\");
            var newPath = Regex.Replace(oldPath, cat, newCategory, RegexOptions.IgnoreCase);
            resource.ResourcePath = newPath;
            var resourceElement = resourceContents.ToXElement();
            var categoryElement = resourceElement.Element("Category");
            if (categoryElement == null)
            {
                resourceElement.Add(new XElement("Category", newPath));
            }
            else
            {
                categoryElement.SetValue(newPath);
            }
            var contents = resourceElement.ToStringBuilder();
            var resourceCatalogResult = SaveImpl(workspaceID, resource, contents, false);
            if (resourceCatalogResult.Status != ExecStatus.Success)
            {
                resource.ResourcePath = oldPath;
            }
            return resourceCatalogResult;
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

        public static Dictionary<Guid, IResourceActivityCache> _parsers = new Dictionary<Guid, IResourceActivityCache>();
        bool _loading;

        public IDev2Activity Parse(Guid workspaceID, Guid resourceID)
        {
            IResourceActivityCache parser = null;
            if (_parsers != null && !_parsers.TryGetValue(workspaceID, out parser))
            {
                parser = new ResourceActivityCache(CustomContainer.Get<IActivityParser>(), new ConcurrentDictionary<Guid, IDev2Activity>());
                _parsers.Add(workspaceID, parser);
            }
            if (parser != null && parser.HasActivityInCache(resourceID))
            {
                return parser.GetActivity(resourceID);
            }
            var resource = GetResource(workspaceID, resourceID);
            var service = GetService(workspaceID, resourceID, resource.ResourceName);
            if (service != null)
            {
                var sa = service.Actions.FirstOrDefault();
                MapServiceActionDependencies(workspaceID, sa);
                var activity = GetActivity(sa);
                if (parser != null)
                {
                    return parser.Parse(activity, resourceID);
                }
            }
            return null;
        }
    }


}
