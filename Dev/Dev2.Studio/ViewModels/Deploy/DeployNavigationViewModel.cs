
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.ConnectionHelpers;
using Dev2.Models;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;

namespace Dev2.ViewModels.Deploy
{
    public class DeployNavigationViewModel : NavigationViewModelBase
    {
        IEnvironmentModel _environment;
        private readonly bool _target;
        public IAuthorizationService AuthorizationService
        {
            get
            {
                return _authorizationService;
            }
            private set
            {
                _authorizationService = value;
                if(_authorizationService != null)
                {
                    _authorizationService.PermissionsChanged += AuthorizationServiceOnPermissionsModified;
                }
            }
        }
        ObservableCollection<IExplorerItemModel> _explorerItemModels;
        IAuthorizationService _authorizationService;
        ICommand _refreshMenuCommand;
        IConnectControlSingleton _connectControlSingleton;

        public DeployNavigationViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository, IStudioResourceRepository studioResourceRepository, bool target, IConnectControlSingleton connectControlSingleton)
            : base(eventPublisher, asyncWorker, environmentRepository, studioResourceRepository)
        {
            _target = target;
            ConnectControlSingleton = connectControlSingleton;
        }

        public IEnvironmentModel Environment
        {
            get
            {
                return _environment;
            }
            set
            {
                _environment = value;
                if(value != null)
                {
                    AuthorizationService = value.AuthorizationService;
                }
                if(_environment != null)
                {
                    FilterEnvironments(_environment);
                    _environment.AuthorizationServiceSet += (sender, args) =>
                    {
                        AuthorizationService = _environment.AuthorizationService;

                    };
                }
            }
        }

        void AuthorizationServiceOnPermissionsModified(object sender, EventArgs eventArgs)
        {
            FilterEnvironments(_environment);
        }

        public IExplorerItemModel FindChild(IContextualResourceModel resource)
        {
            var explorerItemModels = ExplorerItemModels.SelectMany(explorerItemModel => explorerItemModel.Descendants()).ToList();
            return resource != null ? explorerItemModels.FirstOrDefault(model => model.ResourceId == resource.ID && model.EnvironmentId == resource.Environment.ID) : null;
        }

        public void Filter(Func<IExplorerItemModel, bool> filter, bool fromFilter = false)
        {
            if(filter == null)
            {
                return;
            }

            ExplorerItemModels = StudioResourceRepository.Filter(filter);
            if(fromFilter)
            {
                Iterate(model => model.IsExplorerExpanded = true);
            }

            foreach(var explorerItemModel in ExplorerItemModels)
            {
                explorerItemModel.IsExplorerExpanded = true;
            }
        }
        public void Update()
        {
            
            StudioResourceRepository.Load(_environment.ID, AsyncWorker);
            FilterEnvironments(Environment, false);

        }
        /// <summary>
        /// perform some kind of action on all children of a node
        /// </summary>
        /// <param name="action"></param>
        protected void Iterate(Action<IExplorerItemModel> action)
        {
            if(ExplorerItemModels != null && action != null)
            {
                var explorerItemModels = ExplorerItemModels.ToList();
                explorerItemModels.ForEach(model =>
                {
                    if(model != null)
                    {
                        Iterate(action, model);
                    }
                });
            }
        }

        /// <summary>
        /// perform some kind of action on all children of a node. this can be moved onto the tree node interface if it is found to be needed elsewhere
        /// </summary>
        /// <param name="action"></param>
        /// <param name="node"></param>
        void Iterate(Action<IExplorerItemModel> action, IExplorerItemModel node)
        {
            if(node != null)
            {
                action(node);
                if(node.Children != null)
                {
                    foreach(var child in node.Children)
                    {
                        Iterate(action, child);
                    }
                }
            }
        }
        public virtual ObservableCollection<IExplorerItemModel> ExplorerItemModels
        {
            get
            {
                return _explorerItemModels ?? new ObservableCollection<IExplorerItemModel>();
            }
            set
            {
                if(Equals(value, _explorerItemModels))
                {
                    return;
                }
                _explorerItemModels = value;
                OnPropertyChanged("ExplorerItemModels");
            }
        }

        private void FilterEnvironments(IEnvironmentModel connection, bool clearSelections = true)
        {
            if(connection != null)
            {
                if(Application.Current != null && Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() => ExplorerItemModels.Clear());

                }
                else
                {
                    ExplorerItemModels.Clear();
                }

                var isAuthorizedDeployTo = AuthorizationService != null && AuthorizationService.IsAuthorized(AuthorizationContext.DeployTo, Guid.Empty.ToString());
                if(isAuthorizedDeployTo && _target)
                {
                    ObservableCollection<IExplorerItemModel> explorerItemModels = new ObservableCollection<IExplorerItemModel>
                        {
                            StudioResourceRepository.FindItem(env => env.EnvironmentId == connection.ID)
                        };

                    ExplorerItemModels = explorerItemModels;

                    if(ExplorerItemModels != null && ExplorerItemModels.Count == 1 && ExplorerItemModels[0] != null)
                    {
                        ExplorerItemModels[0].IsDeployTargetExpanded = true;
                    }
                }
                else
                {
                    var isAuthorizedDeployFrom = AuthorizationService != null && AuthorizationService.IsAuthorized(AuthorizationContext.DeployFrom, Guid.Empty.ToString());
                    if(isAuthorizedDeployFrom && !_target)
                    {
                        var explorerItemModels = new ObservableCollection<IExplorerItemModel>
                            {
                                StudioResourceRepository.FindItem(env => env.EnvironmentId == connection.ID)
                            };

                        ExplorerItemModels = explorerItemModels;

                        if(ExplorerItemModels != null && ExplorerItemModels.Count == 1 && ExplorerItemModels[0] != null)
                        {
                            ExplorerItemModels[0].IsDeploySourceExpanded = true;
                        }
                    }
                    else
                    {

                        var model = StudioResourceRepository.Filter(env => env.EnvironmentId == connection.ID);
                        if(model != null)
                        {
                            if(model.Count == 1)
                            {
                                StudioResourceRepository.PerformUpdateOnDispatcher(() => model[0].Children = new ObservableCollection<IExplorerItemModel>());
                                if(AuthorizationService != null)
                                {
                                    var resourcePermissions = AuthorizationService.GetResourcePermissions(Guid.Empty);
                                    model[0].Permissions = resourcePermissions;
                                }
                            }
                            ExplorerItemModels = model;
                        }
                    }
                }
                if(clearSelections)
                {
                    Iterate(model => model.IsChecked = false);
                    Iterate(model => model.IsOverwrite = false);
                }

            }
        }

        public virtual void ClearConflictingNodesNodes()
        {
            Iterate((node =>
            {
                node.IsOverwrite = false;
            }));
        }

        protected override void DoFiltering(string searhFilter)
        {
            if(!string.IsNullOrEmpty(searhFilter))
            {
                Filter(model => model.DisplayName.ToLower().Contains(searhFilter.ToLower()) && model.EnvironmentId == Environment.ID);
            }
            else
            {
                Filter(model => model.EnvironmentId == Environment.ID);
            }
        }

        public bool SetNodeOverwrite(IContextualResourceModel resource, bool state)
        {
            if(resource != null && !resource.IsNewWorkflow && Environment != null)
            {
                IEnvironmentModel env = Environment;

                var resModel = env.ResourceRepository.All()
                                    .FirstOrDefault(r => ResourceModelEqualityComparer
                                    .Current.Equals(r, resource));
                if(resModel != null)
                {
                    var child = TryGetResourceNode(resModel as IContextualResourceModel);
                    if(child != null)
                    {
                        if(child.Parent != null)
                        {
                            child.Parent.IsOverwrite = state;
                        }
                        return child.IsOverwrite = state;
                    }
                }
            }
            return false;
        }
        public ICommand RefreshMenuCommand
        {
            get
            {
                return _refreshMenuCommand ??
                       (_refreshMenuCommand = new DelegateCommand(param => RefreshConnectControl()));
            }
        }
        public void RefreshConnectControl()
        {
            ConnectControlSingleton.Refresh(_environment.ID);
        }

        // ReSharper disable ConvertToAutoProperty
        public IConnectControlSingleton ConnectControlSingleton
            // ReSharper restore ConvertToAutoProperty
        {
            get
            {
                return _connectControlSingleton;
            }
            private set
            {
                _connectControlSingleton = value;
            }
        }

        public void RefreshEnvironment()
        {
            
            StudioResourceRepository.Load(_environment.ID, AsyncWorker);
            FilterEnvironments(_environment);
        }


    }
}
