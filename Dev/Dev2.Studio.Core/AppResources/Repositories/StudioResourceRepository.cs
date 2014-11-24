
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Explorer;
using Dev2.Models;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using ServiceStack.Common.Extensions;

namespace Dev2.AppResources.Repositories
{
    public class StudioResourceRepository : IStudioResourceRepository
    {
        private readonly Dispatcher _currentDispatcher;
        private readonly Action<System.Action, DispatcherPriority> _invoke;

        static StudioResourceRepository()
        {
            Instance = new StudioResourceRepository();
        }

        private StudioResourceRepository()
        {
            ExplorerItemModels = new ObservableCollection<IExplorerItemModel>();
            ExplorerItemModelClone = a => a.Clone();
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

        internal StudioResourceRepository(IExplorerItem explorerItem, Guid environmentId, Action<System.Action, DispatcherPriority> invoke)
        {
            ExplorerItemModelClone = a => a.Clone();
            ExplorerItemModels = new ObservableCollection<IExplorerItemModel>();
            _invoke = invoke;

            if(explorerItem != null)
            {
                var environmentRepository = GetEnvironmentRepository();
                var explorerItems = MapData(explorerItem, environmentRepository, environmentId);
                LoadItemsToTree(environmentId, explorerItems);
            }
            Instance = this;
        }

        //This is for testing only need better way of putting this together
        internal StudioResourceRepository(ExplorerItemModel explorerItem, Action<System.Action, DispatcherPriority> invoke)
        {
            ExplorerItemModelClone = a => a.Clone();
            ExplorerItemModels = new ObservableCollection<IExplorerItemModel>();
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

        public Func<Guid, IClientExplorerResourceRepository> GetExplorerProxy = environmentId =>
        {
            var environmentModel = EnvironmentRepository.Instance.Get(environmentId);
            var connection = environmentModel.Connection;
            connection.ItemAddedMessageAction = Instance.ItemAddedMessageHandler;
            return new ServerExplorerClientProxy(connection);
        };

        public Func<Guid, IVersionRepository> GetVersionProxy = environmentId =>
        {
            var environmentModel = EnvironmentRepository.Instance.Get(environmentId);
            var connection = environmentModel.Connection;
            return new ServerExplorerVersionProxy(connection);
        };

        readonly object _syncRoot = new object();
        bool _isRegistered;

        #endregion

        #region Implementation of IStudioResourceRepository

        public ObservableCollection<IExplorerItemModel> ExplorerItemModels { get; private set; }

        public void Load(Guid environmentId, IAsyncWorker asyncWorker)
        {
            Load(environmentId, asyncWorker, id => { });
        }

        public void Load(Guid environmentId, IAsyncWorker asyncWorker, Action<Guid> onCompletion)
        {
            if(asyncWorker == null)
            {
                throw new ArgumentNullException("asyncWorker");
            }
            var environmentRepository = GetEnvironmentRepository();
            if(!_isRegistered)
            {
                _isRegistered = true;
            }
            // ReSharper disable ImplicitlyCapturedClosure
            IEnvironmentModel environmentModel = environmentRepository.FindSingle(c => c.ID == environmentId);
            // ReSharper restore ImplicitlyCapturedClosure
            if(environmentModel != null)
            {
                if(!environmentModel.IsConnected)
                {
                    // ReSharper disable ImplicitlyCapturedClosure
                    asyncWorker.Start(environmentModel.Connect, () => LoadEnvironmentTree(environmentId, onCompletion, environmentModel), e => onCompletion(environmentId));
                    // ReSharper restore ImplicitlyCapturedClosure
                }
                else
                {
                    asyncWorker.Start(()=>{}, () => LoadEnvironmentTree(environmentId, onCompletion, environmentModel), e => onCompletion(environmentId));
                }
            }
        }

        void LoadEnvironmentTree(Guid environmentId, Action<Guid> onCompletion, IEnvironmentModel environmentModel)
        {
            var explorerItemModel = LoadEnvironment(environmentId);
            environmentModel.LoadResources();
            LoadItemsToTree(environmentId, explorerItemModel);
            onCompletion(environmentModel.ID);
        }

        public void Disconnect(Guid environmentId)
        {
            var environment = ExplorerItemModels.FirstOrDefault(env => env.EnvironmentId == environmentId);

            var environmentRepository = GetEnvironmentRepository();
            IEnvironmentModel environmentModel = environmentRepository.FindSingle(c => c.ID == environmentId);
            if(environment != null)
            {
                environment.IsConnected = false;
                environment.Children = new ObservableCollection<IExplorerItemModel>();
            }
            if(environmentModel != null)
            {
                if(environmentModel.IsConnected)
                {
                    environmentModel.Disconnect();
                }
            }

        }

        public void Connect(Guid environmentId)
        {
            var environment = ExplorerItemModels.FirstOrDefault(env => env.EnvironmentId == environmentId);

            if(environment == null)
            {
                throw new Exception(string.Format("Environment Id : [{0}] was not found", environmentId));
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
            Dev2Logger.Log.Info(String.Format("Delete Item Resource: {0} Id:{1}", resourceId, environmentId));
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

        public void DeleteFolder(IExplorerItemModel item)
        {
            Dev2Logger.Log.Info(String.Format("Delete Folder Resource: {0} Id:{1}", item.DisplayName, item.EnvironmentId));
            VerifyArgument.IsNotNull("item", item);
            IExplorerItemModel parentItem = item.Parent;
            if(parentItem != null)
            {
                var found = parentItem.Children.FirstOrDefault(a => a.ResourcePath == item.ResourcePath && a.ResourceType == ResourceType.Folder);
                if (found != null) item = found;
                if( parentItem.Children.Remove(item))
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
        }

        public void UpdateRootAndFoldersPermissions(Permissions modifiedPermissions, Guid environmentGuid, bool updateRoot = true)
        {
            var server = FindItem(a => a.EnvironmentId == environmentGuid && a.ResourceType == ResourceType.Server);
            if(server != null)
            {
                // ReSharper disable MaximumChainedReferences
                server.Descendants().Where(a => a.ResourceType == ResourceType.Folder).ToList().ForEach(a =>
                    // ReSharper restore MaximumChainedReferences
                    {

                        a.Permissions = modifiedPermissions;
                    });
                if(updateRoot)
                    server.Permissions = modifiedPermissions;
            }

        }

        public void DeleteItem(IExplorerItemModel item)
        {
            VerifyArgument.IsNotNull("item", item);
            var tmpParent = item.Parent;
            if(item.Parent != null)
            {
                item.Parent.Children.Remove(item);
                tmpParent.OnChildrenChanged();
            }
        }

        public void AddServerNode(IExplorerItemModel explorerItem)
        {
            var otherServers = ExplorerItemModels.Where(i => i.ResourceType == ResourceType.Server).ToList();
            otherServers.ForEach(i =>
            {
                i.IsExplorerExpanded = false;
                i.IsExplorerSelected = false;
            });

            var exists = ExplorerItemModels.FirstOrDefault(i => i.EnvironmentId == explorerItem.EnvironmentId && i.ResourceType == ResourceType.Server);
            if(exists == null)
            {
                explorerItem.IsExplorerSelected = true;
                ExplorerItemModels.Add(explorerItem);
            }
            else
            {
                exists.IsExplorerExpanded = true;
                exists.IsExplorerSelected = true;
            }
        }

        public void RenameItem(IExplorerItemModel item, string newName)
        {
            VerifyArgument.IsNotNull("item", item);
            VerifyArgument.IsNotNullOrWhitespace("newName", newName);
            Dev2Logger.Log.Info(String.Format("Rename Item Resource: {0} New name :{1} Id:{2}", item.DisplayName, newName, item.EnvironmentId));
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

            RefreshVersionHistory(item.EnvironmentId, item.ResourceId);

            if(!item.Equals(explorerItemModel))
            {
                explorerItemModel.SetDisplay(newName);
            }
        }

        public void RenameFolder(IExplorerItemModel item, string newName)
        {


            VerifyArgument.IsNotNull("item", item);
            VerifyArgument.IsNotNullOrWhitespace("newName", newName);
            Dev2Logger.Log.Info(String.Format("Rename Folder Resource: {0} New name :{1} Id:{2}", item.DisplayName, newName, item.EnvironmentId));
            if(item.DisplayName == newName)
            {
                return;
            }
            var oldResourcePath = item.ResourcePath;
            var newPath = oldResourcePath.Replace(item.DisplayName, newName);
            var environmentId = item.EnvironmentId;
            var result = GetExplorerProxy(environmentId).RenameFolder(oldResourcePath, newPath, Guid.Empty);
            var explorerItemModel = LoadEnvironment(environmentId);
            LoadItemsToTree(environmentId, explorerItemModel);
            if(result.Status != ExecStatus.Success)
            {
                EventPublishers.Aggregator.Publish(new DisplayMessageBoxMessage("Error Renaming Folder", "Conflicting resources found in destination folder.", MessageBoxImage.Warning));
            }
        }
        public void MoveFolder(IExplorerItemModel item, string newName)
        {

            VerifyArgument.IsNotNull("item", item);
            VerifyArgument.IsNotNullOrWhitespace("newName", newName);
            Dev2Logger.Log.Info(String.Format("Move Folder Resource: {0} New name :{1} Id:{2}", item.DisplayName, newName, item.EnvironmentId));
            var oldResourcePath = item.ResourcePath;
            var newPath = newName;
            var environmentId = item.EnvironmentId;
            var result = GetExplorerProxy(environmentId).RenameFolder(oldResourcePath, newPath, Guid.Empty);
            var explorerItemModel = LoadEnvironment(environmentId);
            LoadItemsToTree(environmentId, explorerItemModel);
            if (result.Status != ExecStatus.Success)
            {
                EventPublishers.Aggregator.Publish(new DisplayMessageBoxMessage("Error Renaming Folder", "Conflicting resources found in destination folder.", MessageBoxImage.Warning));
            }
        }

        public void MoveItem(IExplorerItemModel model, string newPath)
        {
            VerifyArgument.IsNotNull("model", model);
            VerifyArgument.IsNotNull("newPath", newPath);
            if((model.ResourcePath==""?"": Path.GetDirectoryName(model.ResourcePath)) != newPath )
            switch (model.ResourceType)
            {
                case ResourceType.Folder:
                    MoveFolder(model, newPath+ (newPath==""?"":"\\")+model.DisplayName);
                    UpdateCategory(model, newPath + (newPath == "" ? "" : "\\") + model.DisplayName);
                    break;
                default:
                    MoveResource(model, newPath);
                    break;
            }
         

        }

        private void UpdateCategory(IExplorerItemModel model, string newPath)
        {
            
            foreach(var child in model.Children)
            {
                if (child.ResourceType == ResourceType.Folder)
                    UpdateCategory(child, newPath + "\\" + child.DisplayName);
                else
                child.UpdateCategoryIfOpened(newPath + (newPath == "" ? "" : "\\") + child.DisplayName);  

            }
        }



        void MoveResource(IExplorerItemModel model, string newPath)
        {
            if(model.ResourcePath == newPath +"\\"+model.DisplayName && model.ResourcePath != model.DisplayName)
            {
                return;
            }
            switch(model.ResourceType)
            {
                case ResourceType.Folder :
                    break;
                case ResourceType.Server :
                    break;
                case ResourceType.Version:
                    return;
                default :
                        model.Children = new ObservableCollection<IExplorerItemModel>();
                        break;
            }
            model.UpdateCategoryIfOpened(newPath + "\\" + model.DisplayName);
            var result = GetExplorerProxy(model.EnvironmentId).MoveItem(MapData(model), newPath, Guid.Empty);

            if(result.Status != ExecStatus.Success)
            {
                throw new Exception(result.Message);
            }
            model.Parent.RemoveChild(model);
            model.ResourcePath = newPath;
            ItemAddedMessageHandler(new ServerExplorerItem(model.DisplayName,model.ResourceId,model.ResourceType,null,model.Permissions,newPath+"\\"+model.DisplayName){ServerId = model.EnvironmentId});

            RefreshVersionHistory(model.EnvironmentId, model.ResourceId);
        }

        public void AddResouceItem(IContextualResourceModel resourceModel)
        {
            var explorerItemModel = new ServerExplorerItem { ResourcePath = resourceModel.Category, DisplayName = resourceModel.DisplayName, ResourceId = resourceModel.ID, Permissions = resourceModel.UserPermissions,ServerId = resourceModel.Environment.ID};
            ResourceType resourceType;
            Enum.TryParse(resourceModel.ServerResourceType, out resourceType);
            explorerItemModel.ResourceType = resourceType;
            ItemAddedMessageHandler(explorerItemModel);
        }

        public void AddItem(IExplorerItemModel item)
        {

            VerifyArgument.IsNotNull("item", item);
            VerifyArgument.IsNotNull("parent", item.Parent);
            Dev2Logger.Log.Info(String.Format("AddItem Name: {0} Type:{1}", item.DisplayName, item.ResourceType));
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

        public void UpdateItem(Guid id, Action<IExplorerItemModel> update, Guid environmentId)
        {
            VerifyArgument.IsNotNull("Update", update);
            var item = FindItemByIdAndEnvironment(id, environmentId);
            if(item != null)
                update(item);
        }

        public void ItemAddedMessageHandler(IExplorerItem item)
        {
            var environmentId = item.ServerId;
            var explorerItem = MapData(item, GetEnvironmentRepository(), environmentId);
            var resourcePath = item.ResourcePath.Replace("\\\\", "\\");

            if(!String.IsNullOrEmpty(resourcePath))
            {
                resourcePath = resourcePath.Equals(item.DisplayName) ? "" : resourcePath.Substring(0, resourcePath.LastIndexOf("\\" + item.DisplayName, StringComparison.Ordinal));
            }

            // ReSharper disable ImplicitlyCapturedClosure
            var parent = FindItem(model => model.ResourcePath != null && model.ResourcePath.Equals(resourcePath) && model.EnvironmentId == environmentId);
            var alreadyAdded = FindItem(model =>  model.ResourcePath == item.ResourcePath && model.EnvironmentId == environmentId) != null;
            // ReSharper restore ImplicitlyCapturedClosure
            var environmentModel = EnvironmentRepository.Instance.Get(environmentId);
            var resourceRepository = environmentModel.ResourceRepository;
            // ReSharper disable ImplicitlyCapturedClosure
            var resourceModel = resourceRepository.FindSingle(model => model.ID == item.ResourceId);
            // ReSharper restore ImplicitlyCapturedClosure
            if(resourceModel == null && item.ResourceType != ResourceType.Folder)
            {
                if(item.ResourceType == ResourceType.ServerSource)
                {
                    resourceRepository.ReloadResource(item.ResourceId, Studio.Core.AppResources.Enums.ResourceType.Source, ResourceModelEqualityComparer.Current, true);
                }
                else if(item.ResourceType >= ResourceType.DbSource)
                {
                    resourceRepository.ReloadResource(item.ResourceId, Studio.Core.AppResources.Enums.ResourceType.Source, ResourceModelEqualityComparer.Current, true);
                }
                else if(item.ResourceType >= ResourceType.DbService && item.ResourceType < ResourceType.DbSource )
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
                    if(parent != null && !alreadyAdded )
                    {
                        explorerItem.EnvironmentId = parent.EnvironmentId;
                        explorerItem.Parent = parent;
                        if(parent.Children == null)
                        {
                            parent.Children = new ObservableCollection<IExplorerItemModel>();
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

        public void PerformUpdateOnDispatcher(System.Action action)
        {
            _invoke(action, DispatcherPriority.Send);
        }

        static void AddChildItem(IExplorerItemModel parent, IExplorerItemModel explorerItem)
        {
            if(explorerItem != null)
            {
                if(explorerItem.ResourceId == Guid.Empty)
                {
                    parent.Children.Add(explorerItem);
                    parent.OnChildrenChanged();
                }
                else
                 {
                    var child = Instance.FindItem(i => i.ResourceId == explorerItem.ResourceId && i.Parent.ResourceId == parent.ResourceId && i.EnvironmentId == parent.EnvironmentId);
                    if(child == null)
                    {
                        parent.Children.Add(explorerItem);
                        parent.OnChildrenChanged();
                    }
                }
            }
        }

        public IExplorerItemModel FindItemById(Guid id)
        {
            return FindItem(model => model.ResourceId == id);
        }

        public Func<IExplorerItemModel, IExplorerItemModel> ExplorerItemModelClone { get; set; } 


        public IExplorerItemModel FindItemByIdAndEnvironment(Guid id, Guid environmentId)
        {
            return FindItem(model => model.ResourceId == id && model.EnvironmentId == environmentId);
        }

        public ObservableCollection<IExplorerItemModel> Filter(Func<IExplorerItemModel, bool> searchCriteria)
        {
            if(searchCriteria == null)
            {
                return ExplorerItemModels;
            }
            var filteredCollection = new ObservableCollection<IExplorerItemModel>();
            foreach(var explorerItemModel in ExplorerItemModels)
            {
                var cloned = ExplorerItemModelClone(explorerItemModel);
                var serverItem = FilterRec(searchCriteria, cloned);
                if(serverItem != null)
                {
                    filteredCollection.Add(serverItem);
                }
            }
            return filteredCollection;
        }

        public ObservableCollection<IExplorerItemModel> DialogFilter(Func<IExplorerItemModel, bool> searchCriteria)
        {
            if (searchCriteria == null)
            {
                return ExplorerItemModels;
            }
            var filteredCollection = new ObservableCollection<IExplorerItemModel>();
            foreach (var explorerItemModel in ExplorerItemModels)
            {
                var cloned = ExplorerItemModelClone(explorerItemModel);
                var serverItem = DialogFilterRec(searchCriteria, cloned);
                if (serverItem != null)
                {
                    filteredCollection.Add(serverItem);
                }
            }
            return filteredCollection;
        }
        private IExplorerItemModel DialogFilterRec(Func<IExplorerItemModel, bool> filter, IExplorerItemModel root, bool includeFolders = false)
        {
            if (filter == null)
            {
                return root;
            }
            var innerFilter = filter;
            if (includeFolders)
                innerFilter = a => filter(a) || a.ResourceType == ResourceType.Folder;
            root.Children = root.Children.Where(innerFilter).ToObservableCollection();
            foreach (var a in root.Children)
            {
                DialogFilterRec(filter, a);
            }
            return root;
        }

        private bool ContainsChild(Func<IExplorerItemModel, bool> filter, IExplorerItemModel root)
        {
            if (root.Children == null)
                return false;

            bool desc = root.Children.Any(filter);
            return desc || root.Children.Any(a => ContainsChild(filter, a));
        }

        private IExplorerItemModel FilterRec(Func<IExplorerItemModel, bool> filter, IExplorerItemModel root)
        {
            if (filter == null)
            {
                return root;
            }
            if (!ContainsChild(filter, root))
            {
                return null;
            }
            // ReSharper disable MaximumChainedReferences
            root.Children = root.Children.Where(a => FilterRec(filter, a) != null || filter(a)).ToObservableCollection();
            // ReSharper restore MaximumChainedReferences
            return root;
        }


        #endregion

        #region Private Members

        private void LoadItemsToTree(Guid environmentId, IExplorerItemModel explorerItemModel)
        {
            if(explorerItemModel != null)
            {
                var otherServers = ExplorerItemModels.Where(i => i.ResourceType == ResourceType.Server).ToList();
                otherServers.ForEach(i =>
                {
                    i.IsExplorerExpanded = false;
                    i.IsExplorerSelected = false;
                });

                explorerItemModel.IsExplorerSelected = explorerItemModel.IsExplorerExpanded = explorerItemModel.ResourceType == ResourceType.Server;

                explorerItemModel.EnvironmentId = environmentId;
                explorerItemModel.IsConnected = true;
                if(explorerItemModel.Children != null)
                {
                    ExplorerItemModelSetup(explorerItemModel, environmentId);
                }
                if(ExplorerItemModels.Any(a => a.EnvironmentId == environmentId))
                {
                    var index = ExplorerItemModels.IndexOf(ExplorerItemModels.FirstOrDefault(a => a.EnvironmentId == environmentId));
                    if (index >= 0)
                    {
                        explorerItemModel.IsRefreshing = ExplorerItemModels[index].IsRefreshing;
                        UpdateExplorerItemModelOnUiThread(explorerItemModel, index);
                    }
                }
                else
                {
                    var index = ExplorerItemModels.IndexOf(ExplorerItemModels.FirstOrDefault(a => a.EnvironmentId == environmentId));
                    if(index >= 0)
                    {
                        explorerItemModel.IsRefreshing = ExplorerItemModels[index].IsRefreshing;
                        UpdateExplorerItemModelOnUiThread(explorerItemModel, index);
                    }
                    else
                    {
                        Execute.OnUIThread(() =>
           {
               ExplorerItemModels.Add(explorerItemModel);
           });
                    }
                }
            }
        }

        void UpdateExplorerItemModelOnUiThread(IExplorerItemModel explorerItemModel, int index)
        {
            Execute.OnUIThread(() =>
            {
                ExplorerItemModels.RemoveAt(index);
                ExplorerItemModels.Insert(index, explorerItemModel);
            });
        }

        private void LoadItemsToTree(Guid environmentId, IExplorerItemModel explorerItemModel, int indexToReplace)
        {
            if(explorerItemModel != null)
            {
                explorerItemModel.EnvironmentId = environmentId;
                explorerItemModel.IsConnected = true;
                if(explorerItemModel.Children != null)
                {
                    ExplorerItemModelSetup(explorerItemModel, environmentId);
                }
                explorerItemModel.IsRefreshing = ExplorerItemModels[indexToReplace].IsRefreshing;
                ExplorerItemModels.RemoveAt(indexToReplace);
                ExplorerItemModels.Insert(indexToReplace, explorerItemModel);
            }
        }

        private void ExplorerItemModelSetup(IExplorerItemModel explorerItemModel, Guid enviromentId)
        {
            if(explorerItemModel.Children != null)
            {
                foreach(var child in explorerItemModel.Children)
                {
                    child.Parent = explorerItemModel;
                    child.EnvironmentId = enviromentId;
                    ExplorerItemModelSetup(child, enviromentId);
                }
            }
        }

        private IExplorerItemModel LoadEnvironment(Guid environmentId)
        {
            var explorerResourceRepository = GetExplorerProxy(environmentId);
            var explorerItem = explorerResourceRepository.Load(environmentId);
            return MapData(explorerItem, GetEnvironmentRepository(), environmentId);
        }

        private IExplorerItemModel MapData(IExplorerItem item, IEnvironmentRepository environmentRepository, Guid environmentId)
        {
            if(item == null)
            {
                return null;
            }
            string displayname = item.DisplayName;
            // ReSharper disable ImplicitlyCapturedClosure
            // ReSharper disable RedundantAssignment
            IEnvironmentModel environmentModel = environmentRepository.FindSingle(model => GetEnvironmentModel(model, item, environmentId));
            if((item.ResourceType == ResourceType.Server && environmentId != Guid.Empty) || (environmentId == Guid.Empty && displayname.ToLower() == Environment.MachineName.ToLower()))
            {
                environmentModel = environmentRepository.FindSingle(model => GetEnvironmentModel(model, item, environmentId));
                if(environmentModel != null && environmentModel.Connection != null)
                {
                    displayname = environmentModel.DisplayName;
                }
            }

            return new ExplorerItemModel
            {
                Children = item.Children == null ? new ObservableCollection<IExplorerItemModel>() : new ObservableCollection<IExplorerItemModel>(item.Children.Select(i => MapData(i, environmentRepository, environmentId))),
                DisplayName = displayname,
                ResourceType = item.ResourceType,
                ResourceId = item.ResourceId,
                Permissions = item.Permissions,
                ResourcePath = item.ResourcePath,
                VersionInfo = item.VersionInfo
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

        private static IExplorerItem MapData(IExplorerItemModel item)
        {
            return new ServerExplorerItem(item.DisplayName, item.ResourceId, item.ResourceType,
                                          item.Children == null
                                              ? null
                                              : item.Children.Select(MapData).ToList(),
                                          item.Permissions, item.ResourcePath);

        }

        #endregion

        public IExplorerItemModel FindItem(Func<IExplorerItemModel, bool> searchCriteria)
        {
            var explorerItemModels = ExplorerItemModels.SelectMany(explorerItemModel => explorerItemModel.Descendants()).ToList();
            return searchCriteria == null ? null : explorerItemModels.FirstOrDefault(searchCriteria);
        }

        public void ShowVersionHistory(Guid environmentId, Guid resourceId)
        {
            var versionProxy = GetVersionProxy(environmentId);
            var versions = versionProxy.GetVersions(resourceId);
            AttachVersionHistoryToParent(environmentId, resourceId, versions);
        }

        public void HideVersionHistory(Guid environmentId, Guid resourceId)
        {
            var parent = FindItem(i => i.ResourceId == resourceId && i.EnvironmentId == environmentId);

            if(parent != null)
            {
                parent.Children.Clear();
            }
        }

        public StringBuilder GetVersion(IVersionInfo versionInfo, Guid environmentId)
        {
            VerifyArgument.IsNotNull("versionInfo", versionInfo);
            var versionProxy = GetVersionProxy(environmentId);
            return versionProxy.GetVersion(versionInfo);
        }

        public void RollbackTo(IVersionInfo versionInfo, Guid environmentId)
        {

            VerifyArgument.IsNotNull("versionInfo", versionInfo);
            Dev2Logger.Log.Info(String.Format("Rollback Version Resource: {0} Version:{1}", versionInfo.ResourceId, versionInfo.VersionNumber));
            var resourceId = versionInfo.ResourceId;
            var versionProxy = GetVersionProxy(environmentId);
            IRollbackResult rollbackResult = versionProxy.RollbackTo(resourceId, versionInfo.VersionNumber);

            if(rollbackResult != null)
            {
                var parent = FindItem(i => i.ResourceId == resourceId && i.EnvironmentId == environmentId);
                if(parent != null && parent.DisplayName != rollbackResult.DisplayName)
                {
                    parent.RefreshName(rollbackResult.DisplayName);
                }
                AttachVersionHistoryToParent(environmentId, resourceId, rollbackResult.VersionHistory);
            }
        }

        public void DeleteVersion(IVersionInfo versionInfo, Guid environmentId)
        {

            VerifyArgument.IsNotNull("versionInfo", versionInfo);
            Dev2Logger.Log.Info(String.Format("Delete Version. Resource: {0} Version:{1}", versionInfo.ResourceId, versionInfo.VersionNumber));
            var resourceId = versionInfo.ResourceId;
            var versionProxy = GetVersionProxy(environmentId);
            var versions = versionProxy.DeleteVersion(resourceId, versionInfo.VersionNumber);
            AttachVersionHistoryToParent(environmentId, resourceId, versions);
        }

        void AttachVersionHistoryToParent(Guid environmentId, Guid resourceId, IList<IExplorerItem> versions)
        {
            ObservableCollection<IExplorerItemModel> explorerItemModels;
            // ReSharper disable ImplicitlyCapturedClosure
            var parent = FindItem(i => i.ResourceId == resourceId && i.EnvironmentId == environmentId);
            // ReSharper restore ImplicitlyCapturedClosure

            if(versions == null || versions.Count == 0)
            {
                explorerItemModels = new ObservableCollection<IExplorerItemModel>();
            }
            else
            {
                // ReSharper disable ImplicitlyCapturedClosure
                explorerItemModels = versions.Select(version => MapData(version, GetEnvironmentRepository(), environmentId)).ToObservableCollection();
                explorerItemModels.ForEach(e =>
                    {
                        e.EnvironmentId = environmentId;
                        e.Parent = parent;
                        e.ResourceId = GenerateVersionResourceId(resourceId, environmentId, e.VersionInfo.VersionNumber); //Note this will need to be relooked for execution
                    });
            }
            // ReSharper restore ImplicitlyCapturedClosure

            if(parent != null)
            {
                if(explorerItemModels.Count == 0)
                {
                    parent.Children = new ObservableCollection<IExplorerItemModel>{
                    new ExplorerItemModel
                    {
                        DisplayName = "There is no version history to display",
                        ResourceType = ResourceType.Message,
                        Permissions = Permissions.View
                    }};
                }
                else
                {
                    parent.Children = explorerItemModels;
                }
                parent.IsExplorerExpanded = true;
            }
        }

        readonly Dictionary<string, Guid> _versionKeys = new Dictionary<string, Guid>();

        private Guid GenerateVersionResourceId(Guid resourceId, Guid environmentId, string versionNumber)
        {
            var key = string.Format("{0}{1}{2}", resourceId, environmentId, versionNumber);

            if(_versionKeys.ContainsKey(key))
            {
                return _versionKeys[key];
            }

            var newKey = Guid.NewGuid();
            _versionKeys.Add(key, newKey);
            return newKey;
        }

        public void RefreshVersionHistory(Guid environmentId, Guid resourceId)
        {
            var parent = FindItem(i => i.ResourceId == resourceId && i.EnvironmentId == environmentId);
            if(parent != null && parent.Children.Count > 0)
            {
                ShowVersionHistory(environmentId, resourceId);
            }
        }

        public string GetServerVersion(Guid environmentId)
        {
            try
            {
                return GetExplorerProxy(environmentId).GetServerVersion();
            }
            catch (Exception)
            {

                return "Less than 0.4.19.1";
            }
        }
    }
}
