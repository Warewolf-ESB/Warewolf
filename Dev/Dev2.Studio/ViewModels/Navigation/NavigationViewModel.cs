//6180 CODEREVIEW - Please region you code

#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Enums;
using Dev2.Studio.Factory;
using Dev2.Threading;
using Unlimited.Framework;
using Action = System.Action;

#endregion

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
        bool _fromActivityDrop;
        readonly IEventAggregator _eventPublisher;
        readonly IAsyncWorker _asyncWorker;

        #endregion private fields

        #region ctor + init

        public NavigationViewModel(Guid? context, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All)
            : this(context, Core.EnvironmentRepository.Instance, isFromActivityDrop, activityType)
        {
        }
      
        public NavigationViewModel(Guid? context, IEnvironmentRepository environmentRepository, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All)
            : this(EventPublishers.Aggregator, new AsyncWorker(), context, environmentRepository, isFromActivityDrop, activityType)
        {
        }

        public NavigationViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, Guid? context, IEnvironmentRepository environmentRepository, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All)
            : this(eventPublisher, ImportService.GetExportValue<IWizardEngine>(), asyncWorker, context, environmentRepository, isFromActivityDrop, activityType)
        {
            }

        public NavigationViewModel(IEventAggregator eventPublisher, IWizardEngine wizardEngine, IAsyncWorker asyncWorker, Guid? context, IEnvironmentRepository environmentRepository, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("wizardEngine", wizardEngine);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);

            _eventPublisher = eventPublisher;
            _asyncWorker = asyncWorker;
            _eventPublisher.Subscribe(this);
            EnvironmentRepository = environmentRepository;
            Context = context;

            _activityType = activityType;
            _fromActivityDrop = isFromActivityDrop;
            WizardEngine = wizardEngine;
            Environments = new List<IEnvironmentModel>();
            _root = TreeViewModelFactory.Create(eventPublisher, wizardEngine);

            var screen = _root as Screen;
            if(screen != null)
            {
                screen.Parent = this;
            }
        }


        #endregion ctor + intit

        #region public properties

        public Guid? Context { get; private set; }

        public List<IEnvironmentModel> Environments { get; private set; }

        public enDsfActivityType DsfActivityType { get { return _activityType; } set { _activityType = value; } }

        public bool IsFromActivityDrop { get { return _fromActivityDrop; } set { _fromActivityDrop = value; } }

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
            UpdateResource(message.ResourceModel);
        }

        public void Handle(RemoveNavigationResourceMessage message)
        {
            var resource = message.ResourceModel;
            var vm = Root.FindChild(resource);
            if(vm != null)
            {
                if(vm.TreeParent != null)
                {
                    vm.TreeParent.Children.Remove(vm);
                }
            }
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Adds an environment and it's resources to the tree
        /// </summary>
        public void AddEnvironment(IEnvironmentModel environment, IDesignValidationService validator = null)
        {
            if(Environments.Any(e => e.ID == environment.ID))
            {
                return;
            }
            Environments.Add(environment);

            //2013.06.02: Ashley Lewis for bugs 9444+9445 - Show disconnected environments but dont autoconnect
            if(environment.CanStudioExecute)
            {
                ITreeNode newEnvNode = TreeViewModelFactory.Create(environment, Root);
                newEnvNode.IsSelected = true;
            }
            if(environment.IsConnected)
            {
                LoadEnvironmentResources(environment);
            }
            else if(Equals(environment, EnvironmentRepository.Source))
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
                var environmentNavigationItemViewModel =
                    Find(environment, true);
                Root.Children.Remove(environmentNavigationItemViewModel);
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
                returnNavigationItemViewModel = TreeViewModelFactory.Create(environment, Root);
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
        public void UpdateResource(IResourceModel resource)
        {
            var resourceModel = resource as IContextualResourceModel;
            ITreeNode serviceTypeNode;

            var child = GetChildNode(resourceModel,out serviceTypeNode);
            var resourceNode = child as ResourceTreeViewModel;


            var newCategoryName = TreeViewModelHelper.GetCategoryDisplayName(resourceModel.Category);
            var newCategoryNode = serviceTypeNode.FindChild(newCategoryName);

            if(resourceNode != null)
            {
                var originalCategoryNode = resourceNode.TreeParent;

                //this means the category has changed
                if(newCategoryName != originalCategoryNode.DisplayName)
                {
                    // Remove from old category
                    bool test = originalCategoryNode.Remove(resourceNode);
                    //delete old category if empty
                    if(originalCategoryNode.ChildrenCount == 0)
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
                resourceNode = TreeViewModelFactory.Create(_eventPublisher, WizardEngine, resourceModel, null, WizardEngine.IsWizard(resourceModel)) as ResourceTreeViewModel;
            }

            //Juries Added - this triggers the animation to inform the user that it is new
            resourceNode.IsNew = true;

            //if not exist create category
            bool forceRefresh = false;
            if(newCategoryNode == null)
            {
                forceRefresh = true;
                newCategoryNode = TreeViewModelFactory.CreateCategory(newCategoryName,
                                                                      resourceModel.ResourceType, serviceTypeNode);
            }
            //add to category
            if(!ReferenceEquals(newCategoryNode, resourceNode.TreeParent))
            {
                newCategoryNode.Add(resourceNode);
            }

            if(forceRefresh)
            {
                UpdateSearchFilter(_searchFilter);
            }
        }

        ITreeNode GetChildNode(IContextualResourceModel resourceModel, out ITreeNode serviceTypeNode)
        {
            if(resourceModel == null)
            {
                serviceTypeNode = null;
                return null;
            }

            var environmentNode = Root.FindChild(resourceModel.Environment);
            if(environmentNode == null)
            {
                serviceTypeNode = null;
                return null;
            }

            serviceTypeNode = environmentNode.FindChild(resourceModel.ResourceType);
            if(serviceTypeNode == null)
            {
                return null;
            }

            ITreeNode child = environmentNode.FindChild(resourceModel);
            return child;
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
            _eventPublisher.Publish(new SetSelectedIContextualResourceModel(null, false));
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
        /// Clears the children.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        void ClearChildren(ITreeNode node)
        {
            node.Children = new SortedObservableCollection<ITreeNode>();
        }

        /// <summary>
        /// Builds the resources of an environment into a tree structure
        /// </summary>
        /// <param name="environment">The environment.</param>
        void BuildNavigationItemViewModels(IEnvironmentModel environment)
        {
            var environmentVM = Find(environment, true);

            if(environment == null || !environment.IsConnected || environment.ResourceRepository == null)
            {
                return;
            }

            //
            // Load the environemnts resources
            //
            var resources = environment.ResourceRepository.All();
            var contextualResources = resources.Cast<IContextualResourceModel>().ToList();

            //
            // Clear any resources currently being displayed for the environment
            //
            //ClearChildren(environmentVM);

            HashSet<ResourceTreeViewModel> preTreeViewModels = new HashSet<ResourceTreeViewModel>();

            List<ITreeNode> treeNodes = environmentVM.GetChildren(c => c.GetType() == typeof(ResourceTreeViewModel)).ToList();

            foreach(ResourceTreeViewModel resourceTreeViewModel in treeNodes)
            {
                preTreeViewModels.Add(resourceTreeViewModel);
            }

            switch(_activityType)
            {
                case enDsfActivityType.Workflow:
                    BuildCategoryTree(ResourceType.WorkflowService, environmentVM,
                                      contextualResources.Where(
                                          r => r.ResourceType == ResourceType.WorkflowService && !r.IsNewWorkflow)
                                                         .ToList(), preTreeViewModels);
                    break;
                case enDsfActivityType.Service:
                    BuildCategoryTree(ResourceType.Service, environmentVM,
                                      contextualResources.Where(r => r.ResourceType == ResourceType.Service).ToList(), preTreeViewModels);
                    break;
                case enDsfActivityType.Source:
                    BuildCategoryTree(ResourceType.Source, environmentVM,
                                      contextualResources.Where(r => r.ResourceType == ResourceType.Source).ToList(), preTreeViewModels);
                    break;
                default:
                    BuildCategoryTree(ResourceType.WorkflowService, environmentVM,
                                      contextualResources.Where(
                                          r => r.ResourceType == ResourceType.WorkflowService && !r.IsNewWorkflow)
                                                         .ToList(), preTreeViewModels);
                    BuildCategoryTree(ResourceType.Source, environmentVM,
                                      contextualResources.Where(r => r.ResourceType == ResourceType.Source).ToList(), preTreeViewModels);
                    BuildCategoryTree(ResourceType.Service, environmentVM,
                                      contextualResources.Where(r => r.ResourceType == ResourceType.Service).ToList(), preTreeViewModels);
                    UpdateSearchFilter(_searchFilter);
                    break;
            }

            foreach(ResourceTreeViewModel preResourceTreeViewModel in preTreeViewModels)
            {
                environmentVM.Remove(preResourceTreeViewModel);
            }
        }

        /// <summary>
        /// Builds the category tree.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="environmentVM">The environment VM.</param>
        /// <param name="resources">The resources.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        void BuildCategoryTree(ResourceType resourceType, ITreeNode environmentVM,
                               List<IContextualResourceModel> resources, HashSet<ResourceTreeViewModel> preResourceTreeViewModels)
        {
            ITreeNode workflowServiceRoot;
            ITreeNode serviceNode = environmentVM.FindChild(resourceType);
            if(serviceNode == null)
            {
                workflowServiceRoot =
                TreeViewModelFactory.Create(resourceType, environmentVM);
            }
            else
            {
                workflowServiceRoot = serviceNode;
            }

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

                List<IContextualResourceModel> categoryWorkflowItems = new List<IContextualResourceModel>();

                //
                // Create category under workflow service root 
                //
                foreach(IContextualResourceModel contextualResourceModel in resources)
                {
                    if(CategorySearchPredicate(contextualResourceModel, resourceType, categoryName))
                    {
                        ResourceTreeViewModel tmpResTreeViewModel = preResourceTreeViewModels.FirstOrDefault(r => r.DisplayName == contextualResourceModel.ResourceName);

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
                List<ITreeNode> treeNodes = workflowServiceRoot.GetChildren(x => x.GetType() == typeof(CategoryTreeViewModel)).ToList();

                ITreeNode categoryVM = treeNodes.FirstOrDefault(x => x.DisplayName == displayName);

                if(categoryVM == null)
                {
                    categoryVM = TreeViewModelFactory.CreateCategory(displayName, resourceType, workflowServiceRoot);
                }                

                AddChildren(categoryWorkflowItems, categoryVM, false);
            }
        }

        /// <summary>
        /// Adds the children.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="parent">The parent.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        void AddChildren(IEnumerable<IContextualResourceModel> items,
                                 ITreeNode parent, bool isNewResource = true, IDesignValidationService validator = null)
        {
            if(items == null)
            {
                return;
            }

            items
                .ToList()
                .ForEach(resource => AddChild(resource, parent, false, isNewResource, validator));
        }

        /// <summary>
        /// Adds a child.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="isWizard">if set to <c>true</c> [is wizard].</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        void AddChild(IContextualResourceModel resource,
                              ITreeNode parent, bool isWizard = false, bool isNewResource = true, IDesignValidationService validator = null)
        {
            if(!resource.IsNewWorkflow)
            {
                var res = TreeViewModelFactory.Create(_eventPublisher, WizardEngine, resource, parent, isWizard, isNewResource);

                if(!_fromActivityDrop)
                {
                    //
                    // Add wizard
                    //
                    if(WizardEngine.IsResourceWizard(resource))
                    {
                        return;
                    }

                    var wizardResource = WizardEngine.GetWizard(resource);
                    if(wizardResource != null)
                    {
                        AddChild(wizardResource, res, true, isNewResource);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a resource meets the current search criteria for a category
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        bool CategorySearchPredicate(IContextualResourceModel resource, ResourceType resourceType,
                                             string category)
        {
            if(resource == null || WizardEngine.IsResourceWizard(resource))
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

            //if(_useAuxiliryConnections)
            //{
            //    var primaryEnvironment =
            //        EnvironmentRepository.FindSingle(
            //            e => EnvironmentModelEqualityComparer.Current.Equals(e, environment)) ??
            //        EnvironmentModelFactory.CreateEnvironmentModel(environment);

            //    var disconnectFromPrimary = !primaryEnvironment.IsConnected;

            //    try
            //    {
            //        environment.Connect(primaryEnvironment);
            //    }
            //    // ReSharper disable EmptyGeneralCatchClause
            //    finally
            //    {
            //        if(disconnectFromPrimary && primaryEnvironment.IsConnected)
            //        {
            //            primaryEnvironment.Disconnect();
            //        }

            //        //if (!environment.IsConnected)
            //        //{
            //        //    throw new Exception("Auxiliary Connection failed.");
            //        //}
            //    }
            //}
            //else
            //{
            environment.Connect();
            //}
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

        #region ExecuteCommand

        static dynamic ExecuteCommand(IEnvironmentModel targetEnvironment, UnlimitedObject dataObj,
                                              bool convertResultToUnlimitedObject = true)
        {
            var workspaceID = targetEnvironment.Connection.WorkspaceID;
            var result = targetEnvironment.Connection.ExecuteCommand(dataObj.XmlString, workspaceID,
                                                                     GlobalConstants.NullDataListID);

            if(result == null)
            {
                dynamic tmp = dataObj;
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, tmp.Service));
            }

            if(convertResultToUnlimitedObject)
            {
                // PBI : 7913 -  Travis
                var resultObj = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);
                if(resultObj.HasError)
                {
                    throw new Exception(resultObj.Error);
                }
                return resultObj;
            }
            return result;
        }

        #endregion

        async void LoadResourcesAsync(IEnvironmentModel environment)
        {
            if(IsRefreshing || environment == null)
            {
                return;
            }

            UpdateIsRefreshing(environment, true);

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
                        BuildNavigationItemViewModels(environment);
                    }
                }
                finally
                {
                    UpdateIsRefreshing(environment, false);
                    OnLoadResourcesCompleted();
                }
            });
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
            ITreeNode serviceTypeNode;
            if(item != null)
            {
                ITreeNode childNode = GetChildNode(item, out serviceTypeNode);
                if(childNode != null)
                {
                    childNode.TreeParent.IsExpanded = true;
                    childNode.IsSelected = true;
                    childNode.NotifyOfPropertyChange("IsSelected");
                }
            }
        }
    }

}