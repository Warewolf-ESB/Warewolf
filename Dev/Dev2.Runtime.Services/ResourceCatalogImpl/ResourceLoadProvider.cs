#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects.Base;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using ServiceStack.Common.Extensions;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ResourceCatalogImpl
{
    class ResourceLoadProvider : IResourceLoadProvider
    {
        readonly ConcurrentDictionary<Guid, List<IResource>> _workspaceResources;
        readonly IServerVersionRepository _serverVersionRepository;
        ConcurrentDictionary<string, List<DynamicServiceObjectBase>> FrequentlyUsedServices { get; } = new ConcurrentDictionary<string, List<DynamicServiceObjectBase>>();
        public ConcurrentDictionary<Guid, ManagementServiceResource> ManagementServices { get; } = new ConcurrentDictionary<Guid, ManagementServiceResource>();
        public ConcurrentDictionary<Guid, object> WorkspaceLocks { get; } = new ConcurrentDictionary<Guid, object>();
        public List<DuplicateResource> DuplicateResources { get; set; }
        readonly object _loadLock = new object();
        readonly FileWrapper _dev2FileWrapper;
        readonly IPerformanceCounter _perfCounter;


        public ResourceLoadProvider(ConcurrentDictionary<Guid, List<IResource>> workspaceResources, IServerVersionRepository serverVersionRepository, IEnumerable<DynamicService> managementServices = null)
            : this(new FileWrapper())
        {
            try
            {
                _perfCounter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Count of requests for workflows which don't exist");
            }
            catch (Exception e)
            {
                Dev2Logger.Warn("Error getting perf counters. " + e.Message, "Warewolf Warn");
            }
            if (managementServices != null)
            {
                foreach (var service in managementServices)
                {
                    var resource = new ManagementServiceResource(service);
                    ManagementServices.TryAdd(resource.ResourceID, resource);
                }
            }
            _workspaceResources = workspaceResources;
            _serverVersionRepository = serverVersionRepository;
            LoadFrequentlyUsedServices();
        }

        ResourceLoadProvider(FileWrapper fileWrapper)
        {
            _dev2FileWrapper = fileWrapper;
        }

        void LoadFrequentlyUsedServices()
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
                FrequentlyUsedServices.TryAdd(resourceName, objects);
            }
        }

        public T GetResource<T>(Guid workspaceID, Guid serviceID) where T : Resource, new()
        {
            var resourceContents = ResourceContents<T>(workspaceID, serviceID);
            if (resourceContents == null || resourceContents.Length == 0)
            {
                var resource = GetResource(workspaceID, serviceID);
                var content = GetResourceContents(resource);
                return GetResource<T>(content);
            }
            return GetResource<T>(resourceContents);
        }

        public T GetResource<T>(Guid workspaceID, string resourceName) where T : Resource, new()
        {
            if (resourceName != null)
            {
                var resourceContents = ResourceContents<T>(workspaceID, resourceName);
                if (resourceContents == null || resourceContents.Length == 0)
                {
                    return null;
                }

                return GetResource<T>(resourceContents);
            }
            return null;
        }

        public string GetResourcePath(Guid workspaceID, Guid id)
        {
            var resource = GetResource(workspaceID, id);
            return resource.GetResourcePath(workspaceID);
        }

        public List<IResource> GetResources(Guid workspaceID)
        {
            try
            {
                var @lock = GetWorkspaceLock(workspaceID);
                lock (@lock)
                {
                    if (_workspaceResources != null)
                    {
                        var resources = _workspaceResources.GetOrAdd(workspaceID, LoadWorkspaceImpl);

                        return resources;
                    }
                }
                return new List<IResource>();
            }
            catch (Exception e)
            {
                Dev2Logger.Error(ErrorResource.ErrorGettingResources, e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public IEnumerable GetModels(Guid workspaceID, enSourceType sourceType)
        {
            var workspaceResources = GetResources(workspaceID);
            var resources = workspaceResources.FindAll(r => r.ResourceType == sourceType.ToString());
            if (sourceType == enSourceType.MySqlDatabase || sourceType == enSourceType.Oracle || sourceType == enSourceType.PostgreSQL || sourceType == enSourceType.SqlDatabase || sourceType==enSourceType.ODBC)
            {
                var oldResources = workspaceResources.FindAll(r => r.ResourceType.ToUpper() == "DbSource".ToUpper());
                foreach (var oldResource in oldResources)
                {
                    if (!resources.Exists(resource => resource.ResourceID == oldResource.ResourceID))
                    {
                        resources.Add(oldResource);
                    }
                }
            }
            var commands = new Dictionary<enSourceType, Func<IEnumerable>>
            {
                { enSourceType.Dev2Server, ()=>BuildSourceList<Connection>(resources) },
                { enSourceType.EmailSource, ()=>BuildSourceList<EmailSource>(resources) },
                { enSourceType.SqlDatabase, ()=>BuildSourceList<DbSource>(resources) },
                { enSourceType.MySqlDatabase, ()=>BuildSourceList<DbSource>(resources) },
                { enSourceType.PostgreSQL, ()=>BuildSourceList<DbSource>(resources) },
                { enSourceType.Oracle, ()=>BuildSourceList<DbSource>(resources) },
                { enSourceType.ODBC, ()=>BuildSourceList<DbSource>(resources) },
                { enSourceType.PluginSource, ()=>BuildSourceList<PluginSource>(resources) },
                { enSourceType.WebSource, ()=>BuildSourceList<WebSource>(resources) },
                { enSourceType.OauthSource, ()=>BuildSourceList<DropBoxSource>(resources) },
                { enSourceType.SharepointServerSource, ()=>BuildSourceList<SharepointSource>(resources) },
                { enSourceType.ExchangeSource, ()=>BuildSourceList<ExchangeSource>(resources) }
            };

            var result = commands.ContainsKey(sourceType) ? commands[sourceType].Invoke() : null;
            return result;
        }

        public List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, Guid resourceID) where TServiceType : DynamicServiceObjectBase
        {
            if (resourceID == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(resourceID));
            }

            var resource = GetResource(workspaceID, resourceID);
            var results = resource == null ? new List<DynamicServiceObjectBase>() : GetDynamicObjects(resource);
            return results.OfType<TServiceType>().ToList();
        }

        public IList<IResource> GetResourceList<T>(Guid workspaceId) where T : Resource, new()
        {
            var workspaceResources = GetResources(workspaceId);
            var resourcesMatchingType = workspaceResources.Where(resource => typeof(T) == resource.GetType());
            return resourcesMatchingType.ToList();
        }

        public List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, string resourceName) where TServiceType : DynamicServiceObjectBase=> GetDynamicObjects<TServiceType>(workspaceID, resourceName, false);

        public List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, string resourceName, bool useContains) where TServiceType : DynamicServiceObjectBase
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new ArgumentNullException(nameof(resourceName));
            }
            List<DynamicServiceObjectBase> results;

            if (useContains)
            {
                var resources = GetResources(workspaceID);
                results = GetDynamicObjects(resources.Where(r => r.ResourceName.Contains(resourceName)));
            }
            else
            {
                var resource = GetResource(workspaceID, resourceName);
                results = resource == null ? new List<DynamicServiceObjectBase>() : GetDynamicObjects(resource);
            }
            return results.OfType<TServiceType>().ToList();
        }

        public List<DynamicServiceObjectBase> GetDynamicObjects(IResource resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            var result = new List<DynamicServiceObjectBase>();
            AddResourceAsDynamicServiceObject(result, resource);
            return result;
        }

        public List<DynamicServiceObjectBase> GetDynamicObjects(IEnumerable<IResource> resources)
        {
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            var result = new List<DynamicServiceObjectBase>();
            foreach (var resource in resources)
            {
                AddResourceAsDynamicServiceObject(result, resource);
            }
            return result;
        }

        public List<Guid> GetDependants(Guid workspaceID, Guid? resourceId)
        {
            if (resourceId == null)
            {
                throw new ArgumentNullException(nameof(resourceId), ErrorResource.NoResourceName);
            }

            var resources = GetResources(workspaceID);
            var dependants = new List<Guid>();
            resources.ForEach(resource =>
            {
                if (resource.Dependencies == null && (resource.IsSource || resource.IsServer))
                {
                    resource = new Resource(resource.ToXml());
                }

                if (resource.Dependencies == null)
                {
                    return;
                }
                resource.Dependencies.ForEach(tree =>
                {
                    if (tree.ResourceID == resourceId)
                    {
                        dependants.Add(resource.ResourceID);
                    }
                });
            });
            return dependants.ToList();
        }

        public List<ResourceForTree> GetDependentsAsResourceForTrees(Guid workspaceID, Guid resourceId)
        {
            var resources = GetResources(workspaceID);
            var dependants = new List<ResourceForTree>();
            foreach (var resource in resources)
            {
                resource.Dependencies?.ForEach(tree =>
                {
                    if (tree.ResourceID == resourceId)
                    {
                        dependants.Add(CreateResourceForTree(resource, tree));
                    }
                });
            }
            return dependants.ToList();
        }

        public IList<IResource> GetResourceList(Guid workspaceId)
        {
            var workspaceResources = GetResources(workspaceId);
            return workspaceResources.ToList();
        }

        public IList<Resource> GetResourceList(Guid workspaceId, Dictionary<string, string> filterParams)
        {
            filterParams.TryGetValue("resourceName", out string resourceName);
            filterParams.TryGetValue("type", out string type);
            filterParams.TryGetValue("guidCsv", out string guidCsv);
            var workspaceResources = GetResources(workspaceId);

            if (!string.IsNullOrEmpty(guidCsv) || filterParams.ContainsKey(nameof(guidCsv)))
            {
                if (guidCsv == null)
                {
                    guidCsv = string.Empty;
                }
                var guids = SplitGuidsByComma(guidCsv);
                var resources = GetResourcesBasedOnType(type, workspaceResources, r => guids.Contains(r.ResourceID));
                return resources.Cast<Resource>().ToList();
            }
            else
            {
                if (string.IsNullOrEmpty(resourceName) && string.IsNullOrEmpty(type))
                {
                    throw new InvalidDataContractException(ErrorResource.ResourceNameAndTypeMissing);
                }
                if (string.IsNullOrEmpty(resourceName) || resourceName == "*")
                {
                    resourceName = string.Empty;
                }
                var resources = GetResourcesBasedOnType(type, workspaceResources, r => r.ResourceName.Contains(resourceName));
                return resources.Cast<Resource>().ToList();
            }
        }

        public int GetResourceCount(Guid workspaceID) => GetResources(workspaceID).Count;
        public IResource GetResource(Guid workspaceID, string resourceName) => GetResource(workspaceID, resourceName, "Unknown", null);

        public IResource GetResource(Guid workspaceID, Guid resourceID, string version)
        {
            var resource = GetResource(workspaceID, resourceID, "Unknown", version);

            if (!string.IsNullOrEmpty(version) && version != "1")
            {
                return ResourceFromGivenVersion(version, resource);
            }
            return resource;
        }

        public IResource GetResource(Guid workspaceID, string resourceName, string resourceType, string version)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new ArgumentNullException(nameof(resourceName));
            }

            var resourceNameToSearchFor = resourceName.Replace("/", "\\");
            var resourcePath = resourceNameToSearchFor;
            var endOfResourcePath = resourceNameToSearchFor.LastIndexOf('\\');
            if (endOfResourcePath >= 0)
            {
                resourceNameToSearchFor = resourceNameToSearchFor.Substring(endOfResourcePath + 1);
            }

            Func<Guid, Func<IResource, bool>> getfilter = id =>
            {
                Func<IResource, bool> result = r =>
                {
                    if (r == null)
                    {
                        return false;
                    }
                    return string.Equals(r.GetResourcePath(id) ?? "", resourcePath, StringComparison.InvariantCultureIgnoreCase) && string.Equals(r.ResourceName, resourceNameToSearchFor, StringComparison.InvariantCultureIgnoreCase) && (resourceType == "Unknown" || r.ResourceType == resourceType);
                };
                return result;
            };

            return GetResource(ref workspaceID, getfilter);
        }
        public IResource GetResource(Guid workspaceID, Guid resourceId, string resourceType, string version)
        {
            Func<Guid, Func<IResource, bool>> getfilter = id =>
            {
                Func<IResource, bool> result = r =>
                {
                    if (r == null)
                    {
                        return false;
                    }
                    return r.ResourceID.Equals(resourceId);
                };
                return result;
            };

            return GetResource(ref workspaceID, getfilter);
        }

        private IResource GetResource(ref Guid workspaceID, Func<Guid, Func<IResource, bool>> getfilter)
        {
            while (true)
            {
                var resources = GetResources(workspaceID);

                if (resources != null)
                {
                    var id = workspaceID;
                    var foundResource = resources.AsParallel().FirstOrDefault(getfilter(id));
                    if (foundResource == null && workspaceID != GlobalConstants.ServerWorkspaceID)
                    {
                        workspaceID = GlobalConstants.ServerWorkspaceID;
                        continue;
                    }
                    return foundResource;
                } 
                break;
            }
            throw new Exception("should not reach here");
        }

        private IResource ResourceFromGivenVersion(string version, IResource resource)
        {
            var xmlBuilder = _serverVersionRepository.GetVersion(new VersionInfo(DateTime.MinValue, string.Empty, string.Empty, version, resource.ResourceID, resource.VersionInfo.VersionId), string.Empty);
            var xml = xmlBuilder.ToXElement();
            return new Resource(xml);
        }

        public IResource GetResource(Guid workspaceID, Guid resourceID)
        {
            IResource foundResource = null;
            try
            {
                if (_workspaceResources.TryGetValue(workspaceID, out List<IResource> resources))
                {
                    foundResource = resources.AsParallel().FirstOrDefault(resource => resource.ResourceID == resourceID);
                }

                if (foundResource == null && workspaceID != GlobalConstants.ServerWorkspaceID && _workspaceResources.TryGetValue(GlobalConstants.ServerWorkspaceID, out resources))
                {
                    foundResource = resources.AsParallel().FirstOrDefault(resource => resource.ResourceID == resourceID);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(ErrorResource.ErrorGettingResources, e, GlobalConstants.WarewolfError);
            }
            if (foundResource == null)
            {
                _perfCounter?.Increment();
            }
            return foundResource;
        }

        public StringBuilder GetResourceContents(Guid workspaceID, Guid resourceID)
        {
            IResource foundResource = null;
            if (_workspaceResources.TryGetValue(workspaceID, out List<IResource> resources))
            {
                foundResource = resources.AsParallel().FirstOrDefault(resource => resource.ResourceID == resourceID);
            }

            if (foundResource == null && workspaceID != GlobalConstants.ServerWorkspaceID && _workspaceResources.TryGetValue(GlobalConstants.ServerWorkspaceID, out resources))
            {
                foundResource = resources.AsParallel().FirstOrDefault(resource => resource.ResourceID == resourceID);
            }

            return GetResourceContents(foundResource);
        }

        public StringBuilder GetResourceContents(IResource resource)
        {
            var contents = new StringBuilder();

            if (string.IsNullOrEmpty(resource?.FilePath) || !_dev2FileWrapper.Exists(resource.FilePath))
            {
                return contents;
            }

            // Open the file with the file share option of read. This will ensure that if the file is opened for write while this read operation
            // is happening the wite will fail.
            using (FileStream fs = new FileStream(resource.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        var readLine = sr.ReadLine();
                        if (!string.IsNullOrEmpty(readLine))
                        {
                            contents.Append(readLine);
                            contents.Append(Environment.NewLine);
                        }
                    }
                }
            }
            return contents;
        }

        StringBuilder ResourceContents<T>(Guid workspaceID, string resourceName) where T : Resource, new()
        {
            var resource = GetResource(workspaceID, resourceName);
            var resourceContents = GetResourceContents(resource);
            return CheckType<T>(resource) ? resourceContents : null;
        }

        StringBuilder ResourceContents<T>(Guid workspaceID, Guid resourceID) where T : Resource, new()
        {
            var resource = GetResource(workspaceID, resourceID);
            var resourceContents = GetResourceContents(resource);
            return CheckType<T>(resource) ? resourceContents : null;
        }

        static bool CheckType<T>(IResource resource) where T : Resource, new()
        {
            if (resource != null)
            {
                //This is for migration from pre-v1 to V1. Remove once V1 is released.
                if (typeof(T) == typeof(DbService) && resource.ResourceType == "DbService")
                {
                    return true;
                }
                if (typeof(T) == typeof(PluginService) && resource.ResourceType == "PluginService")
                {
                    return true;
                }
                if (typeof(T) == typeof(WebService) && resource.ResourceType == "WebService")
                {
                    return true;
                }

                if (typeof(T) == typeof(Workflow) && resource.IsService)
                {
                    return true;
                }
                if (typeof(IResourceSource).IsAssignableFrom(typeof(T)) && resource.IsSource)
                {
                    return true;
                }
            }
            return false;
        }

        static T GetResource<T>(StringBuilder resourceContents) where T : Resource, new()
        {
            if (resourceContents == null || string.IsNullOrEmpty(resourceContents.ToString()) || resourceContents.Length == 0)
            {
                return default(T);
            }

            var elm = resourceContents.ToXElement();
            object[] args = { elm };
            return (T)Activator.CreateInstance(typeof(T), args);
        }

        void AddResourceAsDynamicServiceObject(List<DynamicServiceObjectBase> result, IResource resource)
        {
            if (resource.ResourceType == "ReservedService")
            {
                if (resource is ManagementServiceResource managementResource)
                {
                    result.Add(managementResource.Service);
                }
            }
            else
            {
                if (!FrequentlyUsedServices.TryGetValue(resource.ResourceName, out List<DynamicServiceObjectBase> objects))
                {
                    objects = GenerateObjectGraph(resource);
                }
                else
                {
                    Dev2Logger.Debug($"{resource.ResourceName} -> Resource Catalog Cache HIT", GlobalConstants.WarewolfDebug);
                }
                if (objects != null)
                {
                    result.AddRange(objects);
                }
            }
        }
        List<Guid> SplitGuidsByComma(string guidCsv)
        {
            var guids = new List<Guid>();
            var guidStrs = guidCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var guidStr in guidStrs)
            {
                if (Guid.TryParse(guidStr, out Guid guid))
                {
                    guids.Add(guid);
                }
            }
            return guids;
        }

        public List<IResource> GetResourcesBasedOnType(string type, List<IResource> workspaceResources, Func<IResource, bool> func)
        {
            List<IResource> resources;
            if (string.IsNullOrEmpty(type))
            {
                resources = workspaceResources.FindAll(func.Invoke);
            }
            else
            {
                var commands = new Dictionary<string, List<IResource>>
                {
                    {"WorkflowService", workspaceResources.FindAll(r => func.Invoke(r) && r.IsService)},
                    {"Source", workspaceResources.FindAll(r => func.Invoke(r) && r.IsSource)},
                    {"ReservedService", workspaceResources.FindAll(r => func.Invoke(r) && r.IsReservedService)},
                };

                resources = commands.ContainsKey(type) ? commands[type] : workspaceResources.FindAll(func.Invoke);
            }
            return resources;
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

        IList<IResource> LoadWorkspaceViaBuilder(string workspacePath, bool getDuplicates, params string[] folders)
        {
            var builder = new ResourceCatalogBuilder();

            builder.TryBuildCatalogFromWorkspace(workspacePath, folders);
            if (getDuplicates)
            {
                DuplicateResources = builder.DuplicateResources;
            }

            var resources = builder.ResourceList;
            return resources;
        }
        object GetWorkspaceLock(Guid workspaceID)
        {
            lock (_loadLock)
            {
                return WorkspaceLocks.GetOrAdd(workspaceID, guid => new object());
            }
        }
        static ResourceForTree CreateResourceForTree(IResource resource, IResourceForTree tree) => new ResourceForTree
        {
            UniqueID = tree.UniqueID,
            ResourceID = resource.ResourceID,
            ResourceName = resource.ResourceName,
            ResourceType = resource.ResourceType
        };

        List<DynamicServiceObjectBase> GenerateObjectGraph(IResource resource)
        {
            var xml = GetResourceContents(resource);
            if (xml == null || xml.Length > 0)
            {
                return new ServiceDefinitionLoader().GenerateServiceGraph(xml);
            }

            return null;
        }
        IEnumerable BuildSourceList<T>(IEnumerable<IResource> resources) where T : Resource, new()
        {
            var objects = resources.Select(r => GetResource<T>(ToPayload(r))).ToList();
            return objects;
        }

        StringBuilder ToPayload(IResource resource)
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
    }
}
