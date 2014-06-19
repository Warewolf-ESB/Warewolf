using Caliburn.Micro;
using Dev2.AppResources.DependencyInjection.EqualityComparers;
using Dev2.AppResources.Enums;
using Dev2.AppResources.Repositories;
using Dev2.Instrumentation;
using Dev2.Messages;
using Dev2.Models;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Deploy;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.ViewModels.Deploy;
using Dev2.Views.Deploy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Deploy
// ReSharper restore CheckNamespace
{
    public class DeployViewModel : BaseWorkSurfaceViewModel,
        IHandle<UpdateDeployMessage>,
        IHandle<SelectItemInDeployMessage>, IHandle<AddServerToDeployMessage>,
        IHandle<EnvironmentDeletedMessage>
    {
        #region Class Members

        private IDeployStatsCalculator _deployStatsCalculator;

        private IEnvironmentModelProvider _serverProvider;

        private DeployNavigationViewModel _source;
        private DeployNavigationViewModel _target;

        private ObservableCollection<IEnvironmentModel> _servers;
        private IEnvironmentModel _selectedSourceServer;
        private IEnvironmentModel _selectedDestinationServer;

        private ObservableCollection<DeployStatsTO> _sourceStats;
        private ObservableCollection<DeployStatsTO> _targetStats;

        private Dictionary<string, Func<ExplorerItemModel, bool>> _sourceStatPredicates;
        private Dictionary<string, Func<ExplorerItemModel, bool>> _targetStatPredicates;

        private Guid _initialItemResourceID;
        private Guid _initialItemEnvironmentID;
        private bool _isDeploying;
        private bool _deploySuccessfull;
        private bool _initialLoad = true;

        private int _sourceDeployItemCount;
        private int _destinationDeployItemCount;
        private Guid? _destinationContext;
        private Guid? _sourceContext;

        #endregion Class Members

        #region Constructor

        public DeployViewModel()
            : this(new AsyncWorker(), ServerProvider.Instance, Core.EnvironmentRepository.Instance, EventPublishers.Aggregator, Dev2.AppResources.Repositories.StudioResourceRepository.Instance)
        {
        }

        public DeployViewModel(IAsyncWorker asyncWorker, IEnvironmentModelProvider serverProvider, IEnvironmentRepository environmentRepository, IEventAggregator eventAggregator, IStudioResourceRepository studioResourceRepository, IDeployStatsCalculator deployStatsCalculator = null, Guid? resourceID = null, Guid? environmentID = null)
            : base(eventAggregator)
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);

            if(environmentID.HasValue)
            {
                _initialItemEnvironmentID = environmentID.Value;
            }
            _initialItemResourceID = resourceID.GetValueOrDefault(Guid.Empty);
            DestinationServerHasDropped = false;
            StudioResourceRepository = studioResourceRepository;
            Initialize(asyncWorker, serverProvider, environmentRepository, eventAggregator, deployStatsCalculator);
        }

        public DeployViewModel(Guid resourceID, Guid environmentID)
            : this(new AsyncWorker(), ServerProvider.Instance, Core.EnvironmentRepository.Instance, EventPublishers.Aggregator, Dev2.AppResources.Repositories.StudioResourceRepository.Instance, null, resourceID, environmentID)
        {
        }

        #endregion

        #region Commands

        public ICommand SelectAllDependanciesCommand
        {
            get;
            private set;
        }

        public ICommand DeployCommand
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

        public Guid? SourceContext
        {
            get
            {
                return _sourceContext ?? (_sourceContext = Guid.NewGuid());
            }
        }

        public Guid? DestinationContext
        {
            get
            {
                return _destinationContext ?? (_destinationContext = Guid.NewGuid());
            }
        }

        [Import(typeof(IWindowManager))]
        public IWindowManager WindowManager { get; set; }

        public IEnvironmentRepository EnvironmentRepository { get; private set; }

        public bool CanDeploy
        {
            get
            {
                return SelectedDestinationServerIsValid() && SelectedSourceServerIsValid() && HasItemsToDeploy(_sourceDeployItemCount, _destinationDeployItemCount) && !IsDeploying && ServersAreNotTheSame;
            }
        }

        public bool CanSelectAllDependencies { get { return SelectedSourceServerIsValid(); } }

        public bool ServersAreNotTheSame
        {
            get
            {
                return (SelectedDestinationServer == null || SelectedSourceServer == null) || (SelectedDestinationServer.Connection.AppServerUri != SelectedSourceServer.Connection.AppServerUri);
            }
        }

        public Func<int, int, bool> HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => (sourceDeployItemCount > 0 && destinationDeployItemCount > 0);

        bool SelectedDestinationServerIsValid()
        {
            if(SelectedDestinationServer != null && SelectedDestinationServer.IsConnected)
            {
                return SelectedDestinationServer.IsAuthorizedDeployTo;
            }
            return true;
        }

        bool SelectedSourceServerIsValid()
        {
            if(SelectedSourceServer != null && SelectedSourceServer.IsConnected)
            {
                return SelectedSourceServer.IsAuthorizedDeployFrom;
            }
            return true;
        }

        public bool SourceItemsSelected
        {
            get
            {
                return _sourceDeployItemCount > 0;
            }
        }


        public void SourceEnvironmentConnectedChanged(object sender, ConnectedEventArgs e)
        {
            if(null != SelectedDestinationServer && null != Target && _sourceStatPredicates != null && _targetStatPredicates != null && !e.IsConnected)
            {
                Target.ClearConflictingNodesNodes();
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

        public ObservableCollection<IEnvironmentModel> Servers
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

        public DeployNavigationViewModel Source
        {
            get
            {
                return _source;
            }
            set
            {
                if(value == _source) return;

                _source = value;
                if(_selectedDestinationServer != null && _target != null && _sourceStatPredicates != null && _targetStatPredicates != null)
                {
                    CalculateStats();
                }

                NotifyOfPropertyChange(() => Source);
            }
        }

        public DeployNavigationViewModel Target
        {
            get
            {
                return _target;
            }
            set
            {
                if(value == _target) return;
                _target = value;
                NotifyOfPropertyChange(() => Target);
            }
        }

        public IEnvironmentModel SelectedSourceServer
        {
            get
            {
                return _selectedSourceServer;
            }
            set
            {
                if(value == null) return;

                if(null != _selectedSourceServer)
                {
                    _selectedSourceServer.IsConnectedChanged -= SourceEnvironmentConnectedChanged;
                }

                // ReSharper disable PossibleUnintendedReferenceComparison
                if(value != _selectedSourceServer)
                // ReSharper restore PossibleUnintendedReferenceComparison
                {
                    Target.ClearConflictingNodesNodes();
                }
                _selectedSourceServer = value;
                SourceServerHasDropped = false;
                if(_selectedSourceServer != null)
                {
                    Source.Environment = _selectedSourceServer;
                    _selectedSourceServer.IsConnectedChanged -= SelectedSourceServerIsConnectedChanged;
                    _selectedSourceServer.IsConnectedChanged += SelectedSourceServerIsConnectedChanged;
                    _selectedSourceServer.IsConnectedChanged += SourceEnvironmentConnectedChanged;

                }
                LoadSourceEnvironment();

                NotifyOfPropertyChange(() => SelectedSourceServer);
                NotifyOfPropertyChange(() => SourceItemsSelected);
                NotifyOfPropertyChange(() => CanDeploy);
                NotifyOfPropertyChange(() => ServersAreNotTheSame);
            }

        }

        public string ServerDisconnectedMessage
        {
            get
            {
                if(SourceServerHasDropped && !DestinationServerHasDropped)
                {
                    return "Source server has disconnected.";
                }
                if(!SourceServerHasDropped && DestinationServerHasDropped)
                {
                    return "Destination server has disconnected.";
                }
                if(SourceServerHasDropped && DestinationServerHasDropped)
                {
                    return "Source and Destination servers have disconnected.";
                }
                return "";
            }
        }

        void SelectedSourceServerIsConnectedChanged(object sender, ConnectedEventArgs e)
        {
            NotifyOfPropertyChange(() => SelectedSourceServer);
            SourceServerHasDropped = !SelectedSourceServer.IsConnected;
        }

        public IEnvironmentModel SelectedDestinationServer
        {
            get
            {
                return _selectedDestinationServer;
            }
            set
            {
                // ReSharper disable PossibleUnintendedReferenceComparison
                if(value != _selectedDestinationServer)
                // ReSharper restore PossibleUnintendedReferenceComparison
                {
                    _selectedDestinationServer = value;
                    DestinationServerHasDropped = false;
                    if(_selectedDestinationServer != null)
                    {
                        Target.Environment = _selectedSourceServer;
                        _selectedDestinationServer.IsConnectedChanged -= SelectedDestinationServerIsConnectedChanged;
                        _selectedDestinationServer.IsConnectedChanged += SelectedDestinationServerIsConnectedChanged;
                    }
                    LoadDestinationEnvironment();
                    EventPublisher.Publish(new SetConnectControlSelectedServerMessage(SelectedDestinationServer, ConnectControlInstanceType.DeployTarget));
                    NotifyOfPropertyChange(() => SelectedDestinationServer);
                    NotifyOfPropertyChange(() => SourceItemsSelected);
                    NotifyOfPropertyChange(() => CanDeploy);
                    NotifyOfPropertyChange(() => ServersAreNotTheSame);
                }
            }
        }

        void SelectedDestinationServerIsConnectedChanged(object sender, ConnectedEventArgs args)
        {
            NotifyOfPropertyChange(() => SelectedDestinationServer);
            DestinationServerHasDropped = !SelectedDestinationServer.IsConnected;
        }

        #endregion

        #region Private Methods

        private void Initialize(IAsyncWorker asyncWorker, IEnvironmentModelProvider serverProvider, IEnvironmentRepository environmentRepository, IEventAggregator eventAggregator, IDeployStatsCalculator deployStatsCalculator = null)
        {
            EnvironmentRepository = environmentRepository;

            _deployStatsCalculator = deployStatsCalculator ?? new DeployStatsCalculator();
            _serverProvider = serverProvider;
            _servers = new ObservableCollection<IEnvironmentModel>();
            _targetStats = new ObservableCollection<DeployStatsTO>();
            _sourceStats = new ObservableCollection<DeployStatsTO>();

            Target = new DeployNavigationViewModel(eventAggregator, asyncWorker, environmentRepository, StudioResourceRepository, true);
            Source = new DeployNavigationViewModel(eventAggregator, asyncWorker, environmentRepository, StudioResourceRepository, false);

            SetupPredicates();
            SetupCommands();
            LoadServers();
            ExplorerItemModel.OnCheckedStateChangedAction += OnCheckedStateChangedAction;
        }

        void OnCheckedStateChangedAction(CheckStateChangedArgs checkStateChangedArgs)
        {
            if(checkStateChangedArgs != null && checkStateChangedArgs.PreviousState && checkStateChangedArgs.NewState == false)
            {
                if(Target != null)
                {
                    if(checkStateChangedArgs.ResourceID != Guid.Empty)
                    {
                        if(SelectedDestinationServer != null)
                        {
                            if(SelectedDestinationServer.ResourceRepository != null)
                            {
                                var resourceModel = SelectedDestinationServer.ResourceRepository.FindSingle(model => model.ID == checkStateChangedArgs.ResourceID) as IContextualResourceModel;
                                if(resourceModel != null)
                                {
                                    Target.SetNodeOverwrite(resourceModel, false);
                                }
                            }
                        }
                    }
                }
            }
            CalculateStats();
        }

        /// <summary>
        /// Refreshes the resources for all environments
        /// </summary>
        private void RefreshEnvironments()
        {
            Source.RefreshEnvironment();
            Target.RefreshEnvironment();

        }

        /// <summary>
        /// Recalculates
        /// </summary>
        private void CalculateStats()
        {
            DeploySuccessfull = false;

            if(Source != null && Source.ExplorerItemModels != null && Source.ExplorerItemModels.Count > 0)
            {
                ExplorerItemModel explorerItemModel = Source.ExplorerItemModels[0];
                if(explorerItemModel != null)
                {
                    var items = explorerItemModel.Descendants().Where(model => model.IsChecked.GetValueOrDefault(false)).ToList();
                    _deployStatsCalculator.ConflictingResources = new ObservableCollection<DeployDialogTO>();
                    _deployStatsCalculator.CalculateStats(items, _sourceStatPredicates, _sourceStats, out _sourceDeployItemCount);
                    _deployStatsCalculator.CalculateStats(items, _targetStatPredicates, _targetStats, out _destinationDeployItemCount);
                }
            }
            NotifyOfPropertyChange(() => CanDeploy);
            NotifyOfPropertyChange(() => SourceItemsSelected);
        }


        /// <summary>
        /// Create the predicates which are to be used when generating deploy stats
        /// </summary>
        private void SetupPredicates()
        {
            // ReSharper disable ImplicitlyCapturedClosure

            var exclusionCategories = new List<string> { "Website", "Human Interface Workflow", "Webpage" };
            var blankCategories = new List<string>();

            _sourceStatPredicates = new Dictionary<string, Func<ExplorerItemModel, bool>>();
            _targetStatPredicates = new Dictionary<string, Func<ExplorerItemModel, bool>>();

            _sourceStatPredicates.Add("Services",
                                            n => _deployStatsCalculator
                                          .SelectForDeployPredicateWithTypeAndCategories
                                          (n, Data.ServiceModel.ResourceType.DbService | Data.ServiceModel.ResourceType.PluginService | Data.ServiceModel.ResourceType.WebService, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Workflows",
                                      n => _deployStatsCalculator.
                                               SelectForDeployPredicateWithTypeAndCategories
                                               (n, Data.ServiceModel.ResourceType.WorkflowService, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Sources",
                                      n => _deployStatsCalculator
                                               .SelectForDeployPredicateWithTypeAndCategories
                                               (n, Data.ServiceModel.ResourceType.DbSource | Data.ServiceModel.ResourceType.PluginSource | Data.ServiceModel.ResourceType.WebSource | Data.ServiceModel.ResourceType.ServerSource | Data.ServiceModel.ResourceType.EmailSource, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Unknown",
                                      n => _deployStatsCalculator.SelectForDeployPredicate(n));
            _targetStatPredicates.Add("New Resources",
                                      n => _deployStatsCalculator
                                               .DeploySummaryPredicateNew(n, SelectedDestinationServer));
            _targetStatPredicates.Add("Override",
                                      n => _deployStatsCalculator
                                               .DeploySummaryPredicateExisting(n, Target));
            //            if(Target.Environments.Any())
            //            {
            //                IEnvironmentModel env = Target.Environments[0];
            //                _deployStatsCalculator.ConflictingResources.ToList().ForEach(r => env.ResourceRepository.DeleteResource(r.DestinationResource));
            //            }
            // ReSharper restore ImplicitlyCapturedClosure
        }

        /// <summary>
        /// Create and assign commands
        /// </summary>
        private void SetupCommands()
        {
            DeployCommand = new RelayCommand(o => Deploy(), o => CanDeploy);
            SelectAllDependanciesCommand = new RelayCommand(SelectAllDependancies, o => CanSelectAllDependencies);
            SourceServerChangedCommand = new RelayCommand(s =>
            {
                SelectedSourceServer = s as IEnvironmentModel;
            });

            TargetServerChangedCommand = new RelayCommand(s =>
            {
                SelectedDestinationServer = s as IEnvironmentModel;
            });
        }

        /// <summary>
        /// Selects all dependancies of the selected resources
        /// </summary>
        /// <param name="obj"></param>
        void SelectAllDependancies(object obj)
        {
            if(Source != null)
            {
                if(Source.ExplorerItemModels != null)
                {
                    List<ExplorerItemModel> resourceTreeViewModels = Source.ExplorerItemModels.ToList();
                    List<ExplorerItemModel> selectedResourcesTreeViewModels = new List<ExplorerItemModel>();
                    foreach(var resourceTreeViewModel in resourceTreeViewModels)
                    {
                        if(resourceTreeViewModel != null)
                        {
                            IEnumerable<ExplorerItemModel> checkedITems = resourceTreeViewModel.Descendants().Where(model => model.IsChecked.GetValueOrDefault(false));
                            selectedResourcesTreeViewModels.AddRange(checkedITems);
                        }
                    }

                    SelectDependencies(selectedResourcesTreeViewModels);
                }
            }
        }

        /// <summary>
        /// Loads the available servers from the server provider
        /// </summary>
        private void LoadServers()
        {
            List<IEnvironmentModel> servers = _serverProvider.Load();
            Servers.Clear();

            foreach(var server in servers)
            {
                Servers.Add(server);
            }

            if(servers.Count > 0)
            {
                //
                // Find a source server to select
                //
                SelectedSourceServer = servers.FirstOrDefault(s => ServerEqualityComparer.Current.Equals(s, SelectedSourceServer));

                if(SelectedSourceServer == null && _initialLoad)
                {
                    SelectSourceServerFromInitialValue();
                }
            }
        }

        /// <summary>
        /// Adds a new server
        /// </summary>
        private void AddServer(IEnvironmentModel server, ConnectControlInstanceType connectControlInstanceType)
        {

            if(connectControlInstanceType == ConnectControlInstanceType.DeploySource && !_initialLoad)
            {
                SelectedSourceServer = server;
            }
            if(connectControlInstanceType == ConnectControlInstanceType.DeployTarget)
            {
                SelectedDestinationServer = server;
            }
            if(_initialLoad)
            {
                _initialLoad = false;
                EventPublisher.Publish(new SetConnectControlSelectedServerMessage(_selectedSourceServer, ConnectControlInstanceType.DeploySource));
            }
        }

        /// <summary>
        /// Deploys the selected items
        /// </summary>
        private void Deploy()
        {
            Tracker.TrackEvent(TrackerEventGroup.Deploy, TrackerEventName.DeployClicked);
            if(_deployStatsCalculator != null
                && _deployStatsCalculator.ConflictingResources != null
                && _deployStatsCalculator.ConflictingResources.Count > 0)
            {
                var deployDialogViewModel = new DeployDialogViewModel(_deployStatsCalculator.ConflictingResources);
                ShowDialog(deployDialogViewModel);
                if(deployDialogViewModel.DialogResult == ViewModelDialogResults.Cancel)
                {
                    return;
                }
            }

            //
            //Get the resources to deploy
            //
            if(_deployStatsCalculator != null)
            {
                var deployResourceRepo = SelectedSourceServer.ResourceRepository;

                var resourcesToDeploy = Source.ExplorerItemModels.SelectMany(model => model.Descendants()).Where(_deployStatsCalculator.SelectForDeployPredicate).Select(model => deployResourceRepo.FindSingle(resourceModel => resourceModel.ID == model.ResourceId)).ToList();

                if(HasNoResourcesToDeploy(resourcesToDeploy, deployResourceRepo))
                {
                    return;
                }
                //
                // Deploy the resources
                //
                var deployDto = new DeployDto { ResourceModels = resourcesToDeploy };

                try
                {
                    IsDeploying = true;

                    deployResourceRepo.DeployResources(SelectedSourceServer, SelectedDestinationServer, deployDto, EventPublisher);

                    //
                    // Reload the environments resources & update explorer
                    //
                    RefreshEnvironments();
                    DeploySuccessfull = true;
                }
                catch(Exception)
                {
                    DeploySuccessfull = false;
                    IsDeploying = false;
                }
                finally
                {
                    IsDeploying = false;
                }
            }
        }

        public bool DestinationServerHasDropped
        {
            get
            {
                return _destinationServerHasDropped;
            }
            set
            {
                _destinationServerHasDropped = value;
                OnPropertyChanged("DestinationServerHasDropped");
                OnPropertyChanged("ServerDisconnectedMessage");
                OnPropertyChanged("ShowServerDisconnectedMessage");
            }
        }
        public bool SourceServerHasDropped
        {
            get
            {
                return _sourceServerHasDropped;
            }
            set
            {
                _sourceServerHasDropped = value;
                OnPropertyChanged("SourceServerHasDropped");
                OnPropertyChanged("ServerDisconnectedMessage");
                OnPropertyChanged("ShowServerDisconnectedMessage");
            }
        }

        public bool ShowServerDisconnectedMessage
        {
            get
            {
                return SourceServerHasDropped || DestinationServerHasDropped;
            }
        }

        public Func<List<IResourceModel>, IResourceRepository, bool> HasNoResourcesToDeploy = (resourcesToDeploy, deployResourceRepo) =>
        {
            if(resourcesToDeploy.Count <= 0 || deployResourceRepo == null)
            {
                return true;
            }
            return false;
        };

        public Action<object> ShowDialog = deployDialogViewModel =>
        {
            var dialog = new DeployViewDialog { DataContext = deployDialogViewModel };
            dialog.ShowDialog();
        };
        bool _destinationServerHasDropped;
        bool _sourceServerHasDropped;



        /// <summary>
        /// Loads an environment for the source navigation manager
        /// </summary>
        private void LoadSourceEnvironment()
        {
            //Source.RemoveAllEnvironments();
            if(SelectedSourceServer != null)
            {
                Source.Environment = SelectedSourceServer;

                SelectAndExpandFromInitialValue();
                CalculateStats();
            }
        }


        /// <summary>
        /// Loads an environment for the target navigation manager
        /// </summary>
        private void LoadDestinationEnvironment()
        {
            if(SelectedDestinationServer != null)
            {
                Target.Environment = SelectedDestinationServer;

                SelectAndExpandFromInitialValue();
                CalculateStats();
            }
        }

        /// <summary>
        /// Selects the server from initial value.
        /// </summary>
        private void SelectSourceServerFromInitialValue()
        {
            IEnvironmentModel environment = null;
            if(_initialItemResourceID != Guid.Empty)
            {
                environment = EnvironmentRepository.FindSingle(model => model.ID == _initialItemEnvironmentID);
            }
            if(environment != null)
            {
                var server = Servers.FirstOrDefault(s => ServerEqualityComparer.Current.Equals(s, environment));
                //
                // Setting the SelectedSourceServer will run the LoadSourceEnvironment method, 
                // which takes care of selecting and expanding the correct node
                //
                SelectedSourceServer = server;
            }
        }

        private IStudioResourceRepository StudioResourceRepository
        {
            get;
            set;
        }

        /// <summary>
        /// Selects the and expand the correct node from the from initial navigation item view model.
        /// </summary>
        private void SelectAndExpandFromInitialValue()
        {
            ExplorerItemModel navigationItemViewModel = null;

            if(_initialItemResourceID != Guid.Empty)
            {
                navigationItemViewModel = StudioResourceRepository.FindItem(model => model.EnvironmentId == _initialItemEnvironmentID && model.ResourceId == _initialItemResourceID);
            }

            if(navigationItemViewModel == null) return;

            //
            // Select and expand the initial node
            //
            navigationItemViewModel.IsChecked = true;

            var parent = navigationItemViewModel.Parent;
            if(parent != null)
            {
                parent.IsDeploySourceExpanded = true;
            }
        }

        #endregion Private Methods

        #region Dispose Handling

        protected override void OnDispose()
        {
            EventPublishers.Aggregator.Unsubscribe(this);
            // unsubscibe from previous source environemt
            if(null != _selectedSourceServer)
                _selectedSourceServer.IsConnectedChanged -= SourceEnvironmentConnectedChanged;
            base.OnDispose();
        }

        #endregion Dispose Handling

        #region IHandle

        public void Handle(UpdateDeployMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            RefreshEnvironments();
        }

        public void Handle(SelectItemInDeployMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            _initialItemResourceID = message.ResourceID;
            _initialItemEnvironmentID = message.EnvironmentID;
            SelectSourceServerFromInitialValue();
        }

        public void Handle(AddServerToDeployMessage message)
        {
            this.TraceInfo(message.GetType().Name);

            AddServer(message.Server, message.ConnectControlInstanceType);
        }

        public void Handle(EnvironmentDeletedMessage message)
        {
            IEnvironmentModel sourceEnvironmentModel = Source.Environment;
            IEnvironmentModel targetEnvironmentModel = Target.Environment;
            if(Source != null && sourceEnvironmentModel != null)
            {
                if(sourceEnvironmentModel.ID == message.EnvironmentModel.ID)
                {
                    Source.Environment = null;
                }
            }

            if(Target != null && targetEnvironmentModel != null)
            {
                if(targetEnvironmentModel.ID == message.EnvironmentModel.ID)
                {
                    Target.Environment = null;
                }
            }
        }

        #endregion

        #region Public Methods

        public void SelectDependencies(List<ExplorerItemModel> explorerItemModels)
        {
            if(explorerItemModels != null)
            {
                if(SelectedSourceServer != null)
                {
                    IResourceRepository resourceRepository = SelectedSourceServer.ResourceRepository;
                    if(resourceRepository != null)
                    {
                        List<IContextualResourceModel> selectedResourceModels = explorerItemModels.Select(resourceTreeViewModel => SelectedSourceServer.ResourceRepository.FindSingle(model => model.ID == resourceTreeViewModel.ResourceId) as IContextualResourceModel).ToList();
                        if(selectedResourceModels.All(model => model != null))
                        {
                            List<string> dependancyNames = resourceRepository.GetDependanciesOnList(selectedResourceModels, SelectedSourceServer);
                            foreach(var dependant in dependancyNames)
                            {
                                string dependant1 = dependant;
                                var treeNode = StudioResourceRepository.FindItem(model => model.ResourceId.ToString() == dependant1);
                                if(treeNode != null)
                                {
                                    treeNode.IsChecked = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }


}
