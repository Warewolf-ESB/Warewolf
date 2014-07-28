using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Models;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Dev2.ViewModels.Deploy
{
    public abstract class NavigationViewModelBase : SimpleBaseViewModel,
                            IHandle<UpdateResourceMessage>
    {
        protected internal IAsyncWorker AsyncWorker;
        string _searchFilter = string.Empty;
        protected internal IEventAggregator EventAggregator;
        Visibility _circularProgressBarVisibility;
        Visibility _refreshButtonVisibility;

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

        public event EventHandler LoadResourcesCompleted;
        public IEnvironmentRepository EnvironmentRepository { get; set; }

        public Visibility RefreshButtonVisibility
        {
            get
            {
                return _refreshButtonVisibility;
            }
            set
            {
                if(value == _refreshButtonVisibility)
                {
                    return;
                }
                _refreshButtonVisibility = value;
                NotifyOfPropertyChange("RefreshButtonVisibility");
            }
        }

        public Visibility CircularProgressBarVisibility
        {
            get
            {
                return _circularProgressBarVisibility;
            }
            set
            {
                if(value == _circularProgressBarVisibility)
                {
                    return;
                }
                _circularProgressBarVisibility = value;
                NotifyOfPropertyChange("CircularProgressBarVisibility");
            }
        }

        public IStudioResourceRepository StudioResourceRepository
        {
            get;
            private set;
        }

        public void LoadEnvironmentResources(IEnvironmentModel environment)
        {
            this.Warning("Navigation Resources Load - Start");
            LoadResourcesAsync(environment);
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

        internal Task LoadResourcesAsync(IEnvironmentModel environment, List<ExplorerItemModel> expandedList = null, ExplorerItemModel selectedItem = null)
        {
            Task task = null;
            if(AsyncWorker != null)
            {
                task = AsyncWorker.Start(() =>
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
                            UpdateSearchFilter(_searchFilter);
                            SetTreeStateBack(expandedList, selectedItem);
                        }
                    }
                    catch(Exception ex)
                    {
                        this.LogError(ex);
                    }
                    finally
                    {
                        OnLoadResourcesCompleted();
                        environment.RaiseResourcesLoaded();

                    }
                });
            }
            return task;
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
                    worker.DoWork += (s, e) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new System.Action(() => DoFiltering(searhFilter)));
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
            IStudioResourceRepository studioResourceRepository = StudioResourceRepository;
            ExplorerItemModel item = studioResourceRepository.FindItemByIdAndEnvironment(resourceId, environmentId);
            if(item != null)
            {
                item.IsExplorerSelected = true;
                RecusiveExplorerExpandParent(item.Parent);
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