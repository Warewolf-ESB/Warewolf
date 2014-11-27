
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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common;
using Dev2.Models;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Threading;

namespace Dev2.ViewModels.Deploy
{
    public abstract class NavigationViewModelBase : SimpleBaseViewModel,
                            IHandle<UpdateResourceMessage>
    {
        protected internal IAsyncWorker AsyncWorker;
        public readonly System.Action UpdateWorkSpaceItems;
        string _searchFilter = string.Empty;
        protected internal IEventAggregator EventAggregator;
        Visibility _circularProgressBarVisibility;
        Visibility _refreshButtonVisibility;
        IStudioResourceRepository _studioResourceRepository;

        protected NavigationViewModelBase(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository, IStudioResourceRepository studioResourceRepository, System.Action updateWorkSpaceItems)
            : this(eventPublisher, asyncWorker, environmentRepository, studioResourceRepository)
        {
            VerifyArgument.IsNotNull("updateWorkSpaceItems", updateWorkSpaceItems);
            UpdateWorkSpaceItems = updateWorkSpaceItems;
        }

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

        // ReSharper disable once ConvertToAutoProperty
        public IStudioResourceRepository StudioResourceRepository
        {
            get
            {
                return _studioResourceRepository;
            }
            private set
            {
                _studioResourceRepository = value;
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

        internal Task LoadResourcesAsync(IEnvironmentModel environment, List<IExplorerItemModel> expandedList = null, IExplorerItemModel selectedItem = null)
        {
            Task task = null;
            if(AsyncWorker != null)
            {
// ReSharper disable ImplicitlyCapturedClosure
                task = AsyncWorker.Start(() =>
// ReSharper restore ImplicitlyCapturedClosure
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
                            UpdateNavigationView(expandedList, selectedItem);
                        }
                    }
                    catch(Exception ex)
                    {
                        Dev2Logger.Log.Error(ex);
                    }
                    finally
                    {
                        if(UpdateWorkSpaceItems != null)
                        {
                            UpdateWorkSpaceItems();
                        }
                        environment.RaiseResourcesLoaded();

                    }
                });
            }
            return task;
        }

        protected void UpdateNavigationView(IEnumerable<IExplorerItemModel> expandedList = null, IExplorerItemModel selectedItem = null)
        {
            UpdateSearchFilter(_searchFilter);
            SetTreeStateBack(expandedList, selectedItem);
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

        /// <summary>
        ///     Called to filter the root treendode
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2/25/2013</date>
        public void UpdateSearchFilter()
        {

   
                DoFiltering(_searchFilter);
            
        }

        protected abstract void DoFiltering(string searhFilter);

        void SetTreeStateBack(IEnumerable<IExplorerItemModel> expandedList, IExplorerItemModel selectedItem)
        {
            if(expandedList != null)
            {
                IStudioResourceRepository studioResourceRepository = StudioResourceRepository;
                foreach(var item in expandedList)
                {
                    var environment = studioResourceRepository.FindItemById(item.EnvironmentId);
                    if(environment != null)
                    {
                        IExplorerItemModel explorerItem = environment;
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

        public void BringItemIntoView(Guid environmentId, Guid resourceId)
        {
            IStudioResourceRepository studioResourceRepository = StudioResourceRepository;
            IExplorerItemModel item = studioResourceRepository.FindItemByIdAndEnvironment(resourceId, environmentId);
            if(item != null)
            {
                item.IsExplorerSelected = true;
                RecusiveExplorerExpandParent(item.Parent);
            }
        }

        public void RecusiveExplorerExpandParent(IExplorerItemModel parent)
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

        protected IExplorerItemModel TryGetResourceNode(IContextualResourceModel resourceModel)
        {
            return StudioResourceRepository.FindItemByIdAndEnvironment(resourceModel.ID, resourceModel.Environment.ID);
        }
    }
}
