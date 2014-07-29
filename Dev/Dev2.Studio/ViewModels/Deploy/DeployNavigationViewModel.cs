using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Models;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dev2.ViewModels.Deploy
{
    public class DeployNavigationViewModel : NavigationViewModelBase
    {
        IEnvironmentModel _environment;
        private readonly bool _target;
        public IAuthorizationService AuthorizationService { get; private set; }
        ObservableCollection<ExplorerItemModel> _explorerItemModels;
        public DeployNavigationViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository, IStudioResourceRepository studioResourceRepository, bool target)
            : base(eventPublisher, asyncWorker, environmentRepository, studioResourceRepository)
        {
            _target = target;

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
                }
            }
        }


        public ExplorerItemModel FindChild(IContextualResourceModel resource)
        {
            var explorerItemModels = ExplorerItemModels.SelectMany(explorerItemModel => explorerItemModel.Descendants()).ToList();
            return resource != null ? explorerItemModels.FirstOrDefault(model => model.ResourceId == resource.ID && model.EnvironmentId == resource.Environment.ID) : null;
        }

        public void Filter(Func<ExplorerItemModel, bool> filter, bool fromFilter = false)
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

            foreach(ExplorerItemModel explorerItemModel in ExplorerItemModels)
            {
                explorerItemModel.IsExplorerExpanded = true;
            }
        }

        /// <summary>
        /// perform some kind of action on all children of a node
        /// </summary>
        /// <param name="action"></param>
        protected void Iterate(Action<ExplorerItemModel> action)
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
        void Iterate(Action<ExplorerItemModel> action, ExplorerItemModel node)
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
        public virtual ObservableCollection<ExplorerItemModel> ExplorerItemModels
        {
            get
            {
                return _explorerItemModels ?? new ObservableCollection<ExplorerItemModel>();
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

        private void FilterEnvironments(IEnvironmentModel connection)
        {
            if(connection != null)
            {
                var isAuthorizedDeployTo = AuthorizationService.IsAuthorized(AuthorizationContext.DeployTo, Guid.Empty.ToString());
                if(isAuthorizedDeployTo && _target)
                {
                    ObservableCollection<ExplorerItemModel> explorerItemModels = new ObservableCollection<ExplorerItemModel>
                        {
                            StudioResourceRepository.FindItem(env => env.EnvironmentId == connection.ID)
                        };

                    ExplorerItemModels.Clear();
                    ExplorerItemModels = explorerItemModels;

                    if(ExplorerItemModels != null && ExplorerItemModels.Count == 1 && ExplorerItemModels[0] != null)
                    {
                        ExplorerItemModels[0].IsDeployTargetExpanded = true;
                    }
                }
                else
                {
                    var isAuthorizedDeployFrom = AuthorizationService.IsAuthorized(AuthorizationContext.DeployFrom, Guid.Empty.ToString());
                    if(isAuthorizedDeployFrom && !_target)
                    {
                        var explorerItemModels = new ObservableCollection<ExplorerItemModel>
                            {
                                StudioResourceRepository.FindItem(env => env.EnvironmentId == connection.ID)
                            };

                        ExplorerItemModels.Clear();
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
                                StudioResourceRepository.PerformUpdateOnDispatcher(() => model[0].Children = new ObservableCollection<ExplorerItemModel>());
                                var resourcePermissions = AuthorizationService.GetResourcePermissions(Guid.Empty);
                                model[0].Permissions = resourcePermissions;
                            }
                            ExplorerItemModels = model;
                        }
                    }
                }
                Iterate(model => model.IsChecked = false);
                Iterate(model => model.IsOverwrite = false);

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

        public void RefreshEnvironment()
        {
            StudioResourceRepository.Load(_environment.ID, AsyncWorker);
            FilterEnvironments(_environment);
        }
    }
}
