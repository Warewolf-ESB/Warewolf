using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Deploy;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Studio.ViewModels.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.ViewModels.Workflow;

namespace Dev2.Studio.ViewModels.Deploy
{
    public class DeployViewModel : BaseWorkSurfaceViewModel,
        IHandle<ResourceCheckedMessage>, IHandle<UpdateDeployMessage>, IHandle<SelectItemInDeployMessage>
    {
        #region Class Members

        private DeployStatsCalculator _deployStatsCalculator;

        private IDeployService _deployService;
        private IServerProvider _serverProvider;

        private NavigationViewModel _source;
        private NavigationViewModel _target;

        private ObservableCollection<IServer> _servers;
        private IServer _selectedSourceServer;
        private IServer _selectedDestinationServer;

        private ObservableCollection<DeployStatsTO> _sourceStats;
        private ObservableCollection<DeployStatsTO> _targetStats;

        private IEnvironmentModel _sourceEnvironment;
        private IEnvironmentModel _targetEnvironment;

        private Dictionary<string, Func<ITreeNode, bool>> _sourceStatPredicates;
        private Dictionary<string, Func<ITreeNode, bool>> _targetStatPredicates;

        private AbstractTreeViewModel _initialNavigationItemViewModel;
        private IContextualResourceModel _initialResource;
        private IEnvironmentModel _initialEnvironment;
        private bool _isDeploying;
        private bool _deploySuccessfull;
        private bool _initialLoad = true;
        private bool _selectingAndExpandingFromNavigationItem;

        private int _sourceDeployItemCount;
        private int _destinationDeployItemCount;

        #endregion Class Members

        #region Constructor

        public DeployViewModel()
            : this(ServerProvider.Instance)
        {
        }

        public DeployViewModel(IServerProvider serverProvider)
        {
            Initialize(serverProvider);
        }

        public DeployViewModel(AbstractTreeViewModel navigationItemViewModel)
        {
            _initialNavigationItemViewModel = navigationItemViewModel;
            Initialize(ServerProvider.Instance);
        }

        public DeployViewModel(IContextualResourceModel resourceModel)
        {
            _initialResource = resourceModel;
            Initialize(ServerProvider.Instance);
        }

        public DeployViewModel(IEnvironmentModel environment)
        {
            _initialEnvironment = environment;
            Initialize(ServerProvider.Instance);
        }

        #endregion

        #region Commands

        public ICommand DeployCommand
        {
            get;
            private set;
        }

        public ICommand ConnectCommand
        {
            get;
            private set;
        }

        public ICommand SourceServerChangedCommand
        {
            get;
            private set;
        }

        public ICommand TargetServerChangedCommand
        {
            get;
            private set;
        }

        #endregion

        #region Properties

        public IDev2WindowManager WindowNavigationBehavior { get; private set; }

        public IFrameworkRepository<IEnvironmentModel> EnvironmentRepository { get; private set; }

        public bool CanDeploy
        {
            get
            {
                return SelectedDestinationServer != null && _sourceDeployItemCount > 0 && _destinationDeployItemCount > 0 && !IsDeploying;
            }
        }

        public bool SourceItemsSelected
        {
            get
            {
                return _sourceDeployItemCount > 0;
            }
        }

        public IEnvironmentModel SourceEnvironment
        {
            get
            {
                return _sourceEnvironment;
            }
            private set
            {
                _sourceEnvironment = value;
                NotifyOfPropertyChange(() => SourceEnvironment);
            }
        }

        public IEnvironmentModel TargetEnvironment
        {
            get
            {
                return _targetEnvironment;
            }
            private set
            {
                _targetEnvironment = value;
                NotifyOfPropertyChange(() => TargetEnvironment);
            }
        }

        /// <summary>
        /// Used to indicate a successfull deploy has happened
        /// </summary>
        public bool DeploySuccessfull
        {
            get
            {
                return _deploySuccessfull;
            }
            private set
            {
                _deploySuccessfull = value;
                NotifyOfPropertyChange(() => DeploySuccessfull);
            }
        }

        /// <summary>
        /// Used to indicate if a deploy is in progress
        /// </summary>
        public bool IsDeploying 
        {
            get
            {
                return _isDeploying;
            }
            private set
            {
                _isDeploying = value;
                NotifyOfPropertyChange(() => IsDeploying);
                NotifyOfPropertyChange(() => CanDeploy);
            }
        }

        public ObservableCollection<IServer> Servers
        {
            get
            {
                return _servers;
            }
        }

        public ObservableCollection<DeployStatsTO> TargetStats
        {
            get
            {
                return _targetStats;
            }
        }

        public ObservableCollection<DeployStatsTO> SourceStats
        {
            get
            {
                return _sourceStats;
            }
        }

        public NavigationViewModel Source
        {
            get
            {
                return _source;
            }
            set
            {
                if (value == _source) return;

                _source = value;
                NotifyOfPropertyChange(() => Source);
            }
        }

        public NavigationViewModel Target
        {
            get
            {
                return _target;
            }
            set
            {
                if (value == _target) return;
                _target = value;
                NotifyOfPropertyChange(() => Target);
            }
        }

        public IServer SelectedSourceServer
        {
            get
            {
                return _selectedSourceServer;
            }
            set
            {
                _selectedSourceServer = value;
                LoadSourceEnvironment(_selectedSourceServer);
                NotifyOfPropertyChange(() => SelectedDestinationServer);
                NotifyOfPropertyChange(() => SourceItemsSelected);
                NotifyOfPropertyChange(() => CanDeploy);
            }
        }

        public IServer SelectedDestinationServer
        {
            get
            {
                return _selectedDestinationServer;
            }
            set
            {
                if (value != _selectedDestinationServer)
                {
                    _selectedDestinationServer = value;
                    LoadDestinationEnvironment(_selectedDestinationServer);
                    NotifyOfPropertyChange(() => SelectedDestinationServer);
                    NotifyOfPropertyChange(() => SourceItemsSelected);
                    NotifyOfPropertyChange(() => CanDeploy);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize(IServerProvider serverProvider)
        {
            WindowNavigationBehavior = ImportService.GetExportValue<IDev2WindowManager>();
            EnvironmentRepository = ImportService.GetExportValue<IFrameworkRepository<IEnvironmentModel>>();

            _deployStatsCalculator = new DeployStatsCalculator();
            _deployService = new DeployService();
            _serverProvider = serverProvider;
            _servers = new ObservableCollection<IServer>();
            _targetStats = new ObservableCollection<DeployStatsTO>();
            _sourceStats = new ObservableCollection<DeployStatsTO>();

            Target = new NavigationViewModel(true);
            Source = new NavigationViewModel(true);

            SetupPredicates();
            SetupCommands();
            LoadServers();
            
//            _mediatorKeyUpdateDeploy = Mediator.RegisterToReceiveMessage(MediatorMessages.UpdateDeploy, o => RefreshEnvironments());
//            _mediatorKeySelectItemInDeploy = Mediator.RegisterToReceiveMessage(MediatorMessages.SelectItemInDeploy, o =>
//            {
//                _initialResource = o as IContextualResourceModel;
//                _initialNavigationItemViewModel = o as AbstractTreeViewModel;
//                _initialEnvironment = o as IEnvironmentModel;
//
//                SelectServerFromInitialValue();
//            });


        }

        /// <summary>
        /// Refreshes the resources for all environments
        /// </summary>
        private void RefreshEnvironments()
        {
            Source.RefreshEnvironments();
            Target.RefreshEnvironments();
        }

        /// <summary>
        /// Recalculates
        /// </summary>
        private void CalculateStats()
        {
            DeploySuccessfull = false;

            var items = _source.Root.GetChildren(null).OfType<ResourceTreeViewModel>().ToList();
            _deployStatsCalculator.CalculateStats(items, _sourceStatPredicates, _sourceStats, out _sourceDeployItemCount);
            _deployStatsCalculator.CalculateStats(items, _targetStatPredicates, _targetStats, out _destinationDeployItemCount);
            NotifyOfPropertyChange(() => CanDeploy);
            NotifyOfPropertyChange(() => SourceItemsSelected);
        }


        /// <summary>
        /// Create the predicates which are to be used when generating deploy stats
        /// </summary>
        private void SetupPredicates()
        {
            var exclusionCategories = new List<string> { "Website", "Human Interface Workflow", "Webpage" };
            var websiteCategories = new List<string> { "Website" };
            var webpageCategories = new List<string> { "Human Interface Workflow", "Webpage" };
            var blankCategories = new List<string>();

            _sourceStatPredicates = new Dictionary<string, Func<ITreeNode, bool>>();
            _targetStatPredicates = new Dictionary<string, Func<ITreeNode, bool>>();

            _sourceStatPredicates.Add("Services",
                                      n => _deployStatsCalculator
                                               .SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.Service, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Workflows",
                                      n => _deployStatsCalculator.
                                               SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.WorkflowService, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Sources",
                                      n => _deployStatsCalculator
                                               .SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.Source, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Webpages",
                                      n => _deployStatsCalculator
                                               .SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.WorkflowService, webpageCategories, blankCategories));
            _sourceStatPredicates.Add("Websites",
                                      n => _deployStatsCalculator
                                               .SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.WorkflowService, websiteCategories, blankCategories));
            _sourceStatPredicates.Add("Unknown",
                                      n => _deployStatsCalculator.SelectForDeployPredicate(n));
            _targetStatPredicates.Add("New Resources",
                                      n => _deployStatsCalculator
                                               .DeploySummaryPredicateNew(n, TargetEnvironment));
            _targetStatPredicates.Add("Override",
                                      n => _deployStatsCalculator
                                               .DeploySummaryPredicateExisting(n, TargetEnvironment));
        }

        /// <summary>
        /// Create and assign commands
        /// </summary>
        private void SetupCommands()
        {
            DeployCommand = new RelayCommand(o => Deploy(), o => CanDeploy);
            ConnectCommand = new RelayCommand(Connect);
            SourceServerChangedCommand = new RelayCommand(s =>
            {
                SelectedSourceServer = s as IServer;
            });

            TargetServerChangedCommand = new RelayCommand(s =>
            {
                SelectedDestinationServer = s as IServer;
            });
        }

        /// <summary>
        /// Loads the available servers from the server provider
        /// </summary>
        private void LoadServers()
        {
            List<IServer> servers = _serverProvider.Load();
            Servers.Clear();

            foreach (var server in servers)
            {
                Servers.Add(server);
            }

            if (servers.Count > 0)
            {
                //
                // Find a source server to select
                //
                SelectedSourceServer = servers.FirstOrDefault(s => ServerEqualityComparer.Current.Equals(s, SelectedSourceServer));

                if (SelectedSourceServer == null && _initialLoad)
                {
                    SelectServerFromInitialValue();
                    _initialLoad = false;
                }

                if (SelectedSourceServer == null)
                {
                    SelectedSourceServer = servers[0];
                }

                //
                // Find target server to select
                //
                //SelectedDestinationServer = servers.FirstOrDefault(s => ServerEqualityComparer.Current.Equals(s, SelectedDestinationServer));

                //if (SelectedDestinationServer == null)
                //{
                //    SelectedDestinationServer = servers[0];
                //}
            }
        }

        /// <summary>
        /// Adds a new server
        /// </summary>
        private void AddServer(IServer server, bool connectSource, bool connectTarget)
        {
            Servers.Add(server);

            if (connectSource)
            {
                SelectedSourceServer = server;
            }

            if (connectTarget)
            {
                SelectedDestinationServer = server;
            }
        }

        /// <summary>
        /// Deploys the selected items
        /// </summary>
        private void Deploy()
        {
            //
            //Get the resources to deploy
            //
            var resourcesToDeploy = Source.Root.GetChildren
                (_deployStatsCalculator.SelectForDeployPredicate)
                                          .Where(n => n is ResourceTreeViewModel).Cast<ResourceTreeViewModel>()
                                          .Select(n => n.DataContext as IResourceModel).ToList();

            if (resourcesToDeploy.Count <= 0 || TargetEnvironment == null) return;

            //
            // Deploy the resources
            //
            var deployDTO = new DeployDTO { ResourceModels = resourcesToDeploy };

            try
            {
                IsDeploying = true;
                _deployService.Deploy(deployDTO, TargetEnvironment);

                //
                // Reload the environments resources & update explorer
                //
                RefreshEnvironments();
                EventAggregator.Publish(new UpdateExplorerMessage(false));
                //Mediator.SendMessage(MediatorMessages.UpdateExplorer, false);
                DeploySuccessfull = true;
            }
            finally
            {
                IsDeploying = false;
            }
        }

        /// <summary>
        /// Loads an environment for the source navigation manager
        /// </summary>
        private void LoadSourceEnvironment(IServer server)
        {
            SourceEnvironment = EnvironmentModelFactory.CreateEnvironmentModel(server);

            Source.RemoveAllEnvironments();

            if (SourceEnvironment != null)
            {
                Source.AddEnvironment(SourceEnvironment);

                if (_selectingAndExpandingFromNavigationItem)
                {
                    SelectAndExpandFromInitialValue();
                }
            }

            CalculateStats();
        }

        /// <summary>
        /// Loads an environment for the target navigation manager
        /// </summary>
        private void LoadDestinationEnvironment(IServer server)
        {
            TargetEnvironment = EnvironmentModelFactory.CreateEnvironmentModel(server);

            Target.RemoveAllEnvironments();

            if (TargetEnvironment != null)
            {
                Target.AddEnvironment(TargetEnvironment);
            }

            CalculateStats();
        }

        /// <summary>
        /// Shows the connect view and acts on it's results.
        /// </summary>
        private void Connect(object o)
        {
            //
            // Create and show the connect view
            //
            var connectViewModel = new ConnectViewModel();
            WindowNavigationBehavior.ShowDialog(connectViewModel);

            if (connectViewModel.DialogResult != ViewModelDialogResults.Okay) return;

            //
            // If connect view closed with okay then create an environment, load it into the navigation view model
            //

            //
            // Add the new server
            //
            var connectSource = o == Source;
            var connectTarget = o == Target;
            AddServer(connectViewModel.Server, connectSource, connectTarget);

            //
            // Signal the explorer to update loading any new servers
            //
            EventAggregator.Publish(new UpdateExplorerMessage(false));
            //Mediator.SendMessage(MediatorMessages.UpdateExplorer, true);
        }


        /// <summary>
        /// Selects the server from initial value.
        /// </summary>
        private void SelectServerFromInitialValue()
        {
            _selectingAndExpandingFromNavigationItem = true;

            IEnvironmentModel environment = null;

            if (_initialNavigationItemViewModel != null && _initialNavigationItemViewModel.EnvironmentModel != null)
            {
                environment = _initialNavigationItemViewModel.EnvironmentModel;
            }
            else if (_initialEnvironment != null)
            {
                environment = _initialEnvironment;
            }
            else if (_initialResource != null && _initialResource.Environment != null)
            {
                environment = _initialResource.Environment;
            }

            if (environment != null)
            {
                var server = Servers.FirstOrDefault(s => ServerEqualityComparer.Current.Equals(s, environment));
                //if (server != SelectedSourceServer)
                //{
                //
                // Setting the SelectedSourceServer will run the LoadSourceEnvironment method, 
                // which takes care of selecting and expanding the correct node
                //
                SelectedSourceServer = server;
                //}
                //else
                //{
                //SelectAndExpandFromInitialNavigationItemViewModel();
                //}
            }
            _selectingAndExpandingFromNavigationItem = false;
        }

        /// <summary>
        /// Selects the and expand the correct node from the from initial navigation item view model.
        /// </summary>
        private void SelectAndExpandFromInitialValue()
        {
            ITreeNode navigationItemViewModel = null;

            if (_initialNavigationItemViewModel != null)
            {
                navigationItemViewModel = Source.Root.GetChildren(n => n.DisplayName == _initialNavigationItemViewModel.DisplayName)
                    .FirstOrDefault();
            }
            else if (_initialEnvironment != null)
            {
                navigationItemViewModel = Source.Root.GetChildren(n =>
                    {
                        var item = n as AbstractTreeViewModel;
                        return item != null
                            && EnvironmentModelEqualityComparer.Current.Equals(item.EnvironmentModel, _initialEnvironment);
                    }).FirstOrDefault();
            }
            else if (_initialResource != null && _initialResource.Environment != null)
            {
                navigationItemViewModel = Source.Root.GetChildren(n => n.DisplayName == _initialResource.ResourceName)
                    .FirstOrDefault();
            }

            if (navigationItemViewModel == null) return;

            //
            // Select and expand the initial node
            //
            navigationItemViewModel.IsChecked = true;

            var parent = navigationItemViewModel.TreeParent;
            if (parent != null)
            {
                parent.IsExpanded = true;
            }
        }

        #endregion Private Methods

        #region Dispose Handling

        protected override void OnDispose()
        {
//            Mediator.DeRegister(MediatorMessages.UpdateDeploy, _mediatorKeyUpdateDeploy);
//            Mediator.DeRegister(MediatorMessages.UpdateDeploy, _mediatorKeySelectItemInDeploy);
            EventAggregator.Unsubscribe(this);
            base.OnDispose();
        }

        #endregion Dispose Handling

        public void Handle(ResourceCheckedMessage message)
        {
            CalculateStats();
        }

        #region Implementation of IHandle<UpdateDeployMessage>

        public void Handle(UpdateDeployMessage message)
        {
            RefreshEnvironments();
        }

        #endregion

        #region Implementation of IHandle<SelectItemInDeployMessage>

        public void Handle(SelectItemInDeployMessage message)
        {
            _initialResource = message.Value as IContextualResourceModel;
            _initialNavigationItemViewModel = message.Value as AbstractTreeViewModel;
            _initialEnvironment = message.Value as IEnvironmentModel;

            SelectServerFromInitialValue();
        }

        #endregion
    }

    
}
