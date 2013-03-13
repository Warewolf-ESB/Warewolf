//6180 CODEREVIEW - Please region you code

#region

using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;


#endregion

namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    /// The ViewModel associated with a tree in either the deploy or the explorer tabs
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public class NavigationViewModel : SimpleBaseViewModel,
                                       IHandle<EnvironmentConnectedMessage>, IHandle<EnvironmentDisconnectedMessage>,
                                       IHandle<UpdateResourceMessage>, IHandle<RemoveNavigationResourceMessage>
    {
        #region private fields

        private bool _isRefreshing;
        private readonly List<IEnvironmentModel> _environments;
        private readonly ITreeNode _root;
        private readonly bool _useAuxiliryConnections;
        private RelayCommand _refreshMenuCommand;
        private string _searchFilter = string.Empty;
        private readonly SynchronizationContext _synchronizationContext;

        #endregion private fields

        #region ctor + init

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationViewModel" /> class.
        /// </summary>
        /// <param name="useAuxiliryConnections">if set to <c>true</c> [use auxiliry connections].</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public NavigationViewModel(bool useAuxiliryConnections)
        {
            _synchronizationContext = SynchronizationContext.Current;
            EnvironmentRepository = ImportService.GetExportValue<IFrameworkRepository<IEnvironmentModel>>();
            WizardEngine = ImportService.GetExportValue<IWizardEngine>();

            _useAuxiliryConnections = useAuxiliryConnections;

            _environments = new List<IEnvironmentModel>();
            _root = TreeViewModelFactory.Create();
        }

        #endregion ctor + intit

        #region public properties

        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                _isRefreshing = value;
                NotifyOfPropertyChange(() => IsRefreshing);
            }
        }

        public IFrameworkRepository<IEnvironmentModel> EnvironmentRepository { get; set; }

        public IWizardEngine WizardEngine { get; set; }

        ///// <summary>
        ///// Gets or sets the filter to filter tree items by.
        ///// </summary>
        ///// <value>
        ///// The search filter.
        ///// </value>
        ///// <author>Jurie.smit</author>
        ///// <date>2013/01/23</date>
        //public string SearchFilter
        //{
        //    get { return _searchFilter; }
        //    set
        //    {
        //        _searchFilter = value;
        //        NotifyOfPropertyChange(() => SearchFilter);
        //    }
        //}

        /// <summary>
        /// Gets the root node of the tree.
        /// </summary>
        /// <value>
        /// The root node.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        public ITreeNode Root
        {
            get { return _root; }
        }

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
            var e = _environments.FirstOrDefault(o => ReferenceEquals(o, message.EnvironmentModel));

            if (e == null)
                return;

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
            var e = _environments.FirstOrDefault(o => ReferenceEquals(o, message.EnvironmentModel));

            if (e == null) return;

            var environmentNavigationItemViewModel =
                Find(e, false) as EnvironmentTreeViewModel;

            if (environmentNavigationItemViewModel == null)
                return;
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
            UpdateResource(message.ResourceModel);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Adds all environments from repository.
        /// </summary>
        /// <author>Jurie.Smit</author>
        /// <date>2013/01/28</date>
        public void AddAllEnvironmentsFromRepository()
        {
            EnvironmentRepository.All().ToList().ForEach(_environments.Add);
        }

        /// <summary>
        ///     Creates an environment form the server then adds it and environment and it's resources to the tree
        /// </summary>
        public void AddEnvironment(IServer server)
        {
            if (server.Environment == null)
            {
                EnvironmentModelFactory.CreateEnvironmentModel(server);
            }
            AddEnvironment(server.Environment);
        }

        /// <summary>
        ///     Adds an environment and it's resources to the tree
        /// </summary>
        public void AddEnvironment(IEnvironmentModel environment)
        {
            if (_environments.Contains(environment, EnvironmentModelEqualityComparer.Current))
                return;

            _environments.Add(environment);
            LoadEnvironmentResources(environment);
        }

        /// <summary>
        ///     Removes an environment and it's resources from the tree
        /// </summary>
        public void RemoveEnvironment(IEnvironmentModel environment)
        {
            if (!_environments.Remove(environment))
                return;

            var environmentNavigationItemViewModel =
                Find(environment, true);
            Root.Children.Remove(environmentNavigationItemViewModel);
        }

        /// <summary>
        ///     Removes all environemnts
        /// </summary>
        public void RemoveAllEnvironments()
        {
            foreach (var environment in _environments.ToList())
            {
                RemoveEnvironment(environment);
            }
        }

        /// <summary>
        ///     Reload all environments resources
        /// </summary>
        public void RefreshEnvironments()
        {
                foreach (var environment in _environments)
                {
                    RefreshEnvironment(environment);
                }
            }

        /// <summary>
        ///     Updates the worksapces for all environments
        /// </summary>
        public void UpdateWorkspaces()
        {
            var mainVM = ImportService.GetExportValue<IMainViewModel>();
            if (IsRefreshing)
            {
                return;
            }

            IsRefreshing = true;

            try
            {
                foreach (var environment in _environments)
                {
                UpdateWorkspace(environment, mainVM.WorkspaceItems);
                }
            }
            finally
            {
                IsRefreshing = false;
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

            if (returnNavigationItemViewModel == null && createIfMissing)
            {
                returnNavigationItemViewModel = TreeViewModelFactory.Create(environment, Root);
            }

            return returnNavigationItemViewModel;
        }

        /// <summary>
        /// Loads the resources for an environment, any existing resources will be cleared.
        /// If there isn't a node to represent the environment one is created.
        /// If the environment isn't connected an attempt is made to connect.
        /// </summary>
        /// <param name="environment">The environment.</param>
        public void LoadEnvironmentResources(IEnvironmentModel environment)
        {
            if (IsRefreshing)
            {
                return;
            }

            IsRefreshing = true;

            try
            {
            if (environment != null && !environment.IsConnected)
            {
                Connect(environment);
            }

            if (environment != null && environment.IsConnected)
            {
                //
                // Load the environemnts resources
                //
                environment.LoadResources();
            }

            //
            // Build the resources into a tree
            //
            BuildNavigationItemViewModels(environment);
        }
            finally
            {
                IsRefreshing = false;
            }
        }

        /// <summary>
        /// Updates an item with in the current NavigationItemViewModel graph
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void UpdateResource(IResourceModel resource)
        {
            var resourceModel = resource as IContextualResourceModel;
            if (resourceModel == null) return;

            var environmentNode = Root.FindChild(resourceModel.Environment);
            if (environmentNode == null) return;

            var serviceTypeNode = environmentNode.FindChild(resourceModel.ResourceType);
            if (serviceTypeNode == null) return;

            var resourceNode = environmentNode.FindChild(resourceModel) as ResourceTreeViewModel;

            var newCategoryName = TreeViewModelHelper.GetCategoryDisplayName(resourceModel.Category);
            var newCategoryNode = serviceTypeNode.FindChild(newCategoryName);

            if (resourceNode != null)
            {

                var originalCategoryNode = resourceNode.TreeParent;

                //this means the category has changed
                if (newCategoryName != originalCategoryNode.DisplayName)
                {
                    // Remove from old category
                    bool test = originalCategoryNode.Remove(resourceNode);
                    //delete old category if empty
                    if (originalCategoryNode.ChildrenCount == 0)
                    {
                        originalCategoryNode.TreeParent.Remove(originalCategoryNode);
                    }
                }
                else //just update the actual resource
                {
                    resourceNode.DataContext = resource as IContextualResourceModel;
                }
            }
            //Means it doesnt exist, therefore create without a parent
            else
            {
                resourceNode = TreeViewModelFactory.Create(resourceModel, null, WizardEngine.IsWizard(resourceModel)) as ResourceTreeViewModel;
            }

            //if not exist create category
            if (newCategoryNode == null)
            {
                newCategoryNode = TreeViewModelFactory.CreateCategory(newCategoryName,
                                                                      resourceModel.ResourceType, serviceTypeNode);
            }
            //add to category
            if (!ReferenceEquals(newCategoryNode, resourceNode.TreeParent)) newCategoryNode.Add(resourceNode);

        }

        /// <summary>
        /// Called to filter the root treendode
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2/25/2013</date>
        public void UpdateSearchFilter(string searhFilter)
        {
            _searchFilter = searhFilter;

            Root.FilterText = _searchFilter;
            Root.UpdateFilteredNodeExpansionStates(searhFilter);
            Root.NotifyOfFilterPropertyChanged(false);
                     //BackgroundWorker worker = new BackgroundWorker();
            
                     //   worker.DoWork += (s, e) => 
                     //   {
                     //       Root.FilterText = _searchFilter;
                     //       Root.UpdateFilteredNodeExpansionStates(searhFilter);
                     //       Root.NotifyOfFilterPropertyChanged(false);
                     //   };
            
                     //   worker.RunWorkerAsync();
        }

        ///// <summary>
        ///// Called after the specified delay after a key is pressed in the search box, to filter treeview and expand items acordingly.
        ///// </summary>
        ///// <author>Jurie.smit</author>
        ///// <date>2/25/2013</date>
        //public void UpdateSearchFilter(object searhFilter)
        //{
        //    if (searhFilter == null || !(searhFilter is string)) return;
        //    UpdateSearchFilter((string)searhFilter);
        //}
        #endregion public methods

        #region private methods

        /// <summary>
        /// Reloads an environment and all of it's resources if the environment 
        /// is being represented by this navigation view model
        /// </summary>
        /// <param name="environment">The environment.</param>
        private void RefreshEnvironment(IEnvironmentModel environment)
        {
            if (!_environments.Contains(environment, EnvironmentModelEqualityComparer.Current))
                return;

            var environmentNavigationItemViewModel =
                Find(environment, true);
            environmentNavigationItemViewModel.IsChecked = false;

            LoadEnvironmentResources(environment);
        }

        /// <summary>
        ///     Updates the workspace of an environment then reloads it's resources,
        ///     any existing resources will be cleared.
        ///     If there isn't a node to represent the environment one is created.
        ///     If the environment isn't connected an attempt is made to connect.
        /// </summary>
        private void UpdateWorkspace(IEnvironmentModel environment, IList<IWorkspaceItem> workspaceItems)
        {
            if (environment != null && !environment.IsConnected)
            {
                Connect(environment);
            }

            if (environment == null || environment.ResourceRepository == null || !environment.IsConnected) return;

            //
            // Load the environments resources
            //
            environment.ResourceRepository.UpdateWorkspace(workspaceItems);

            //
            // Build the resources into a tree
            //
            BuildNavigationItemViewModels(environment);
        }

        /// <summary>
        /// Clears the children.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void ClearChildren(ITreeNode node)
        {
            node.Children = new SortedObservableCollection<ITreeNode>();
        }

        /// <summary>
        /// Builds the resources of an environment into a tree structure
        /// </summary>
        /// <param name="environment">The environment.</param>
        private void BuildNavigationItemViewModels(IEnvironmentModel environment)
        {
            var environmentVM =
                Find(environment, true);

            if (environment == null || !environment.IsConnected || environment.ResourceRepository == null) return;

            //
            // Load the environemnts resources
            //
            var resources = environment.ResourceRepository.All().Cast<IContextualResourceModel>().ToList();

            //
            // Clear any resources currently being displayed for the environment
            //
            ClearChildren(environmentVM);

            BuildCategoryTree(ResourceType.WorkflowService, environmentVM,
                                resources.Where(r => r.ResourceType == ResourceType.WorkflowService).ToList());
            BuildCategoryTree(ResourceType.Source, environmentVM,
                                resources.Where(r => r.ResourceType == ResourceType.Source).ToList());
            BuildCategoryTree(ResourceType.Service, environmentVM,
                                resources.Where(r => r.ResourceType == ResourceType.Service).ToList());
            UpdateSearchFilter(_searchFilter);
        }

        /// <summary>
        /// Builds the category tree.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="environmentVM">The environment VM.</param>
        /// <param name="resources">The resources.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void BuildCategoryTree(ResourceType resourceType, ITreeNode environmentVM,
                                       List<IContextualResourceModel> resources)
        {
            var workflowServiceRoot =
                TreeViewModelFactory.Create(resourceType, environmentVM);

            //
            // Add workflow categories
            //
            var categoryList = (from c in resources
                                orderby c.Category
                                select c.Category.ToUpper()).Distinct();

            foreach (var c in categoryList)
            {
                var categoryName = c;
                var displayName = TreeViewModelHelper.GetCategoryDisplayName(c);

                //
                // Create category under workflow service root 
                //
                var categoryWorkflowItems =
                    resources.Where(res =>
                                    CategorySearchPredicate(res, resourceType, categoryName)).ToList();

                if (!categoryWorkflowItems.Any()) continue;

                var categoryVM = TreeViewModelFactory.CreateCategory(displayName, resourceType, workflowServiceRoot);
                AddChildren(categoryWorkflowItems, categoryVM);
            }
        }

        /// <summary>
        /// Adds the children.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="parent">The parent.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void AddChildren(IEnumerable<IContextualResourceModel> items,
                                 ITreeNode parent)
        {
            if (items == null)
            {
                return;
            }

            items
                .ToList()
                .ForEach(resource => AddChild(resource, parent));
        }

        /// <summary>
        /// Adds a child.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="isWizard">if set to <c>true</c> [is wizard].</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        private void AddChild(IContextualResourceModel resource,
                              ITreeNode parent, bool isWizard = false)
        {
            var res = TreeViewModelFactory.Create(resource, parent, isWizard);

            //
            // Add wizard
            //
            if (WizardEngine.IsResourceWizard(resource))
                return;

            var wizardResource = WizardEngine.GetWizard(resource);
            if (wizardResource != null)
            {
                AddChild(wizardResource, res, true);
            }
        }

        /// <summary>
        /// Determines if a resource meets the current search criteria for a category
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        private bool CategorySearchPredicate(IContextualResourceModel resource, ResourceType resourceType,
                                             string category)
        {
            if (resource == null || WizardEngine.IsResourceWizard(resource))
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
        private void Connect(IEnvironmentModel environment)
        {
            //TODO Refactor to EnvironmentController or something
            if (environment.IsConnected) return;

            if (_useAuxiliryConnections)
            {
                var primaryEnvironment =
                    EnvironmentRepository.FindSingle(
                        e => EnvironmentModelEqualityComparer.Current.Equals(e, environment)) ??
                    EnvironmentModelFactory.CreateEnvironmentModel(environment);

                var disconnectFromPrimary = !primaryEnvironment.IsConnected;

                try
                {
                    environment.Connect(primaryEnvironment);
                }
                // ReSharper disable EmptyGeneralCatchClause
                finally
                {
                    if (disconnectFromPrimary && primaryEnvironment.IsConnected)
                    {
                        primaryEnvironment.Disconnect();
                    }

                    //if (!environment.IsConnected)
                    //{
                    //    throw new Exception("Auxiliary Connection failed.");
                    //}
                }
            }
            else
            {
                environment.Connect();
            }
        }

        #endregion private methods

        public void Handle(RemoveNavigationResourceMessage message)
        {
            var resource = message.ResourceModel;
            var vm = Root.FindChild(resource);
            if (vm != null)
            {
                if (vm.TreeParent != null)
                {
                    vm.TreeParent.Children.Remove(vm);
                }
            }
        }
    }
}