using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.ViewModels.Navigation
{
    public class NavigationViewModel : BaseViewModel
    {
        #region Fields

        private const string UnassignedCategory = "Unassigned";
        private const string WorkflowServicesCategory = "WORKFLOW SERVICES";
        private const string WorkerServicesCategory = "WORKER SERVICES";
        private const string SourcesCategory = "SOURCES";
        private const string RootNdeName = "Explorer";

        private readonly List<IEnvironmentModel> _environments;
        private readonly NavigationItemViewModel _root;
        private readonly bool _useAuxiliryConnections;
        private RelayCommand _refreshMenuCommand;
        private string _searchFilter = string.Empty;

        #endregion

        #region Events

        #region ItemsChecked

        public event EventHandler ItemsChecked;

        protected void OnItemsChecked()
        {
            if (ItemsChecked != null)
            {
                ItemsChecked(this, new EventArgs());
            }
        }

        #endregion ItemsChecked

        #endregion Events

        #region Constructor

        public NavigationViewModel(bool useAuxiliryConnections)
        {
            _useAuxiliryConnections = useAuxiliryConnections;

            _environments = new List<IEnvironmentModel>();
            _root = new NavigationItemViewModel(RootNdeName, StringResources.Navigation_Folder_Icon_Pack_Uri, null, null);
                // Requires Factory

            Mediator.RegisterToReceiveMessage(MediatorMessages.UpdateNavigationItemViewModel, UpdateResource);
            Mediator.RegisterToReceiveMessage(MediatorMessages.EnvironmentConnected, EnvironmentConnected);
            Mediator.RegisterToReceiveMessage(MediatorMessages.EnvironmentDisconnected, EnvironmentDisconnected);
        }

        #endregion Constructor

        #region Properties

        [Import]
        public IUserInterfaceLayoutProvider UserInterfaceLayoutProvider { get; set; }

        [Import]
        public IFrameworkRepository<IEnvironmentModel> EnvironmentRepository { get; set; }

        [Import]
        public IWizardEngine WizardEngine { get; set; }

        public string SearchFilter
        {
            get { return _searchFilter; }
            set
            {
                _searchFilter = value;
                RebuildAllNavigationItemViewModels();
                OnItemsChecked();
                NotifyOfPropertyChange(() => SearchFilter);
            }
        }

        public NavigationItemViewModelBase Root
        {
            get { return _root; }
        }

        #endregion

        #region Commands

        public ICommand RefreshMenuCommand
        {
            get
            {
                return _refreshMenuCommand ??
                       (_refreshMenuCommand = new RelayCommand(param => UpdateWorkspaces(), param => true));
            }
        }

        #endregion

        #region Methods

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
            if (!_environments.Contains(environment, EnvironmentModelEqualityComparer.Current))
            {
                _environments.Add(environment);
                LoadEnvironmentResources(environment);
            }
        }

        /// <summary>
        ///     Removes an environment and it's resources from the tree
        /// </summary>
        public void RemoveEnvironment(IEnvironmentModel environment)
        {
            if (!_environments.Remove(environment)) return;

            var environmentNavigationItemViewModel =
                GetNavigationItemViewModelFromEnvironment(environment, true);
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
        ///     Reloads an environment and all of it's resources if the environment is being represented by this navigation view model
        /// </summary>
        public void RefreshEnvironment(IEnvironmentModel environment)
        {
            if (!_environments.Contains(environment, EnvironmentModelEqualityComparer.Current)) return;

            var environmentNavigationItemViewModel =
                GetNavigationItemViewModelFromEnvironment(environment, true);
            environmentNavigationItemViewModel.IsChecked = false;

            LoadEnvironmentResources(environment);

            OnItemsChecked();
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
            foreach (var environment in _environments)
            {
                UpdateWorkspace(environment, UserInterfaceLayoutProvider.WorkspaceItems);
            }
        }

        /// <summary>
        ///     Returns the NavigationItemViewModel node which represents an environment.
        /// </summary>
        public NavigationItemViewModel GetNavigationItemViewModelFromEnvironment(IEnvironmentModel environment,
                                                                                 bool createIfMissing)
        {
            var returnNavigationItemViewModel =
                Root.Children.Cast<NavigationItemViewModel>()
                    .FirstOrDefault(
                        navigationItemViewModel =>
                        EnvironmentModelEqualityComparer.Current.Equals(environment, navigationItemViewModel.DataContext));

            if (returnNavigationItemViewModel == null && createIfMissing)
            {
                returnNavigationItemViewModel =
                    new NavigationItemViewModel(
                        string.Format("{0} ({1})", environment.Name, environment.DsfAddress.AbsoluteUri),
                        StringResources.Navigation_Environment_Icon_Pack_Uri, _root, environment, true, false, null,
                        true, false, environment, ChildCountPredicate); //Requires a Factory
                Root.Children.Add(returnNavigationItemViewModel);
            }

            return returnNavigationItemViewModel;
        }

        #endregion Methods

        #region Private Methods

        private void EnvironmentConnected(object environment)
        {
            var e = environment as IEnvironmentModel;
            e = _environments.FirstOrDefault(o => ReferenceEquals(o, e));

            if (e != null)
            {
                LoadEnvironmentResources(e);
            }
        }

        private void EnvironmentDisconnected(object environment)
        {
            var e = environment as IEnvironmentModel;
            e = _environments.FirstOrDefault(o => ReferenceEquals(o, e));

            NavigationItemViewModel environmentNavigationItemViewModel = null;

            if (e != null)
            {
                environmentNavigationItemViewModel = GetNavigationItemViewModelFromEnvironment(e, false);
            }

            if (environmentNavigationItemViewModel == null) return;

            environmentNavigationItemViewModel.IsChecked = false;
            ClearChildren(environmentNavigationItemViewModel);
            environmentNavigationItemViewModel.RaisePropertyChangedForCommands();
        }

        /// <summary>
        ///     Loads the resources for an environment, any existing resources will be cleared.
        ///     If there isn't a node to represent the environment one is created.
        ///     If the environment isn't connected an attempt is made to connect.
        /// </summary>
        private void LoadEnvironmentResources(IEnvironmentModel environment)
        {

            if (environment != null && !environment.IsConnected)
            {
                Connect(environment);
            }
            else if (environment != null)
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

        /// <summary>
        ///     Updates the workspace of an environment then reloads it's resources, any existing resources will be cleared.
        ///     If there isn't a node to represent the environment one is created.
        ///     If the environment isn't connected an attempt is made to connect.
        /// </summary>
        private void UpdateWorkspace(IEnvironmentModel environment, IList<IWorkspaceItem> workspaceItems)
        {
            if (environment != null && !environment.IsConnected)
            {
                Connect(environment);
            }

            if (environment == null || environment.Resources == null || !environment.IsConnected) return;

            //
            // Load the environemnts resources
            //
            environment.Resources.UpdateWorkspace(workspaceItems);

            //
            // Build the resources into a tree
            //
            BuildNavigationItemViewModels(environment);
        }

        /// <summary>
        ///     Builds the resources of an environment into a tree structure
        /// </summary>
        private void BuildNavigationItemViewModels(IEnvironmentModel environment)
        {
            var environmentNavigationItemViewModel =
                GetNavigationItemViewModelFromEnvironment(environment, true);
            UnsubscribeFromChecked(environmentNavigationItemViewModel);
            environmentNavigationItemViewModel.IsChecked = false;

            if (environment == null || !environment.IsConnected || environment.Resources == null) return;

            //
            // Load the environemnts resources
            //
            var resources = environment.Resources.All().ToList();

            //
            // Clear any resources currently being displayed for the environment
            //
            ClearChildren(environmentNavigationItemViewModel);

            //
            // Create workflow service root
            //
            var workflowServiceRoot =
                new NavigationItemViewModel(WorkflowServicesCategory,
                                            StringResources.Navigation_Folder_Icon_Pack_Uri,
                                            environmentNavigationItemViewModel,
                                            resources.Where(
                                                c => RootSearchPredicate(c, enResourceType.WorkflowService)),
                                            true, false, null, false, true, environment,
                                            ChildCountPredicate);
            environmentNavigationItemViewModel.Children.Add(workflowServiceRoot);

            //
            // Create worker service root
            //
            var workerServiceRoot =
                new NavigationItemViewModel(WorkerServicesCategory,
                                            StringResources.Navigation_Folder_Icon_Pack_Uri,
                                            environmentNavigationItemViewModel,
                                            resources.Where(
                                                c => RootSearchPredicate(c, enResourceType.Service)),
                                            true, false, null, false, true, environment,
                                            ChildCountPredicate);
            environmentNavigationItemViewModel.Children.Add(workerServiceRoot);

            //
            // Create source root
            //
            var sourceServiceRoot =
                new NavigationItemViewModel(SourcesCategory,
                                            StringResources.Navigation_Folder_Icon_Pack_Uri,
                                            environmentNavigationItemViewModel,
                                            resources.Where(
                                                c => RootSearchPredicate(c, enResourceType.Service)),
                                            true, false, null, false, true, environment,
                                            ChildCountPredicate);
            environmentNavigationItemViewModel.Children.Add(sourceServiceRoot);

            //
            // Add workflow categories
            //
            var categoryList = from c in resources
                               group c by c.Category.ToUpper()
                               into grp
                               select new {Category = grp.Key, Count = grp.Count()};


            foreach (var c in categoryList.OrderBy(ca => ca.Category))
            {
                var category = c.Category;
                var cat = GetCategoryName(category);

                //
                // Create category under workflow service root 
                //
                var categoryWorkflowItems =
                    resources.Where(res => CategorySearchPredicate(res, enResourceType.WorkflowService, cat));
                var categoryVM = new NavigationItemViewModel(cat, StringResources.Navigation_Folder_Icon_Pack_Uri,
                                                             workflowServiceRoot, categoryWorkflowItems, false,
                                                             false, null, false, true, environment,
                                                             ChildCountPredicate);
                if (categoryWorkflowItems.Any() || cat.Equals(UnassignedCategory))
                {
                    workflowServiceRoot.Children.Add(categoryVM);

                    //
                    // Add relavent items to category
                    //
                    AddChildren(resources, d => CategorySearchPredicate(d, enResourceType.WorkflowService, category),
                                categoryVM, environment);
                }

                //
                // Create category under worker service root 
                //
                var categoryServiceItems =
                    resources.Where(res => CategorySearchPredicate(res, enResourceType.Service, cat));
                categoryVM = new NavigationItemViewModel(cat, StringResources.Navigation_Folder_Icon_Pack_Uri,
                                                         workerServiceRoot, categoryServiceItems, false, false, null,
                                                         false, true, environment, ChildCountPredicate);
                if (categoryServiceItems.Any() || cat.Equals(UnassignedCategory))
                {
                    workerServiceRoot.Children.Add(categoryVM);

                    //
                    // Add relavent items to category
                    //
                    AddChildren(resources, d => CategorySearchPredicate(d, enResourceType.Service, category),
                                categoryVM, environment);
                }

                //
                // Create category under worker service root 
                //
                var categorySourceItems =
                    resources.Where(res => CategorySearchPredicate(res, enResourceType.Source, cat));
                categoryVM = new NavigationItemViewModel(cat, StringResources.Navigation_Folder_Icon_Pack_Uri,
                                                         sourceServiceRoot, categorySourceItems, false, false, null,
                                                         false, true, environment, ChildCountPredicate);

                if (!categorySourceItems.Any() && !cat.Equals(UnassignedCategory)) continue;

                sourceServiceRoot.Children.Add(categoryVM);

                //
                // Add relavent items to category
                //
                AddChildren(resources, d => CategorySearchPredicate(d, enResourceType.Source, category),
                            categoryVM, environment);
            }

            //
            // Ensure the child count is upto date
            //
            environmentNavigationItemViewModel.UpdateChildCount();
            SubscribeToChecked(environmentNavigationItemViewModel);
        }

        private static void ClearChildren(NavigationItemViewModel node)
        {
            foreach (NavigationItemViewModel child in node.Children)
            {
                child.Dispose();
            }

            node.Children.Clear();
        }

        /// <summary>
        ///     Rebuilds all the trees for all environments
        /// </summary>
        private void RebuildAllNavigationItemViewModels()
        {
            foreach (var environment in _environments)
            {
                BuildNavigationItemViewModels(environment);
            }
        }

        private void AddChildren(IEnumerable<IResourceModel> items, Func<IResourceModel, bool> predicate,
                                 NavigationItemViewModel parent, IEnvironmentModel environment)
        {
            if (items == null)
            {
                return;
            }

            parent.DisplayName = parent.Name;

            items
                .Where(predicate)
                .OrderBy(c => c.ResourceName)
                .ToList()
                .ForEach(resource => AddChild(resource, parent, environment));
        }

        private void AddChild(IResourceModel resource, NavigationItemViewModel parent, IEnvironmentModel environment)
        {
            var iconPath = string.Empty;

            switch (resource.ResourceType)
            {
                case enResourceType.Source:
                    iconPath = string.IsNullOrEmpty(resource.IconPath)
                                   ? StringResources.Navigation_Source_Icon_Pack_Uri
                                   : resource.IconPath;
                    break;



            var resource = navigationViewModelBase.DataContext as IContextualResourceModel;

            return resource != null && !WizardEngine.IsResourceWizard(resource);
        }

        /// <summary>
        ///     Updates an item with in the current NavigationItemViewModel graph
        /// </summary>
        private void UpdateResource(object resource)
        {
            var resourceModel = resource as IContextualResourceModel;
            if (resourceModel == null)
            {
                // Do nothing
            }
            else if (_environments.Contains(resourceModel.Environment, EnvironmentModelEqualityComparer.Current))
            {
                //
                // Get the NavigationItemViewModel for the resource to add/update
                //
                var resourceModelEqualityComparer = new ResourceModelEqualityComparer();
                var resourceNavigationItemViewModel =
                    _root.GetChild(
                        n => resourceModelEqualityComparer.Equals(n.DataContext as IResourceModel, resourceModel)) as
                    NavigationItemViewModel;

                //
                // Get the NavigationItemViewModel for the environment the resource belongs to
                //
                var environmentNavigationItemViewModel =
                    GetNavigationItemViewModelFromEnvironment(resourceModel.Environment, false);

                //
                // Get name of the cataegory the new resource will be put in
                //
                var resourceCategoryName = GetCategoryName(resourceModel.Category);

                if (resourceNavigationItemViewModel != null && resourceModel.Environment != null)
                {
                    //
                    // If resource should be shown given the current search criteria
                    //
                    if (CategorySearchPredicate(resourceModel, enResourceType.WorkflowService, resourceCategoryName) ||
                        CategorySearchPredicate(resourceModel, enResourceType.Service, resourceCategoryName) ||
                        CategorySearchPredicate(resourceModel, enResourceType.Source, resourceCategoryName))
                    {
                        //
                        // Get the typeRootNavigationItemViewModel for the resource model
                        //
                        var typeRootNavigationItemViewModel = environmentNavigationItemViewModel.GetChild(n =>
                            {
                                var match = false;
                                var typeRootResources = n.DataContext as IEnumerable<IResourceModel>;
                                if (typeRootResources != null)
                                {
                                    match = typeRootResources.Contains(resourceModel, resourceModelEqualityComparer);
                                }

                                return match;
                            }) as NavigationItemViewModel;

                        if (typeRootNavigationItemViewModel != null)
                        {
                            //
                            // Update the name
                            //
                            resourceNavigationItemViewModel.Name = resourceModel.ResourceName;
                            resourceNavigationItemViewModel.DisplayName = resourceModel.ResourceName;
                            resourceNavigationItemViewModel.IconPath = resourceModel.IconPath;

                            //
                            // Get the NavigationItemViewModel for the category the resource originally belonged to
                            //
                            var originalCategoryNavigationItemViewModel =
                                resourceNavigationItemViewModel.Parent as NavigationItemViewModel;

                            //
                            // Check if the resource need to be moved to a different category
                            //
                            if (originalCategoryNavigationItemViewModel != null &&
                                originalCategoryNavigationItemViewModel.IsCategory &&
                                resourceCategoryName.ToLower() != originalCategoryNavigationItemViewModel.Name.ToLower())
                            {
                                //
                                // Get the NavigationItemViewModel for the category the resource needs to be moved to
                                //
                                var newCategoryNavigationItemViewModel =
                                    typeRootNavigationItemViewModel.GetChild(
                                        n => n.Name.ToLower() == resourceCategoryName.ToLower()) as
                                    NavigationItemViewModel;

                                if (newCategoryNavigationItemViewModel == null)
                                {
                                    //
                                    // Create new category
                                    //
                                    var categoryWorkflowItems =
                                        resourceModel.Environment.Resources.All()
                                                     .Where(
                                                         res =>
                                                         res.ResourceType == enResourceType.WorkflowService &&
                                                         res.Category.Equals(resourceCategoryName,
                                                                             StringComparison.InvariantCultureIgnoreCase) &&
                                                         res.ResourceName.ToUpper().Contains(SearchFilter.ToUpper()));
                                    newCategoryNavigationItemViewModel =
                                        new NavigationItemViewModel(resourceCategoryName,
                                                                    StringResources.Navigation_Folder_Icon_Pack_Uri,
                                                                    typeRootNavigationItemViewModel,
                                                                    categoryWorkflowItems, false, false, null, false,
                                                                    true, resourceModel.Environment, ChildCountPredicate);

                                    var categoryInsertionIndex =
                                        GetInsertionindex(
                                            typeRootNavigationItemViewModel.Children.Select(n => n.Name).ToList(),
                                            newCategoryNavigationItemViewModel.Name);

                                    newCategoryNavigationItemViewModel.Parent = typeRootNavigationItemViewModel;
                                    typeRootNavigationItemViewModel.Children.Insert(categoryInsertionIndex,
                                                                                    newCategoryNavigationItemViewModel);
                                }

                                //
                                // Move Category
                                //
                                resourceNavigationItemViewModel.Parent = null;
                                originalCategoryNavigationItemViewModel.Children.Remove(resourceNavigationItemViewModel);

                                var resourceInsertionIndex =
                                    GetInsertionindex(
                                        newCategoryNavigationItemViewModel.Children.Select(n => n.Name).ToList(),
                                        resourceNavigationItemViewModel.Name);

                                resourceNavigationItemViewModel.Parent = newCategoryNavigationItemViewModel;
                                newCategoryNavigationItemViewModel.Children.Insert(resourceInsertionIndex,
                                                                                   resourceNavigationItemViewModel);

                                //
                                // Remove category empty
                                //
                                if (originalCategoryNavigationItemViewModel.Children.Count == 0)
                                {
                                    typeRootNavigationItemViewModel.Children.Remove(
                                        originalCategoryNavigationItemViewModel);
                                }
                            }
                        }
                    }
                }
                else if (resourceModel.Environment != null)
                {
                    //Add code here to update a new item
                }
            }
        }

        /// <summary>
        ///     Calculates the index at which an item should be insered to maintain an alpha numeric order
        /// </summary>
        private static int GetInsertionindex(IEnumerable<string> list, string newItem)
        {
            var orderedList = list.ToList();
            var unassignedPresent = orderedList.Remove(UnassignedCategory);

            orderedList.Add(newItem);
            orderedList = orderedList.OrderBy(s => s).ToList();

            var index = orderedList.IndexOf(newItem);

            if (unassignedPresent)
            {
                index += 1;
            }

            return index;
        }

        /// <summary>
        ///     Formats a category name for display
        /// </summary>
        private static string GetCategoryName(string categoryName)
        {
            return string.IsNullOrEmpty(categoryName) ? UnassignedCategory : categoryName.ToUpper();
        }

        /// <summary>
        ///     Determines if a resource meets the current search criteria for a root type
        /// </summary>
        private bool RootSearchPredicate(IResourceModel resource, enResourceType resourceType)
        {
            var contextualResource = resource as IContextualResourceModel;

            if (contextualResource == null || WizardEngine.IsResourceWizard(contextualResource))
            {
                return false;
            }

            return resource.ResourceType == resourceType &&
                   resource.ResourceName.ToUpper().Contains(SearchFilter.ToUpper());
        }

        /// <summary>
        ///     Determines if a resource meets the current search criteria for a category
        /// </summary>
        private bool CategorySearchPredicate(IResourceModel resource, enResourceType resourceType, string category)
        {
            var contextualResource = resource as IContextualResourceModel;

            if (contextualResource == null || WizardEngine.IsResourceWizard(contextualResource))
            {
                return false;
            }

            return resource.Category.Equals(category, StringComparison.InvariantCultureIgnoreCase) &&
                   resource.ResourceType == resourceType &&
                   resource.ResourceName.ToUpper().Contains(SearchFilter.ToUpper());
        }

        /// <summary>
        ///     Subscribes to the checked event of a item and all it's children
        /// </summary>
        private void SubscribeToChecked(NavigationItemViewModelBase navigationItemViewModel)
        {
            navigationItemViewModel.Checked -= NavigationItemViewModel_Checked;
            navigationItemViewModel.Checked += NavigationItemViewModel_Checked;

            foreach (var child in navigationItemViewModel.Children)
            {
                SubscribeToChecked(child);
            }
        }

        /// <summary>
        ///     Unsubscribes to the checked event of an item and all it's children
        /// </summary>
        private void UnsubscribeFromChecked(NavigationItemViewModelBase navigationItemViewModel)
        {
            navigationItemViewModel.Checked -= NavigationItemViewModel_Checked;

            foreach (var child in navigationItemViewModel.Children)
            {
                UnsubscribeFromChecked(child);
            }
        }

        /// <summary>
        ///     Checks or unchecks an item and all it's children without raising the checked event
        /// </summary>
        private static void CheckItemAndChildren(NavigationItemViewModelBase navigationItemViewModel, bool isChecked)
        {
            navigationItemViewModel.SilentlyCheck(isChecked);

            foreach (var child in navigationItemViewModel.Children)
            {
                CheckItemAndChildren(child, isChecked);
            }
        }

        /// <summary>
        ///     Connects to a server considering the auxilliry connection field.
        /// </summary>
        private void Connect(IEnvironmentModel environment)
        {
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
                catch
                {
                    //TODO show that connection failed.
                }
                finally
                {
                    if (disconnectFromPrimary && primaryEnvironment.IsConnected)
                    {
                        primaryEnvironment.Disconnect();
                    }
                }
            }
            else
            {
                environment.Connect();
            }
        }

        #endregion Private Methods

        #region Event Handler Methods

        /// <summary>
        ///     Handles the checked event of an item
        /// </summary>
        private void NavigationItemViewModel_Checked(object sender, EventArgs e)
        {
            var navigationItemViewModelBase = sender as NavigationItemViewModelBase;

            if (navigationItemViewModelBase != null)
            {
                CheckItemAndChildren(navigationItemViewModelBase, navigationItemViewModelBase.IsChecked);
            }

            OnItemsChecked();
        }

        #endregion Event Handler Methods
    }
}