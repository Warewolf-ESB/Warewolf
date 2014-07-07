using Dev2.Common.Common;
using Dev2.Data.ServiceModel;
using Dev2.Explorer;
using Dev2.Interfaces;
using Dev2.Models;
using Dev2.Runtime.Hosting;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace Dev2.AppResources.Repositories
{
    public class StudioResourceRepository : IStudioResourceRepository
    {
        private readonly Dispatcher _currentDispatcher;
        private readonly Action<Action, DispatcherPriority> _invoke;
        static StudioResourceRepository()
        {
            Instance = new StudioResourceRepository();
        }

        private StudioResourceRepository()
        {
            ExplorerItemModels = new ObservableCollection<ExplorerItemModel>();
            try
            {
                _currentDispatcher = Dispatcher.CurrentDispatcher;
                _invoke = _currentDispatcher.Invoke;
            }
            catch(Exception)
            {
                //This is primarily for the testing as the server runs as a service and no window handle i.e. Ui dispatcher can be gotten.
                _invoke = (action, priority) => { };
            }
        }

        internal StudioResourceRepository(IExplorerItem explorerItem, Guid environmentId, Action<Action, DispatcherPriority> invoke)
        {
            ExplorerItemModels = new ObservableCollection<ExplorerItemModel>();
            _invoke = invoke;


            if(explorerItem != null)
            {
                var environmentRepository = GetEnvironmentRepository();
                var explorerItems = MapData(explorerItem, environmentRepository, environmentId);
                LoadItemsToTree(environmentId, explorerItems);
            }
            Instance = this;
        }

        void EnvironmentRepositoryOnItemEdited(object sender, EnvironmentEditedArgs environmentEditedArgs)
        {
            var environmentModel = environmentEditedArgs.Environment;
            if(environmentModel != null && environmentEditedArgs.IsConnected)
            {
                var environmentId = environmentModel.ID;
                var itemModel = ExplorerItemModels.FirstOrDefault(model => model.EnvironmentId == environmentId && model.ResourceType == ResourceType.Server);
                if(itemModel != null)
                {
                    itemModel.IsRefreshing = true;
                }
                if(environmentModel.Connection != null)
                {
                    if(environmentModel.Connection.AsyncWorker != null)
                    {
                        Load(environmentId, environmentModel.Connection.AsyncWorker);
                    }
                }
                if(itemModel != null)
                {
                    itemModel.IsRefreshing = false;
                }
            }
        }

        //This is for testing only need better way of putting this together
        internal StudioResourceRepository(ExplorerItemModel explorerItem, Action<Action, DispatcherPriority> invoke)
        {
            ExplorerItemModels = new ObservableCollection<ExplorerItemModel>();
            _invoke = invoke;
            if(explorerItem != null)
            {
                LoadItemsToTree(explorerItem.EnvironmentId, explorerItem);
                Instance = this;
            }
        }

        public static IStudioResourceRepository Instance { get; private set; }

        #region Public Functions
        public Func<IEnvironmentRepository> GetEnvironmentRepository = () => EnvironmentRepository.Instance;

        public Func<Guid> GetCurrentEnvironment = () => EnvironmentRepository.Instance.ActiveEnvironment.ID;

        public Func<Guid, IExplorerResourceRepository> GetExplorerProxy = environmentId =>
        {
            var environmentModel = EnvironmentRepository.Instance.Get(environmentId);
            var connection = environmentModel.Connection;
            connection.ItemAddedMessageAction = Instance.ItemAddedMessageHandler;
            return new ServerExplorerClientProxy(connection);
        };
        readonly object _syncRoot = new object();
        bool _isRegistered;

        #endregion

        #region Implementation of IStudioResourceRepository

        public ObservableCollection<ExplorerItemModel> ExplorerItemModels { get; private set; }

        public void Load(Guid environmentId, IAsyncWorker asyncWorker)
        {
            if(asyncWorker == null)
            {
                throw new ArgumentNullException("asyncWorker");
            }
            var environmentRepository = GetEnvironmentRepository();
            if(!_isRegistered)
            {
                environmentRepository.ItemEdited += EnvironmentRepositoryOnItemEdited;
                _isRegistered = true;
            }
            // ReSharper disable ImplicitlyCapturedClosure
            IEnvironmentModel environmentModel = environmentRepository.FindSingle(c => c.ID == environmentId);
            // ReSharper restore ImplicitlyCapturedClosure
            if(environmentModel != null)
            {
                if(!environmentModel.IsConnected)
                {
                    asyncWorker.Start(environmentModel.Connect, () =>
                    {
                        var explorerItemModel = LoadEnvironment(environmentId);
                        LoadItemsToTree(environmentId, explorerItemModel);
                    });
                }
                else
                {
                    var explorerItemModel = LoadEnvironment(environmentId);
                    LoadItemsToTree(environmentId, explorerItemModel);
                }

            }
        }

        public void Disconnect(Guid environmentId)
        {
            var environment = ExplorerItemModels.FirstOrDefault(env => env.EnvironmentId == environmentId);

            var environmentRepository = GetEnvironmentRepository();
            IEnvironmentModel environmentModel = environmentRepository.FindSingle(c => c.ID == environmentId);
            if(environmentModel != null)
            {
                if(environmentModel.IsConnected)
                {
                    environmentModel.Disconnect();
                }
            }
            if(environment != null)
            {
                environment.IsConnected = false;
                environment.Children = new ObservableCollection<ExplorerItemModel>();
            }
        }

        public void Connect(Guid environmentId)
        {
            var environment = ExplorerItemModels.FirstOrDefault(env => env.EnvironmentId == environmentId);

            if(environment == null)
            {
                throw new Exception(string.Format("Environment Id : [{0}] was not found", environmentId.ToString()));
            }

            var explorerItemModel = LoadEnvironment(environmentId);
            var indexToReplace = ExplorerItemModels.IndexOf(environment);
            LoadItemsToTree(environmentId, explorerItemModel, indexToReplace);
        }

        public void RemoveEnvironment(Guid environmentId)
        {
            Disconnect(environmentId);
            var environment = ExplorerItemModels.FirstOrDefault(env => env.EnvironmentId == environmentId);

            if(environment != null)
            {
                ExplorerItemModels.Remove(environment);
            }
        }

        public void DeleteItem(Guid environmentId, Guid resourceId)
        {
            var environment = ExplorerItemModels.FirstOrDefault(env => env.EnvironmentId == environmentId);

            if(environment == null)
            {
                throw new Exception(string.Format("Environment Id : [{0}] was not found", environmentId));
            }

            var itemToDelete = environment.Descendants().FirstOrDefault(env => env.ResourceId == resourceId);

            if(itemToDelete == null)
            {
                return;
            }

            DeleteItem(itemToDelete);
        }

        public void DeleteFolder(ExplorerItemModel item)
        {
            VerifyArgument.IsNotNull("item", item);
            ExplorerItemModel parentItem = item.Parent;
            if(parentItem != null && parentItem.Children.Remove(item))
            {
                try
                {
                    var result = GetExplorerProxy(item.EnvironmentId).DeleteItem(MapData(item), Guid.Empty);
                    if(result.Status != ExecStatus.Success)
                    {
                        throw new Exception(result.Message);
                    }
                }
                catch(Exception)
                {
                    parentItem.Children.Add(item);
                    throw;
                }
                parentItem.OnChildrenChanged();
            }
        }

        public void UpdateRootAndFoldersPermissions(Permissions modifiedPermissions, Guid environmentGuid, bool updateRoot = true)
        {
            var server = FindItem(a => a.EnvironmentId == environmentGuid && a.ResourceType == ResourceType.Server);
            if(server != null)
            {
                server.Descendants().Where(a => a.ResourceType == ResourceType.Folder).ToList().ForEach(a =>
                    {

                        a.Permissions = modifiedPermissions;
                    });
                if(updateRoot)
                    server.Permissions = modifiedPermissions;
            }

        }

        public void DeleteItem(ExplorerItemModel item)
        {
            VerifyArgument.IsNotNull("item", item);
            var tmpParent = item.Parent;
            if(item.Parent != null)
            {
                item.Parent.Children.Remove(item);
                tmpParent.OnChildrenChanged();
            }
        }

        public void AddServerNode(ExplorerItemModel explorerItem)
        {
            var otherServers = ExplorerItemModels.Where(i => i.ResourceType == ResourceType.Server).ToList();
            otherServers.ForEach(i =>
            {
                i.IsExplorerExpanded = false;
                i.IsExplorerSelected = false;
            });

            var exists = FindItem(i => i.EnvironmentId == explorerItem.EnvironmentId && i.ResourceType == ResourceType.Server);
            if(exists == null)
            {
                ExplorerItemModels.Add(explorerItem);
                explorerItem.IsExplorerSelected = true;
            }
            else
            {
                exists.IsExplorerExpanded = true;
                exists.IsExplorerSelected = true;
            }
        }

        public void RenameItem(ExplorerItemModel item, string newName)
        {
            VerifyArgument.IsNotNull("item", item);
            VerifyArgument.IsNotNullOrWhitespace("newName", newName);

            if(item.DisplayName == newName)
            {
                return;
            }

            var result = GetExplorerProxy(item.EnvironmentId).RenameItem(MapData(item), newName, Guid.Empty);

            if(result.Status != ExecStatus.Success)
            {
                throw new Exception(result.Message);
            }

            var explorerItemModel = FindItem(i => i.EnvironmentId == item.EnvironmentId && i.ResourceId == item.ResourceId);
            if(!item.Equals(explorerItemModel))
            {
                explorerItemModel.SetDisplay(newName);
            }
        }

        public void RenameFolder(ExplorerItemModel item, string newName)
        {
            VerifyArgument.IsNotNull("item", item);
            VerifyArgument.IsNotNullOrWhitespace("newName", newName);

            if(item.DisplayName == newName)
            {
                return;
            }
            var oldResourcePath = item.ResourcePath;


            var newPath = oldResourcePath.Replace(item.DisplayName, newName);

            var environmentId = item.EnvironmentId;
            var result = GetExplorerProxy(environmentId).RenameFolder(oldResourcePath, newPath, Guid.Empty);
            if(result.Status != ExecStatus.Success)
            {
                throw new Exception(result.Message);
            }

            var explorerItemModel = LoadEnvironment(environmentId);
            LoadItemsToTree(environmentId, explorerItemModel);
        }

        public void AddResouceItem(IContextualResourceModel resourceModel)
        {
            var explorerItemModel = new ServerExplorerItem { ResourcePath = resourceModel.Category, DisplayName = resourceModel.DisplayName, ResourceId = resourceModel.ID, Permissions = resourceModel.UserPermissions };
            ResourceType resourceType;
            Enum.TryParse(resourceModel.ServerResourceType, out resourceType);
            explorerItemModel.ResourceType = resourceType;
            ItemAddedMessageHandler(explorerItemModel);
        }

        public void AddItem(ExplorerItemModel item)
        {
            VerifyArgument.IsNotNull("item", item);
            VerifyArgument.IsNotNull("parent", item.Parent);

            var explorerItem = MapData(item);

            if(explorerItem != null)
            {
                try
                {
                    var result = GetExplorerProxy(item.EnvironmentId).AddItem(explorerItem, item.Parent.EnvironmentId);

                    if(result.Status != ExecStatus.Success)
                    {
                        throw new Exception(result.Message);
                    }
                }
                catch(Exception)
                {
                    item.Parent.Children.Remove(item);
                    item.OnChildrenChanged();
                    throw;
                }
            }
        }

        public void UpdateItem(Guid id, Action<ExplorerItemModel> update, Guid environmentId)
        {
            VerifyArgument.IsNotNull("Update", update);
            var item = FindItemByIdAndEnvironment(id, environmentId);
            if(item != null)
                update(item);
        }

        public void ItemAddedMessageHandler(IExplorerItem item)
        {
            var environmentId = GetCurrentEnvironment();
            var explorerItem = MapData(item, GetEnvironmentRepository(), environmentId);
            var resourcePath = item.ResourcePath.Replace("\\\\", "\\");

            if(!String.IsNullOrEmpty(resourcePath))
            {
                resourcePath = resourcePath.Equals(item.DisplayName) ? "" : resourcePath.Substring(0, resourcePath.LastIndexOf("\\" + item.DisplayName, StringComparison.Ordinal));
            }

            var parent = FindItem(model => model.ResourcePath.Equals(resourcePath) && model.EnvironmentId == environmentId);
            var alreadyAdded = FindItem(model => model.ResourceId == item.ResourceId && model.ResourcePath == item.ResourcePath) != null;
            var environmentModel = EnvironmentRepository.Instance.Get(environmentId);
            var resourceRepository = environmentModel.ResourceRepository;
            var resourceModel = resourceRepository.FindSingle(model => model.ID == item.ResourceId);
            if(resourceModel == null && item.ResourceType != ResourceType.Folder)
            {
                if(item.ResourceType >= ResourceType.DbSource)
                {
                    resourceRepository.ReloadResource(item.ResourceId, Studio.Core.AppResources.Enums.ResourceType.Source, ResourceModelEqualityComparer.Current, true);
                }
                else if(item.ResourceType >= ResourceType.DbService && item.ResourceType < ResourceType.DbSource)
                {
                    resourceRepository.ReloadResource(item.ResourceId, Studio.Core.AppResources.Enums.ResourceType.Service, ResourceModelEqualityComparer.Current, true);
                }
                else if(item.ResourceType == ResourceType.WorkflowService)
                {
                    resourceRepository.ReloadResource(item.ResourceId, Studio.Core.AppResources.Enums.ResourceType.WorkflowService, ResourceModelEqualityComparer.Current, true);
                }
            }
            else
            {
                lock(_syncRoot)
                {
                    if(parent != null && !alreadyAdded)
                    {
                        explorerItem.EnvironmentId = parent.EnvironmentId;
                        explorerItem.Parent = parent;
                        if(parent.Children == null)
                        {
                            parent.Children = new ObservableCollection<ExplorerItemModel>();
                        }
                        if(_currentDispatcher == null)
                        {
                            AddChildItem(parent, explorerItem);
                        }
                        else
                        {
                            PerformUpdateOnDispatcher(() => AddChildItem(parent, explorerItem));

                        }
                    }
                }
            }
        }

        public void PerformUpdateOnDispatcher(Action action)
        {
            _invoke(action, DispatcherPriority.Send);
        }

        static void AddChildItem(ExplorerItemModel parent, ExplorerItemModel explorerItem)
        {
            parent.Children.Add(explorerItem);
            parent.OnChildrenChanged();
        }

        public ExplorerItemModel FindItemById(Guid id)
        {
            return FindItem(model => model.ResourceId == id);
        }
        public ExplorerItemModel FindItemByIdAndEnvironment(Guid id, Guid environmentId)
        {
            return FindItem(model => model.ResourceId == id && model.EnvironmentId == environmentId);
        }
        public ObservableCollection<ExplorerItemModel> Filter(Func<ExplorerItemModel, bool> searchCriteria)
        {
            if(searchCriteria == null)
            {
                return ExplorerItemModels;
            }
            var filteredCollection = new ObservableCollection<ExplorerItemModel>();
            foreach(var explorerItemModel in ExplorerItemModels)
            {
                var serverItem = FilterRec(searchCriteria, explorerItemModel.Clone());
                if(serverItem != null)
                {
                    filteredCollection.Add(serverItem);
                }
            }
            return filteredCollection;
        }

        private bool ContainsChild(Func<ExplorerItemModel, bool> filter, ExplorerItemModel root)
        {
            if(root.Children == null)
                return false;

            bool desc = root.Children.Any(filter);
            return desc || root.Children.Any(a => ContainsChild(filter, a));
        }

        private ExplorerItemModel FilterRec(Func<ExplorerItemModel, bool> filter, ExplorerItemModel root)
        {
            if(filter == null)
            {
                return root;
            }
            if(!ContainsChild(filter, root))
            {
                return null;
            }
            root.Children = root.Children.Where(a => FilterRec(filter, a) != null || filter(a)).ToObservableCollection();
            return root;
        }

        #endregion

        #region Private Members

        private void LoadItemsToTree(Guid environmentId, ExplorerItemModel explorerItemModel)
        {
            if(explorerItemModel != null)
            {
                explorerItemModel.EnvironmentId = environmentId;
                explorerItemModel.IsConnected = true;
                if(explorerItemModel.Children != null)
                {
                    ExplorerItemModelSetup(explorerItemModel, environmentId);
                }
                if(ExplorerItemModels.Any(a => a.EnvironmentId == environmentId))
                {
                    var index = ExplorerItemModels.IndexOf(ExplorerItemModels.First(a => a.EnvironmentId == environmentId));
                    if(index >= 0)
                        ExplorerItemModels.RemoveAt(index);
                    ExplorerItemModels.Insert(index, explorerItemModel);
                }
                else
                    ExplorerItemModels.Add(explorerItemModel);
            }
        }

        private void LoadItemsToTree(Guid environmentId, ExplorerItemModel explorerItemModel, int indexToReplace)
        {
            if(explorerItemModel != null)
            {
                explorerItemModel.EnvironmentId = environmentId;
                explorerItemModel.IsConnected = true;
                if(explorerItemModel.Children != null)
                {
                    ExplorerItemModelSetup(explorerItemModel, environmentId);
                }
                ExplorerItemModels.RemoveAt(indexToReplace);
                ExplorerItemModels.Insert(indexToReplace, explorerItemModel);
            }
        }

        private void ExplorerItemModelSetup(ExplorerItemModel explorerItemModel, Guid enviromentId)
        {
            if(explorerItemModel.Children != null)
            {
                foreach(ExplorerItemModel child in explorerItemModel.Children)
                {
                    child.Parent = explorerItemModel;
                    child.EnvironmentId = enviromentId;
                    ExplorerItemModelSetup(child, enviromentId);
                }
            }
        }

        private ExplorerItemModel LoadEnvironment(Guid environmentId)
        {
            var explorerResourceRepository = GetExplorerProxy(environmentId);
            var explorerItem = explorerResourceRepository.Load(environmentId);
            return MapData(explorerItem, GetEnvironmentRepository(), environmentId);
        }

        private ExplorerItemModel MapData(IExplorerItem item, IEnvironmentRepository environmentRepository, Guid environmentId)
        {
            if(item == null)
            {
                return null;
            }
            bool isExpanded = item.ResourceType == ResourceType.Server;

            string displayname = item.DisplayName;
            if(item.ResourceType == ResourceType.Server)
            {
                // ReSharper disable ImplicitlyCapturedClosure
                IEnvironmentModel environmentModel = environmentRepository.FindSingle(model => GetEnvironmentModel(model, item, environmentId));
                if(environmentModel != null && environmentModel.Connection != null)
                {
                    displayname = environmentModel.DisplayName;
                }
            }

            return new ExplorerItemModel
            {
                Children = item.Children == null ? new ObservableCollection<ExplorerItemModel>() : new ObservableCollection<ExplorerItemModel>(item.Children.Select(i => MapData(i, environmentRepository, environmentId))),
                DisplayName = displayname,
                ResourceType = item.ResourceType,
                ResourceId = item.ResourceId,
                Permissions = item.Permissions,
                ResourcePath = item.ResourcePath,
                IsExplorerExpanded = isExpanded
            };
            // ReSharper restore ImplicitlyCapturedClosure
        }

        public static bool GetEnvironmentModel(IEnvironmentModel model, IExplorerItem item, Guid environmentId)
        {
            if(item != null && model != null)
            {
                var found = model.ID == environmentId;
                if(found)
                {
                    return true;
                }
            }
            return false;
        }

        private static IExplorerItem MapData(ExplorerItemModel item)
        {
            return new ServerExplorerItem(item.DisplayName, item.ResourceId, item.ResourceType,
                                          item.Children == null
                                              ? null
                                              : item.Children.Select(MapData).ToList(),
                                          item.Permissions, item.ResourcePath);

        }

        #endregion

        public ExplorerItemModel FindItem(Func<ExplorerItemModel, bool> searchCriteria)
        {
            var explorerItemModels = ExplorerItemModels.SelectMany(explorerItemModel => explorerItemModel.Descendants()).ToList();
            return searchCriteria == null ? null : explorerItemModels.FirstOrDefault(searchCriteria);
        }

    }
}
