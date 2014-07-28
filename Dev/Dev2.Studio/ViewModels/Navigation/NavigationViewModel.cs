using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.ConnectionHelpers;
using Dev2.Data.ServiceModel;
using Dev2.Models;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Threading;
using Dev2.ViewModels.Deploy;

// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// The ViewModel associated with a tree in either the deploy or the explorer tabs
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public class NavigationViewModel : NavigationViewModelBase, INavigationViewModel
    {

        #region private fields

        RelayCommand _refreshMenuCommand;
        readonly NavigationViewModelType _navigationViewModelType;
        bool _fromActivityDrop;
        readonly IEventAggregator _eventPublisher;
        readonly IConnectControlSingleton _connectControlSingleton;
        ExplorerItemModel _selectedItem;
        ObservableCollection<ExplorerItemModel> _explorerItemModels;
        #endregion private fields

        #region ctor + init

        public NavigationViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, Guid? context, IEnvironmentRepository environmentRepository, IStudioResourceRepository studioResourceRepository, IConnectControlSingleton connectControlSingleton,  bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All, NavigationViewModelType navigationViewModelType = NavigationViewModelType.Explorer)
            : base(eventPublisher, asyncWorker, environmentRepository, studioResourceRepository)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("connectControlSingleton", connectControlSingleton);

            _eventPublisher = eventPublisher;
            _connectControlSingleton = connectControlSingleton;
            _eventPublisher.Subscribe(this);
            EnvironmentRepository = environmentRepository;
            Context = context;

            DsfActivityType = activityType;
            _fromActivityDrop = isFromActivityDrop;
            _navigationViewModelType = navigationViewModelType;
            Environments = new List<IEnvironmentModel>();
            ExplorerItemModels = new ObservableCollection<ExplorerItemModel>();
            CircularProgressBarVisibility = Visibility.Hidden;
            RefreshButtonVisibility = Visibility.Visible;
        }

        #endregion ctor + intit



        #region public properties

        public Guid? Context { get; private set; }



        public List<IEnvironmentModel> Environments { get; private set; }

        public enDsfActivityType DsfActivityType { get; set; }

        public bool IsFromActivityDrop
        {
            get
            {
                return _fromActivityDrop;
            }
            set
            {
                if(value != _fromActivityDrop)
                {
                    _fromActivityDrop = value;
                    NotifyOfPropertyChange(() => IsFromActivityDrop);
                }
            }
        }
        public IEnvironmentModel FilterEnvironment { get; set; }

        public ExplorerItemModel SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if(Equals(value, _selectedItem))
                {
                    return;
                }
                _selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }

        #endregion public properties

        #region Commands

        public ICommand RenameCommand
        {
            get
            {
                return new RelayCommand(Rename);
            }
        }

        /// <summary>
        /// The command for refreshing the entire tree
        /// </summary>
        /// <value>
        /// The refresh menu command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public ICommand RefreshMenuCommand
        {
            get
            {
                return _refreshMenuCommand ??
                       (_refreshMenuCommand = new RelayCommand(param => UpdateWorkspaces(_connectControlSingleton), param => true));
            }
        }
        public NavigationViewModelType NavigationViewModelType
        {
            get
            {
                return _navigationViewModelType;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Adds an environment and it's resources to the tree
        /// </summary>
        public void AddEnvironment(IEnvironmentModel environment)
        {
            VerifyArgument.IsNotNull("environment", environment);
            var environmentId = environment.ID;

            StudioResourceRepository.AddServerNode(new ExplorerItemModel(_connectControlSingleton)
            {
                ResourcePath = "",
                DisplayName = environment.Name,
                ResourceType = ResourceType.Server,
                EnvironmentId = environment.ID,
                IsConnected = false,
            });

            if(Environments.All(e => e.ID != environmentId))
            {
                Environments.Add(environment);
            }
            
            if(environment.IsConnected || environment.IsLocalHost)
            {
                LoadEnvironmentResources(environment);
            }
            if(environment.Equals(EnvironmentRepository.Source) && environment.Connection != null)
            {
                environment.Connection.StartAutoConnect();
            }
        }

        /// <summary>
        ///     Removes an environment and it's resources from the tree
        /// </summary>
        public void RemoveEnvironment(IEnvironmentModel environment)
        {
            var idx = Environments.FindIndex(e => e.ID == environment.ID);

            if(idx != -1)
            {
                Environments.RemoveAt(idx);
                StudioResourceRepository.RemoveEnvironment(environment.ID);
                SelectLocalHost();
            }
        }

        /// <summary>
        ///     Removes all environemnts
        /// </summary>
        public void RemoveAllEnvironments()
        {
            foreach(var environment in Environments.ToList())
            {
                RemoveEnvironment(environment);
            }
        }

        /// <summary>
        ///     Reload all environments resources
        /// </summary>
        public void RefreshEnvironments()
        {
            foreach(var environment in Environments)
            {
                RefreshEnvironment(environment);
            }
        }

        /// <summary>
        ///     Updates the worksapces for all environments
        /// </summary>
        public void UpdateWorkspaces(IConnectControlSingleton connectControlSingleton)
        {
            if(CircularProgressBarVisibility == Visibility.Visible)
            {
                return;
            }

            CircularProgressBarVisibility = Visibility.Visible; 
            RefreshButtonVisibility = Visibility.Hidden;

            var tmpSelected = SelectedItem;
            List<ExplorerItemModel> expandedList = new List<ExplorerItemModel>();
            List<Task> loadTasks = new List<Task>();
            List<Guid> environments = new List<Guid>();

            foreach(var environment in Environments.Where(c => c.IsConnected || c.IsLocalHost))
            {
                var explorerItemModel = ExplorerItemModels.FirstOrDefault(c => c.EnvironmentId == environment.ID);
                if(explorerItemModel != null)
                {
                    expandedList = explorerItemModel.Descendants().Where(c => c.IsExplorerExpanded).ToList();
                }

                if(environment != null)
                {
                    environments.Add(environment.ID);
                    connectControlSingleton.SetConnectionState(environment.ID, ConnectionEnumerations.ConnectedState.Busy);
                    var loadResourcesAsync = LoadResourcesAsync(environment, expandedList, tmpSelected);

                    if(loadResourcesAsync != null)
                    {
                        loadTasks.Add(loadResourcesAsync);
                    }
                }
            }

            var task = Task.WhenAll(loadTasks);
            task.ContinueWith(d =>
                {
                    environments.ForEach(id => connectControlSingleton.SetConnectionState(id, ConnectionEnumerations.ConnectedState.Connected));
                    CircularProgressBarVisibility = Visibility.Hidden;
                    RefreshButtonVisibility = Visibility.Visible;
                });
        }

        protected override void DoFiltering(string searhFilter)
        {
            if(!string.IsNullOrEmpty(searhFilter))
            {
                Filter(model => model.DisplayName.ToLower().Contains(searhFilter.ToLower()), true);
            }
            else
            {
                Filter(null);
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

        public void Filter(Func<ExplorerItemModel, bool> filter, bool fromFilter = false)
        {
            Func<ExplorerItemModel, bool> workflowFilter = model => (model.ResourceType == ResourceType.WorkflowService || model.ResourceType == ResourceType.Folder);
            Func<ExplorerItemModel, bool> serviceFilter = model => ((model.ResourceType >= ResourceType.DbService && model.ResourceType <= ResourceType.WebService));
            Func<ExplorerItemModel, bool> sourceFilter = model => ((model.ResourceType >= ResourceType.DbSource && model.ResourceType <= ResourceType.ServerSource));

            Func<ExplorerItemModel, bool> environmentFilter = model => true;

            if(FilterEnvironment != null)
            {
                environmentFilter = model => model.EnvironmentId == FilterEnvironment.ID;
            }

            if(filter != null)
            {
                switch(DsfActivityType)
                {
                    case enDsfActivityType.All:
                        ExplorerItemModels = StudioResourceRepository.Filter(model => filter(model) && environmentFilter(model));
                        break;
                    case enDsfActivityType.Workflow:
                        ExplorerItemModels = StudioResourceRepository.Filter(model => workflowFilter(model) && filter(model) && environmentFilter(model));
                        break;
                    case enDsfActivityType.Service:
                        ExplorerItemModels = StudioResourceRepository.Filter(model => serviceFilter(model) && filter(model) && environmentFilter(model));
                        break;
                    case enDsfActivityType.Source:
                        ExplorerItemModels = StudioResourceRepository.Filter(model => sourceFilter(model) && filter(model) && environmentFilter(model));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if(fromFilter)
                {
                    Iterate(model =>
                        {
                            model.IsExplorerExpanded = true;
                            model.IsResourcePickerExpanded = true;
                        });
                }
                else
                {
                    foreach(ExplorerItemModel explorerItemModel in ExplorerItemModels)
                    {
                        explorerItemModel.IsExplorerExpanded = true;
                        explorerItemModel.IsResourcePickerExpanded = true;
                    }
                }
            }
            else
            {
                switch(DsfActivityType)
                {
                    case enDsfActivityType.All:
                        ExplorerItemModels = StudioResourceRepository.ExplorerItemModels;
                        break;
                    case enDsfActivityType.Workflow:
                        ExplorerItemModels = StudioResourceRepository.Filter(model => workflowFilter(model) && environmentFilter(model));
                        break;
                    case enDsfActivityType.Service:
                        ExplorerItemModels = StudioResourceRepository.Filter(model => serviceFilter(model) && environmentFilter(model));
                        break;
                    case enDsfActivityType.Source:
                        ExplorerItemModels = StudioResourceRepository.Filter(model => sourceFilter(model) && environmentFilter(model));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                foreach(ExplorerItemModel explorerItemModel in ExplorerItemModels)
                {
                    explorerItemModel.IsExplorerExpanded = true;
                    explorerItemModel.IsResourcePickerExpanded = true;
                }
            }
        }

        public ExplorerItemModel FindChild(IContextualResourceModel resource)
        {
            var explorerItemModels = ExplorerItemModels.SelectMany(explorerItemModel => explorerItemModel.Descendants()).ToList();
            return resource != null ? explorerItemModels.FirstOrDefault(model => model.ResourceId == resource.ID && model.EnvironmentId == resource.Environment.ID) : null;
        }

        #endregion public methods

        #region private methods

        void Rename(object param)
        {
            if(SelectedItem != null)
            {
                SelectedItem.RenameCommand.Execute(param);
            }
        }

        /// <summary>
        /// Reloads an environment and all of it's resources if the environment 
        /// is being represented by this navigation view model
        /// </summary>
        /// <param name="environment">The environment.</param>
        void RefreshEnvironment(IEnvironmentModel environment)
        {
            if(!Environments.Contains(environment, EnvironmentModelEqualityComparer.Current))
            {
                return;
            }

            var environmentNavigationItemViewModel = StudioResourceRepository.FindItemById(environment.ID);
            environmentNavigationItemViewModel.IsChecked = false;

            LoadEnvironmentResources(environment);
        }

        #endregion

        #region Dispose Handling

        protected override void OnDispose()
        {
            if(EnvironmentRepository != null)
            {
                foreach(IEnvironmentModel environment in EnvironmentRepository.All())
                {
                    environment.ResourceRepository.Dispose();
                }

                EnvironmentRepository.Dispose();
                EnvironmentRepository = null;
            }
            _eventPublisher.Unsubscribe(this);
            base.OnDispose();
        }

        #endregion Dispose Handling

        public ObservableCollection<ExplorerItemModel> ExplorerItemModels
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
        public void BringItemIntoView(IContextualResourceModel item)
        {
            if(item != null && item.Environment != null)
            {
                BringItemIntoView(item.Environment.ID, item.ID);
            }
        }

        public void SelectLocalHost()
        {
            var localhostItem = StudioResourceRepository.FindItem(model => model.DisplayName.ToLower().Contains("localhost"));
            if(localhostItem != null)
            {
                localhostItem.IsExplorerSelected = true;
                this.TraceInfo("Publish message of type - " + typeof(SetActiveEnvironmentMessage));
                var localHost = EnvironmentRepository.FindSingle(model => model.ID == localhostItem.EnvironmentId);
                _eventPublisher.Publish(new SetActiveEnvironmentMessage(localHost));
            }
        }
    }

    public enum NavigationViewModelType
    {
        Explorer,
        DeployFrom,
        DeployTo
    }

    public class BindableSelectedItemBehavior : Behavior<TreeView>
    {
        #region SelectedItem Property

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(BindableSelectedItemBehavior), new UIPropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = e.NewValue as TreeViewItem;
            if(item != null)
            {
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
            }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if(AssociatedObject != null)
            {
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }
    }
}