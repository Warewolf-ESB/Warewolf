using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Data.ServiceModel;
using Dev2.Models;
using Dev2.Network;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Threading;

namespace Dev2.ViewModels.Deploy
{
    public abstract class NavigationViewModelBase : SimpleBaseViewModel,
                            IHandle<UpdateResourceMessage>,
                            IHandle<EnvironmentConnectedMessage>,
                            IHandle<EnvironmentDisconnectedMessage>
    {
        protected internal IAsyncWorker AsyncWorker;
        string _searchFilter = string.Empty;
        bool _isRefreshing;
        protected internal IEventAggregator EventAggregator;
        protected NavigationViewModelBase(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository, IStudioResourceRepository studioResourceRepository)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);

            AsyncWorker = asyncWorker;
            EventAggregator = eventPublisher;
            EventAggregator.Subscribe(this);
            EnvironmentRepository = environmentRepository;
            StudioResourceRepository = studioResourceRepository;
        }

        public abstract void Handle(EnvironmentConnectedMessage message);
        public abstract void Handle(EnvironmentDisconnectedMessage message);

        public event EventHandler LoadResourcesCompleted;
        public IEnvironmentRepository EnvironmentRepository { get; set; }
        //        public virtual ObservableCollection<ExplorerItemModel> ExplorerItemModels
        //        {
        //            get
        //            {
        //                return _explorerItemModels??new ObservableCollection<ExplorerItemModel>();
        //            }
        //            set
        //            {
        //                if(Equals(value, _explorerItemModels))
        //                {
        //                    return;
        //                }
        //                _explorerItemModels = value;
        //                OnPropertyChanged("ExplorerItemModels");
        //            }
        //        }
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged("IsRefreshing");
            }
        }
        public IStudioResourceRepository StudioResourceRepository
        {
            get;
            private set;
        }

        public void LoadEnvironmentResources(IEnvironmentModel environment, List<ExplorerItemModel> expandedList = null, ExplorerItemModel selectedItem = null)
        {
            this.Warning("Navigation Resources Load - Start");
            LoadResourcesAsync(environment, expandedList, selectedItem);
        }

        void UpdateIsRefreshing(IEnvironmentModel environment, bool isRefreshing)
        {
            IsRefreshing = isRefreshing;

            var environmentTreeViewModel = Find(environment);
            if(environmentTreeViewModel != null)
            {
                environmentTreeViewModel.IsRefreshing = isRefreshing;
            }
        }

        /// <summary>
        /// Connects to a server considering the auxilliry connection field.
        /// </summary>
        /// <param name="environment">The environment.</param>
        void Connect(IEnvironmentModel environment)
        {
            if(environment.IsConnected)
            {
                return;
            }
            environment.Connect();
        }

        internal async void LoadResourcesAsync(IEnvironmentModel environment, List<ExplorerItemModel> expandedList = null, ExplorerItemModel selectedItem = null)
        {
            if(environment == null)
            {
                return;
            }

            UpdateIsRefreshing(environment, true);

            if(AsyncWorker != null && !ServerProxy.IsShuttingDown)
            {
                await AsyncWorker.Start(() =>
                {
                    if(!environment.IsConnected)
                    {
                        Connect(environment);
                    }
                    environment.LoadResources();

                }, () =>
                {
                    try
                    {
                        if(environment.IsConnected && environment.CanStudioExecute)
                        {
                            StudioResourceRepository.Load(environment.ID, AsyncWorker);
                            UpdateSearchFilter(_searchFilter);
                            SetTreeStateBack(expandedList, selectedItem);
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger.LogError(this, ex);
                    }
                    finally
                    {
                        UpdateIsRefreshing(environment, false);
                        OnLoadResourcesCompleted();
                        environment.RaiseResourcesLoaded();

                    }
                });
            }
        }

        /// <summary>
        ///     Called to filter the root treendode
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2/25/2013</date>
        public void UpdateSearchFilter(string searhFilter)
        {
            _searchFilter = searhFilter;
            if(Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess())
            {
                try
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += (s, e) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Action(() => DoFiltering(searhFilter)));
                    worker.RunWorkerAsync();
                }
                catch(Exception)
                {
                    DoFiltering(searhFilter);
                }
            }
            else
            {
                DoFiltering(searhFilter);
            }
        }

        protected abstract void DoFiltering(string searhFilter);

        void SetTreeStateBack(IEnumerable<ExplorerItemModel> expandedList, ExplorerItemModel selectedItem)
        {
            if(expandedList != null)
            {
                IStudioResourceRepository studioResourceRepository = StudioResourceRepository;
                foreach(var item in expandedList)
                {
                    var environment = studioResourceRepository.FindItemById(item.EnvironmentId);
                    if(environment != null)
                    {
                        ExplorerItemModel explorerItem = environment;
                        if(!string.IsNullOrEmpty(item.ResourcePath))
                        {
                            var strings = item.ResourcePath.Split('\\');
                            foreach(var s in strings)
                            {
                                if(explorerItem != null)
                                {
                                    explorerItem = explorerItem.Children.FirstOrDefault(c => c.DisplayName == s);
                                }
                            }
                        }
                        if(explorerItem != null)
                        {
                            explorerItem.IsExplorerExpanded = true;
                        }
                    }
                }
            }

            if(selectedItem != null)
            {
                BringItemIntoView(selectedItem.EnvironmentId, selectedItem.ResourceId);
            }
        }

        void OnLoadResourcesCompleted()
        {
            this.Warning("Navigation Resources Load - End");
            if(LoadResourcesCompleted != null)
            {
                LoadResourcesCompleted(this, EventArgs.Empty);
            }
        }

        public void BringItemIntoView(Guid environmentId, Guid resourceId)
        {
            //if(ExplorerItemModels != null && ExplorerItemModels.Count > 0)
            {
                IStudioResourceRepository studioResourceRepository = StudioResourceRepository;
                var environment = studioResourceRepository.FindItemById(environmentId);
                if(environment != null)
                {
                    ExplorerItemModel item = studioResourceRepository.FindItemById(resourceId);
                    if(item != null)
                    {
                        item.IsExplorerSelected = true;
                        RecusiveExplorerExpandParent(item.Parent);
                    }
                }
            }
        }

        public void RecusiveExplorerExpandParent(ExplorerItemModel parent)
        {
            if(parent != null)
            {
                if(parent.Parent != null)
                {
                    RecusiveExplorerExpandParent(parent.Parent);
                }
                parent.IsExplorerExpanded = true;
            }
        }

        /// <summary>
        /// Returns the node which represents an environment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="createIfMissing">if set to <c>true</c> [create if missing].</param>
        /// <returns></returns>
        public ExplorerItemModel Find(IEnvironmentModel environment, bool createIfMissing = false)
        {
            ExplorerItemModel returnNavigationItemViewModel = StudioResourceRepository.FindItem(vm => environment.ID == vm.EnvironmentId && vm.ResourceType == ResourceType.Server);

            if(returnNavigationItemViewModel == null && createIfMissing)
            {
                StudioResourceRepository.Connect(environment.ID);
                returnNavigationItemViewModel = StudioResourceRepository.FindItem(vm => environment.ID == vm.EnvironmentId);
            }

            return returnNavigationItemViewModel;
        }



        /// <summary>
        /// Updates an item with in the current NavigationItemViewModel graph
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void UpdateResource(IContextualResourceModel resource)
        {
            UpdateSearchFilter(_searchFilter);
        }





        /// <summary>
        /// Handles the specified UpdateResourcemessage by updating the resource
        /// </summary>
        /// <param name="message">The message.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void Handle(UpdateResourceMessage message)
        {
            UpdateResource(message.ResourceModel);
        }




        protected ExplorerItemModel TryGetResourceNode(IContextualResourceModel resourceModel)
        {
            return StudioResourceRepository.FindItemByIdAndEnvironment(resourceModel.ID, resourceModel.Environment.ID);
        }
    }
}