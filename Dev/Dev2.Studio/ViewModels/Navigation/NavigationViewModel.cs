using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Enums;
using Dev2.Threading;
using Action = System.Action;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// The ViewModel associated with a tree in either the deploy or the explorer tabs
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public class NavigationViewModel : SimpleBaseViewModel,
        INavigationContext,
        IHandle<EnvironmentConnectedMessage>, IHandle<EnvironmentDisconnectedMessage>,
        IHandle<UpdateResourceMessage>, IHandle<RemoveNavigationResourceMessage>
    {
        public event EventHandler LoadResourcesCompleted;

        #region private fields

        bool _isRefreshing;
        readonly ITreeNode _root;
        RelayCommand _refreshMenuCommand;
        string _searchFilter = string.Empty;
        enDsfActivityType _activityType;
        readonly NavigationViewModelType _navigationViewModelType;
        bool _fromActivityDrop;
        readonly IEventAggregator _eventPublisher;
        readonly IAsyncWorker _asyncWorker;

        #endregion private fields

        #region ctor + init

        public NavigationViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, Guid? context, IEnvironmentRepository environmentRepository, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All, NavigationViewModelType navigationViewModelType = NavigationViewModelType.Explorer)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);

            _eventPublisher = eventPublisher;
            _asyncWorker = asyncWorker;
            _eventPublisher.Subscribe(this);
            EnvironmentRepository = environmentRepository;
            Context = context;

            _activityType = activityType;
            _fromActivityDrop = isFromActivityDrop;
            _navigationViewModelType = navigationViewModelType;
            Environments = new List<IEnvironmentModel>();

            _root = new RootTreeViewModel(eventPublisher);
            ((Screen)_root).Parent = this;
        }


        #endregion ctor + intit

        #region public properties

        public Guid? Context { get; private set; }

        public List<IEnvironmentModel> Environments { get; private set; }

        public enDsfActivityType DsfActivityType { get { return _activityType; } set { _activityType = value; } }

        public bool IsFromActivityDrop
        {
            get { return _fromActivityDrop; }
            set
            {
                if(value != _fromActivityDrop)
                {
                    _fromActivityDrop = value;
                    NotifyOfPropertyChange(() => IsFromActivityDrop);
                }
            }
        }

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                NotifyOfPropertyChange(() => IsRefreshing);
            }
        }

        public IEnvironmentRepository EnvironmentRepository { get; private set; }

        public ITreeNode Root { get { return _root; } }

        #endregion public properties

        #region Commands

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
                       (_refreshMenuCommand = new RelayCommand(param => UpdateWorkspaces(), param => true));
            }
        }

        #endregion

        #region IHandle

        /// <summary>
        /// Handles the specified environment connected message by loading the environments 
        /// and building the tree
        /// </summary>
        /// <param name="message">The message.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void Handle(EnvironmentConnectedMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            var e = Environments.FirstOrDefault(o => ReferenceEquals(o, message.EnvironmentModel));
            LoadEnvironmentResources(e);
        }

        /// <summary>
        /// Handles the specified environment disconnected message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void Handle(EnvironmentDisconnectedMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            var e = Environments.FirstOrDefault(o => ReferenceEquals(o, message.EnvironmentModel));

            if(e == null)
            {
                return;
            }
            IsRefreshing = false;
            var environmentNavigationItemViewModel =
                Find(e, false) as EnvironmentTreeViewModel;

            if(environmentNavigationItemViewModel == null)
            {
                return;
            }
            environmentNavigationItemViewModel.Children.Clear();
            environmentNavigationItemViewModel.RaisePropertyChangedForCommands();
        }

        /// <summary>
        /// Handles the specified UpdateResourcemessage by updating the resource
        /// </summary>
        /// <param name="message">The message.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public void Handle(UpdateResourceMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            UpdateResource(message.ResourceModel);
        }

        public void Handle(RemoveNavigationResourceMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            var node = Root.FindChild(message.ResourceModel);
            if(node is AbstractTreeViewModel)
            {
                var treeParent = (node as AbstractTreeViewModel).TreeParent;
                if(treeParent != null)
                {
                    treeParent.Remove(node);
                }
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
            if(Environments.Any(e => e.ID == environment.ID))
            {
                return;
            }
            Environments.Add(environment);

            if(environment.CanStudioExecute)
            {
                ITreeNode newEnvNode = new EnvironmentTreeViewModel(_eventPublisher, Root, environment, new AsyncWorker());
                newEnvNode.IsSelected = true;
            }
            //2013.06.02: Ashley Lewis for bugs 9444+9445 - Show disconnected environments but dont autoconnect
            if(environment.IsConnected || environment.IsLocalHost())
            {
                LoadEnvironmentResources(environment);
            }
            if(Equals(environment, EnvironmentRepository.Source) && environment.Connection != null)
            {
                // BUG 10106 - 2013.08.13 - TWR - start localhost auto-connect if server not connected
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
                var environmentNavigationItemViewModel = Find(environment, true);
                Root.Children.Remove(environmentNavigationItemViewModel);
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
        public void UpdateWorkspaces()
        {
            if(IsRefreshing)
            {
                return;
            }

            // Added the Where clause to only refresh the connected environments.Massimo.Guerrera BUG 9441
            // Added "|| c.IsLocalHost()" to the Where clause to connect to disconnected localhost - 2013.08.13: Ashley Lewis for bug 10106 (studio autoconnect)
            foreach(var environment in Environments.Where(c => c.IsConnected || c.IsLocalHost()))
            {
                LoadEnvironmentResources(environment);
            }
        }

        /// <summary>
        /// Returns the node which represents an environment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="createIfMissing">if set to <c>true</c> [create if missing].</param>
        /// <returns></returns>
        public ITreeNode Find(IEnvironmentModel environment, bool createIfMissing)
        {
            ITreeNode returnNavigationItemViewModel =
                Root.Children.Cast<EnvironmentTreeViewModel>()
                    .FirstOrDefault(
                        vm => EnvironmentModelEqualityComparer.Current
                            .Equals(environment, vm.EnvironmentModel));

            if(returnNavigationItemViewModel == null && createIfMissing)
            {
                returnNavigationItemViewModel = new EnvironmentTreeViewModel(_eventPublisher, Root, environment, new AsyncWorker());
            }

            return returnNavigationItemViewModel;
        }

        public void LoadEnvironmentResources(IEnvironmentModel environment)
        {
            LoadResourcesAsync(environment);
        }

        /// <summary>
        /// Updates an item with in the current NavigationItemViewModel graph
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void UpdateResource(IContextualResourceModel resource)
        {
            if(Root.Children.Count > 0 && resource != null && !resource.IsNewWorkflow)
            {
                var child = ForceGetChildNode(resource);
                if(child != null)
                {
                    var resourceNode = child as ResourceTreeViewModel;
                    if(resourceNode != null)
                    {
                        UpdateSearchFilter(_searchFilter);
                    }
                }
            }
        }

        public bool SetNodeOverwrite(IContextualResourceModel resource, bool state)
        {
            if(Root.Children.Count > 0 && resource != null && !resource.IsNewWorkflow && Environments.Any())
            {
                IEnvironmentModel env = Environments[0];

                var resModel = env.ResourceRepository.All()
                                    .FirstOrDefault(r => ResourceModelEqualityComparer
                                    .Current.Equals(r, resource));
                if(resModel != null)
                {
                    var child = TryGetResourceNode(resModel as IContextualResourceModel);
                    if(child != null)
                    {
                        child.TreeParent.IsOverwrite = state;
                        return child.IsOverwrite = state;
                    }
                }
            }
            return false;
        }

        public ITreeNode TryGetResourceNode(IContextualResourceModel resourceModel)
        {
            CategoryTreeViewModel findCategoryNode;
            ServiceTypeTreeViewModel findServiceTypeNode;
            TryGetResourceCategoryAndServiceTypeNodes(resourceModel, out findCategoryNode, out findServiceTypeNode);
            if(findCategoryNode != null)
            {
                return findCategoryNode.Children.FirstOrDefault(cat => cat.DisplayName == resourceModel.ResourceName);
            }
            return null;
        }

        void TryGetResourceCategoryAndServiceTypeNodes(IContextualResourceModel resourceModel, out CategoryTreeViewModel categoryNode, out ServiceTypeTreeViewModel serviceTypeNode)
        {
            categoryNode = null;
            serviceTypeNode = null;

            var environmentNode = Root.Children.FirstOrDefault(env =>
                EnvironmentModelEqualityComparer.Current.Equals(env.EnvironmentModel, resourceModel.Environment));
            if(environmentNode == null)
            {
                return;
            }

            serviceTypeNode = environmentNode.Children.FirstOrDefault(typeNode =>
                typeNode is ServiceTypeTreeViewModel && (typeNode as ServiceTypeTreeViewModel).ResourceType == resourceModel.ResourceType) as ServiceTypeTreeViewModel;
            if(serviceTypeNode == null)
            {
                serviceTypeNode = new ServiceTypeTreeViewModel(_eventPublisher, environmentNode, resourceModel.ResourceType);
            }
            else
            {
                categoryNode = serviceTypeNode.Children.FirstOrDefault(cat =>
                    cat.Children.Any(res => res.DisplayName == resourceModel.ResourceName)) as CategoryTreeViewModel;
            }
        }

        /// <summary>
        /// Gets or creates a resource node
        /// </summary>
        ITreeNode ForceGetChildNode(IContextualResourceModel resourceModel)
        {
            CategoryTreeViewModel oldCategoryNode;
            ServiceTypeTreeViewModel serviceTypeNode;
            TryGetResourceCategoryAndServiceTypeNodes(resourceModel, out oldCategoryNode, out serviceTypeNode);
            if(oldCategoryNode == null && serviceTypeNode == null)
            {
                //wrong environment
                return null;
            }

            if(oldCategoryNode != null)
            {
                // Remove resource from old category
                var oldResourceNode = oldCategoryNode.Children.FirstOrDefault(res => res.DisplayName == resourceModel.ResourceName);
                if(oldResourceNode != null)
                {
                    oldCategoryNode.Remove(oldResourceNode);
                }
            }

            // I am sick of this null point lazyness!
            var resourceModelCategory = resourceModel.Category;
            if(!string.IsNullOrEmpty(resourceModelCategory))
            {
                resourceModelCategory = resourceModelCategory.ToUpper();
            }

            var categoryDisplayName = resourceModel.Category == string.Empty ? StringResources.Navigation_Category_Unassigned.ToUpper() : resourceModelCategory;

            var newCategoryNode = serviceTypeNode.Children.FirstOrDefault(cat => cat.DisplayName.ToUpper() == categoryDisplayName)
                                  ?? new CategoryTreeViewModel(_eventPublisher, serviceTypeNode, categoryDisplayName, resourceModel.ResourceType);

            var newResourceNode = newCategoryNode.Children.FirstOrDefault(res => res.DisplayName == resourceModel.ResourceName)
                                  ?? new ResourceTreeViewModel(_eventPublisher, newCategoryNode, resourceModel);

            return newResourceNode;
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
                    worker.DoWork += (s, e) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => DoFiltering(searhFilter)));
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

        void DoFiltering(string searhFilter)
        {
            Root.FilterText = _searchFilter;
            Root.UpdateFilteredNodeExpansionStates(searhFilter);
            Root.NotifyOfFilterPropertyChanged(false);
        }

        /// <summary>
        /// Sets the selected item to null
        /// </summary>
        public void SetSelectedItemNull()
        {
            this.TraceInfo("Publish message of type - " + typeof(SetSelectedIContextualResourceModel));
            _eventPublisher.Publish(new SetSelectedIContextualResourceModel(null, false));
        }

        #endregion public methods

        #region private methods

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

            var environmentNavigationItemViewModel = Find(environment, true);
            environmentNavigationItemViewModel.IsChecked = false;

            LoadEnvironmentResources(environment);
        }

        /// <summary>
        /// Builds the resources of an environment into a tree structure
        /// </summary>
        /// <param name="environment">The environment.</param>
        void BuildNavigationItemViewModels(IEnvironmentModel environment)
        {
            var environmentNode = Find(environment, true);

            if(environment == null || !environment.IsConnected || environment.ResourceRepository == null)
            {
                return;
            }

            //
            // Load the environemnts resources
            //
            var resources = environment.ResourceRepository.All().ToArray();

            // the darn resource keep mutating?!
            var contextualResources = resources.Cast<IContextualResourceModel>().ToList();

            //
            // Clear any resources currently being displayed for the environment
            //

            var preTreeViewModels = new HashSet<ResourceTreeViewModel>();

            var treeNodes = environmentNode.GetChildren(c => c.GetType() == typeof(ResourceTreeViewModel)).ToList();

            foreach(ResourceTreeViewModel resourceTreeViewModel in treeNodes)
            {
                preTreeViewModels.Add(resourceTreeViewModel);
            }

            switch(_activityType)
            {
                case enDsfActivityType.Workflow:
                    BuildCategoryTree(ResourceType.WorkflowService, environmentNode,
                        contextualResources.Where(
                            r => r.ResourceType == ResourceType.WorkflowService && !r.IsNewWorkflow)
                            .ToList(), preTreeViewModels);
                    break;
                case enDsfActivityType.Service:
                    BuildCategoryTree(ResourceType.Service, environmentNode,
                        contextualResources.Where(r => r.ResourceType == ResourceType.Service).ToList(), preTreeViewModels);
                    break;
                case enDsfActivityType.Source:
                    BuildCategoryTree(ResourceType.Source, environmentNode,
                        contextualResources.Where(r => r.ResourceType == ResourceType.Source).ToList(), preTreeViewModels);
                    break;
                default:

                    BuildCategoryTree(ResourceType.WorkflowService, environmentNode,
                        contextualResources.Where(
                            r => r.ResourceType == ResourceType.WorkflowService && !r.IsNewWorkflow)
                            .ToList(), preTreeViewModels);
                    BuildCategoryTree(ResourceType.Source, environmentNode,
                        contextualResources.Where(r => r.ResourceType == ResourceType.Source).ToList(), preTreeViewModels);
                    BuildCategoryTree(ResourceType.Service, environmentNode,
                        contextualResources.Where(r => r.ResourceType == ResourceType.Service).ToList(), preTreeViewModels);
                    UpdateSearchFilter(_searchFilter);
                    break;
            }

            foreach(ResourceTreeViewModel preResourceTreeViewModel in preTreeViewModels)
            {
                var tryFindNode = environmentNode.FindChild(preResourceTreeViewModel);
                if(tryFindNode != null)
                {
                    tryFindNode.TreeParent.Children.Remove(preResourceTreeViewModel);
                }
            }
        }

        void BuildCategoryTree(ResourceType resourceType, ITreeNode environmentNode,
            List<IContextualResourceModel> resources, HashSet<ResourceTreeViewModel> preResourceTreeViewModels)
        {
            var serviceNode = environmentNode.FindChild(resourceType);
            var workflowServiceRoot = serviceNode ?? new ServiceTypeTreeViewModel(_eventPublisher, environmentNode, resourceType);

            //
            // Add workflow categories
            //
            var categoryList = (from c in resources
                                orderby c.Category
                                select c.Category.ToUpper()).Distinct();

            foreach(var c in categoryList)
            {
                var categoryName = c;
                var displayName = TreeViewModelHelper.GetCategoryDisplayName(c);

                var categoryWorkflowItems = new List<IContextualResourceModel>();

                //
                // Create category under workflow service root 
                //
                foreach(IContextualResourceModel contextualResourceModel in resources)
                {
                    if(CategorySearchPredicate(contextualResourceModel, resourceType, categoryName))
                    {
                        var tmpResTreeViewModel = preResourceTreeViewModels.FirstOrDefault(r => r.DisplayName == contextualResourceModel.ResourceName);

                        if(tmpResTreeViewModel == null)
                        {
                            categoryWorkflowItems.Add(contextualResourceModel);
                        }
                        else
                        {
                            preResourceTreeViewModels.Remove(tmpResTreeViewModel);
                        }
                    }
                }

                if(!categoryWorkflowItems.Any())
                {
                    continue;
                }

                var treeNodes = workflowServiceRoot.GetChildren(x => x.GetType() == typeof(CategoryTreeViewModel)).ToList();

                var categoryNode = treeNodes.FirstOrDefault(x => x.DisplayName == displayName)
                                   ?? new CategoryTreeViewModel(_eventPublisher, workflowServiceRoot, displayName, resourceType);

                AddChildren(categoryNode, categoryWorkflowItems);
            }
        }

        /// <summary>
        /// Adds the children.
        /// </summary>
        /// <param name="children">The items.</param>
        /// <param name="parent">The parent.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        void AddChildren(ITreeNode parent, IEnumerable<IContextualResourceModel> children)
        {
            if(children == null)
            {
                return;
            }

            foreach(var resourceModel in children)
            {
                AddChild(parent, resourceModel);
            }
        }

        void AddChild(ITreeNode parent, IContextualResourceModel resourceModel)
        {
            if(!resourceModel.IsNewWorkflow)
            {
                // ReSharper disable UnusedVariable
                var child = new ResourceTreeViewModel(_eventPublisher, parent, resourceModel);
                // ReSharper restore UnusedVariable
            }
        }


        /// <summary>
        /// Determines if a resource meets the current search criteria for a category
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        bool CategorySearchPredicate(IContextualResourceModel resource, ResourceType resourceType, string category)
        {
            if(resource == null)
            {
                return false;
            }

            return resource.Category.Equals(category, StringComparison.InvariantCultureIgnoreCase) &&
                   resource.ResourceType == resourceType;
        }

        /// <summary>
        /// Connects to a server considering the auxilliry connection field.
        /// </summary>
        /// <param name="environment">The environment.</param>
        void Connect(IEnvironmentModel environment)
        {
            //TODO Refactor to EnvironmentController or something
            if(environment.IsConnected)
            {
                return;
            }
            environment.Connect();
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

        async void LoadResourcesAsync(IEnvironmentModel environment)
        {
            if(IsRefreshing || environment == null)
            {
                return;
            }

            UpdateIsRefreshing(environment, true);

            if(_asyncWorker != null)
            {
                await _asyncWorker.Start(() =>
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
                            //
                            // Build the resources into a tree
                            //
                            switch(_navigationViewModelType)
                            {
                                case NavigationViewModelType.Explorer:
                                    BuildNavigationItemViewModels(environment);
                                    break;
                                case NavigationViewModelType.DeployFrom:
                                    if(environment.IsAuthorizedDeployFrom)
                                    {
                                        BuildNavigationItemViewModels(environment);
                                    }
                                    break;
                                case NavigationViewModelType.DeployTo:
                                    if(environment.IsAuthorizedDeployTo)
                                    {
                                        BuildNavigationItemViewModels(environment);
                                    }
                                    break;
                            }
                        }
                    }
                    finally
                    {
                        UpdateIsRefreshing(environment, false);
                        OnLoadResourcesCompleted();
                    }
                });
            }
        }

        void UpdateIsRefreshing(IEnvironmentModel environment, bool isRefreshing)
        {
            IsRefreshing = isRefreshing;

            var environmentTreeViewModel = Root.FindChild(environment) as EnvironmentTreeViewModel;
            if(environmentTreeViewModel != null)
            {
                environmentTreeViewModel.IsRefreshing = isRefreshing;
            }
        }

        void OnLoadResourcesCompleted()
        {
            if(LoadResourcesCompleted != null)
            {
                LoadResourcesCompleted(this, EventArgs.Empty);
            }
        }

        public void BringItemIntoView(IContextualResourceModel item)
        {
            if(item != null)
            {
                ITreeNode childNode = TryGetResourceNode(item);
                if(childNode != null)
                {
                    childNode.TreeParent.IsExpanded = true;
                    childNode.IsSelected = true;
                    childNode.NotifyOfPropertyChange("IsSelected");
                }
            }
        }

        public void SelectLocalHost()
        {
            var treeNodes = Root.GetChildren(c => c.DisplayName.Contains("localhost")).ToList();
            if(treeNodes.Count == 1 && treeNodes[0] is EnvironmentTreeViewModel)
            {
                treeNodes[0].IsSelected = true;
                this.TraceInfo("Publish message of type - " + typeof(SetActiveEnvironmentMessage));
                _eventPublisher.Publish(new SetActiveEnvironmentMessage(treeNodes[0].EnvironmentModel));
            }
        }
    }

    public enum NavigationViewModelType
    {
        Explorer,
        DeployFrom,
        DeployTo
    }
}