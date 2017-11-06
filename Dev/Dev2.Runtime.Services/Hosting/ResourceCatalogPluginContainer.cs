using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Versioning;
using Dev2.DynamicServices;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ResourceCatalogImpl;

namespace Dev2.Runtime.Hosting
{
    internal class ResourceCatalogPluginContainer
    {
        private readonly IServerVersionRepository _versionRepository;
        private readonly ConcurrentDictionary<Guid, List<IResource>> _workspaceResources;
        private readonly IEnumerable<DynamicService> _managementServices;

        public ResourceCatalogPluginContainer(IServerVersionRepository versionRepository, ConcurrentDictionary<Guid, List<IResource>> workspaceResources, IEnumerable<DynamicService> managementServices = null)
        {
            _versionRepository = versionRepository;
            _workspaceResources = workspaceResources;
            _managementServices = managementServices;
        }

        public IResourceLoadProvider LoadProvider { get; private set; }
        public IResourceSyncProvider SyncProvider { get; private set; }
        public IResourceDeleteProvider DeleteProvider { get; private set; }
        public IResourceRenameProvider RenameProvider { get; private set; }
        public IResourceSaveProvider SaveProvider { get; private set; }
        public IResourceDuplicateProvider DuplicateProvider { get; private set; }

        public void Build(IResourceCatalog resourceCatalog)
        {
            LoadProvider = new ResourceLoadProvider(_workspaceResources, _managementServices);
            SyncProvider = new ResourceSyncProvider();
            DeleteProvider = new ResourceDeleteProvider(resourceCatalog,_versionRepository);
            RenameProvider = new ResourceRenameProvider(resourceCatalog,_versionRepository);
            SaveProvider = new ResourceSaveProvider(resourceCatalog,_versionRepository);
            DuplicateProvider = new ResourceDuplicateProvider(resourceCatalog);
        }
    }
}