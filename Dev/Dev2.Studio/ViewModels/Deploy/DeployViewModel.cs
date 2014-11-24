
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.AppResources.DependencyInjection.EqualityComparers;
using Dev2.AppResources.Repositories;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.ConnectionHelpers;
using Dev2.CustomControls.Connections;
using Dev2.Instrumentation;
using Dev2.Models;
using Dev2.Runtime.Configuration.ViewModels.Base;
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

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Deploy
// ReSharper restore CheckNamespace
{
    public class DeployViewModel : BaseWorkSurfaceViewModel,
        IHandle<SelectItemInDeployMessage>,
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

        private Dictionary<string, Func<IExplorerItemModel, bool>> _sourceStatPredicates;
        private Dictionary<string, Func<IExplorerItemModel, bool>> _targetStatPredicates;

        private Guid _initialItemResourceID;
        private Guid _initialItemEnvironmentID;
        private bool _isDeploying;
        private bool _deploySuccessfull;

        private int _sourceDeployItemCount;
        private int _destinationDeployItemCount;
        private Guid? _destinationContext;
        private Guid? _sourceContext;
        private readonly Action<IEnvironmentModel> _setActive = SetActiveEnvironment;
        private readonly Func<IEnvironmentModel> _getActive = GetActiveEnvironment; 
        #endregion Class Members

        #region Constructor

        /// <summary>
        /// Default cTor
        /// </summary>
        public DeployViewModel()
            : this(new AsyncWorker(), ServerProvider.Instance, Core.EnvironmentRepository.Instance, EventPublishers.Aggregator, Dev2.AppResources.Repositories.StudioResourceRepository.Instance, null, null)
        {
        }

        // ReSharper disable TooManyDependencies
        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="asyncWorker"> async worker</param>
        /// <param name="serverProvider">server provider</param>
        /// <param name="environmentRepository"> environments </param>
        /// <param name="eventAggregator"> caliburn event handlers</param>
        /// <param name="studioResourceRepository"> studio repository </param>
        /// <param name="sourceConnectControlVm"> source server connect control</param>
        /// <param name="destinationConnectControlVm"> destination server connect control</param>
        /// <param name="deployStatsCalculator"> calculator for new overwritten totla resources</param>
        /// <param name="resourceID"> resource id</param>
        /// <param name="environmentID">environment id</param>
        /// <param name="connectControlSingleton">connect control</param>
        public DeployViewModel(IAsyncWorker asyncWorker, IEnvironmentModelProvider serverProvider, IEnvironmentRepository environmentRepository, IEventAggregator eventAggregator, IStudioResourceRepository studioResourceRepository, IConnectControlViewModel sourceConnectControlVm, IConnectControlViewModel destinationConnectControlVm, IDeployStatsCalculator deployStatsCalculator = null, Guid? resourceID = null, Guid? environmentID = null,IConnectControlSingleton connectControlSingleton = null)
            // ReSharper restore TooManyDependencies
            : base(eventAggregator)
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            if(connectControlSingleton == null)
                connectControlSingleton = ConnectControlSingleton.Instance;
            if(environmentID.HasValue)
            {
                _initialItemEnvironmentID = environmentID.Value;
            }
            _initialItemResourceID = resourceID.GetValueOrDefault(Guid.Empty);
            DestinationServerHasDropped = false;
            StudioResourceRepository = studioResourceRepository;
            Initialize(asyncWorker, serverProvider, environmentRepository, eventAggregator, connectControlSingleton, deployStatsCalculator);
            SourceConnectControlViewModel = sourceConnectControlVm ?? new ConnectControlViewModel(ChangeSourceServer, "Source Server:", false);
            TargetConnectControlViewModel = destinationConnectControlVm ?? new ConnectControlViewModel(ChangeDestinationServer, "Destination Server:", false);
            TargetConnectControlViewModel.SetTargetEnvironment();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="resourceID"> resource</param>
        /// <param name="environmentID">environment</param>
        public DeployViewModel(Guid resourceID, Guid environmentID)
            : this(new AsyncWorker(), ServerProvider.Instance, Core.EnvironmentRepository.Instance, EventPublishers.Aggregator, Dev2.AppResources.Repositories.StudioResourceRepository.Instance, null, null, null, resourceID, environmentID)
        {
        }

        #endregion

        #region Commands

        public RelayCommand SelectAllDependanciesCommand
        {
            get;
            private set;
        }

        public RelayCommand DeployCommand
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

        /// <summary>
        /// source connection
        /// </summary>
        public IConnectControlViewModel SourceConnectControlViewModel
        {
            get
            {
                return _sourceconnectControlViewModel;
            }
            set
            {
                if(Equals(value, _sourceconnectControlViewModel))
                {
                    return;
                }
                _sourceconnectControlViewModel = value;
                NotifyOfPropertyChange(() => SourceConnectControlViewModel);
            }
        }

        /// <summary>
        /// target connection
        /// </summary>
        public IConnectControlViewModel TargetConnectControlViewModel
        {
            get
            {
                return _targetConnectControlViewModel;
            }
            set
            {
                if(Equals(value, _targetConnectControlViewModel))
                {
                    return;
                }
                _targetConnectControlViewModel = value;
                NotifyOfPropertyChange(() => TargetConnectControlViewModel);
            }
        }

        /// <summary>
        /// Source context
        /// </summary>
        public Guid? SourceContext
        {
            get
            {
                return _sourceContext ?? (_sourceContext = Guid.NewGuid());
            }
        }

        /// <summary>
        /// destination context
        /// </summary>
        public Guid? DestinationContext
        {
            get
            {
                return _destinationContext ?? (_destinationContext = Guid.NewGuid());
            }
        }
        /// <summary>
        /// Caliburn windows manager
        /// </summary>
        public IWindowManager WindowManager { get; set; }

        /// <summary>
        /// Environments
        /// </summary>
        public IEnvironmentRepository EnvironmentRepository { get; private set; }

        /// <summary>
        /// Can Deploy test to enable button
        /// </summary>
        public bool CanDeploy
        {
            get
            {
                return SelectedDestinationServerIsValid() && SelectedSourceServerIsValid() && HasItemsToDeploy(_sourceDeployItemCount, _destinationDeployItemCount) && !IsDeploying && ServersAreNotTheSame;
            }
        }

        /// <summary>
        /// can select all. Enables butto.
        /// </summary>
        public bool CanSelectAllDependencies { get { return SelectedSourceServerIsValid(); } }


        /// <summary>
        /// check is source and destination are the same
        /// </summary>
        public bool ServersAreNotTheSame
        {
            get
            {

                return (SelectedDestinationServer == null || SelectedSourceServer == null) || (SelectedDestinationServer.Connection.AppServerUri != SelectedSourceServer.Connection.AppServerUri);
            }
        }

        public Func<int, int, bool> HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => (sourceDeployItemCount > 0 && destinationDeployItemCount > 0);

        /// <summary>
        /// destination is valid.
        /// </summary>
        /// <returns></returns>
        bool SelectedDestinationServerIsValid()
        {
            if(SelectedDestinationServer != null && SelectedDestinationServer.IsConnected)
            {
                
               return SelectedDestinationServer.IsAuthorizedDeployTo;
            }
           
            return false;
        }

        /// <summary>
        /// source server is valid
        /// </summary>
        /// <returns></returns>
        bool SelectedSourceServerIsValid()
        {
            if(SelectedSourceServer != null && SelectedSourceServer.IsConnected)
            {

                return SelectedSourceServer.IsAuthorizedDeployFrom;
            }
    
            return false;
        }

        /// <summary>
        /// Are anyt items selected
        /// </summary>
        public bool SourceItemsSelected
        {
            get
            {
                return _sourceDeployItemCount > 0;
            }
        }

        /// <summary>
        /// Handle source server changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SourceEnvironmentConnectedChanged(object sender, ConnectedEventArgs e)
        {
            if(null != SelectedDestinationServer && null != Target && _sourceStatPredicates != null && _targetStatPredicates != null && !e.IsConnected)
            {
                Target.ClearConflictingNodesNodes();
            }
            RaiseDeployCommandCanExecuteChanged();
        }

        void RaiseDeployCommandCanExecuteChanged()
        {
            if(Application.Current != null)
            {
                if(Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DeployCommand.RaiseCanExecuteChanged();
                    });
                }
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

        /// <summary>
        /// Available servers
        /// </summary>
        public ObservableCollection<IEnvironmentModel> Servers
        {
            get
            {
                return _servers;
            }
        }

        /// <summary>
        /// Deploy TO Target
        /// </summary>
        public ObservableCollection<DeployStatsTO> TargetStats
        {
            get
            {
                return _targetStats;
            }
        }

        /// <summary>
        /// Deploy TO Source
        /// </summary>
        public ObservableCollection<DeployStatsTO> SourceStats
        {
            get
            {
                return _sourceStats;
            }
        }

        /// <summary>
        /// Treeview for Source
        /// </summary>
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
        /// <summary>
        /// Treeview for destination
        /// </summary>
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

        /// <summary>
        /// Source Connection
        /// </summary>
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
                    _selectedSourceServer.IsConnectedChanged -= SelectedSourceServerIsConnectedChanged;
                    _selectedSourceServer.IsConnectedChanged += SelectedSourceServerIsConnectedChanged;
                    _selectedSourceServer.IsConnectedChanged += SourceEnvironmentConnectedChanged;

                }
                LoadSourceEnvironment();
                Source.Environment = _selectedSourceServer;
                NotifyOfPropertyChange(() => SelectedSourceServer);
                NotifyOfPropertyChange(() => SourceItemsSelected);
                NotifyOfPropertyChange(() => CanDeploy);
                NotifyOfPropertyChange(() => ServersAreNotTheSame);
                RaiseDeployCommandCanExecuteChanged();
            }

        }

        /// <summary>
        /// Message for disconnect
        /// </summary>
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

        /// <summary>
        /// Handle connection change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SelectedSourceServerIsConnectedChanged(object sender, ConnectedEventArgs e)
        {
            NotifyOfPropertyChange(() => SelectedSourceServer);
            SourceServerHasDropped = !SelectedSourceServer.IsConnected;
            RaiseDeployCommandCanExecuteChanged();
        }

        /// <summary>
        /// Destination environment that is selected
        /// </summary>
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
                        _selectedDestinationServer.IsConnectedChanged -= SelectedDestinationServerIsConnectedChanged;
                        _selectedDestinationServer.IsConnectedChanged += SelectedDestinationServerIsConnectedChanged;
                    }
                    LoadDestinationEnvironment();
                    NotifyOfPropertyChange(() => SelectedDestinationServer);
                    NotifyOfPropertyChange(() => SourceItemsSelected);
                    NotifyOfPropertyChange(() => CanDeploy);
                    NotifyOfPropertyChange(() => ServersAreNotTheSame);
                }
                Target.Environment = _selectedDestinationServer;
                CalculateStats();
                RaiseDeployCommandCanExecuteChanged();
            }
        }

        /// <summary>
        /// Selected destination has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void SelectedDestinationServerIsConnectedChanged(object sender, ConnectedEventArgs args)
        {
            NotifyOfPropertyChange(() => SelectedDestinationServer);
            DestinationServerHasDropped = !SelectedDestinationServer.IsConnected;
            RaiseDeployCommandCanExecuteChanged();
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Shared Ctor initialisation
        /// </summary>
        /// <param name="asyncWorker"></param>
        /// <param name="serverProvider"></param>
        /// <param name="environmentRepository"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="connectControl"></param>
        /// <param name="deployStatsCalculator"></param>
        private void Initialize(IAsyncWorker asyncWorker, IEnvironmentModelProvider serverProvider, IEnvironmentRepository environmentRepository, IEventAggregator eventAggregator, IConnectControlSingleton connectControl, IDeployStatsCalculator deployStatsCalculator = null)
        {
            EnvironmentRepository = environmentRepository;

            _deployStatsCalculator = deployStatsCalculator ?? new DeployStatsCalculator();
            _serverProvider = serverProvider;
            _servers = new ObservableCollection<IEnvironmentModel>();
            _targetStats = new ObservableCollection<DeployStatsTO>();
            _sourceStats = new ObservableCollection<DeployStatsTO>();

            Target = new DeployNavigationViewModel(eventAggregator, asyncWorker, environmentRepository, StudioResourceRepository, true, connectControl);
            Source = new DeployNavigationViewModel(eventAggregator, asyncWorker, environmentRepository, StudioResourceRepository, false, connectControl);

            SetupPredicates();
            SetupCommands();
            LoadServers();
            ExplorerItemModel.OnCheckedStateChangedAction += OnCheckedStateChangedAction;
        }

        /// <summary>
        /// handle resource checked
        /// </summary>
        /// <param name="checkStateChangedArgs"></param>
        void OnCheckedStateChangedAction(CheckStateChangedArgs checkStateChangedArgs)
        {
            if(checkStateChangedArgs != null && checkStateChangedArgs.PreviousState && checkStateChangedArgs.NewState == false)
            {
                if(Target != null)
                {
                    if(checkStateChangedArgs.ResourceId != Guid.Empty)
                    {
                        if(SelectedDestinationServer != null)
                        {
                            if(SelectedDestinationServer.ResourceRepository != null)
                            {
                                var resourceModel = SelectedDestinationServer.ResourceRepository.FindSingle(model => model.ID == checkStateChangedArgs.ResourceId) as IContextualResourceModel;
                                if(resourceModel != null)
                                {
                                    Target.SetNodeOverwrite(resourceModel, false);
                                }
                            }
                        }
                    }
                }
            }
            if (checkStateChangedArgs != null && checkStateChangedArgs.UpdateStats)
            CalculateStats();
        }

        /// <summary>
        /// Recalculates
        /// </summary>
        private void CalculateStats(bool updateSuccess = true)
        {
            if (updateSuccess)
            DeploySuccessfull = false;

            if(Source != null && Source.ExplorerItemModels != null && Source.ExplorerItemModels.Count > 0)
            {
                Source.ClearConflictingNodesNodes();
                IExplorerItemModel explorerItemModel = Source.ExplorerItemModels[0];
                if(explorerItemModel != null)
                {
                    var items = explorerItemModel.Descendants().Where(model => model.IsChecked.GetValueOrDefault(false)).ToList();
                    _deployStatsCalculator.ConflictingResources = new ObservableCollection<DeployDialogTO>();
                    
                    _deployStatsCalculator.CalculateStats(items, _sourceStatPredicates, _sourceStats, out _sourceDeployItemCount);
                    _deployStatsCalculator.CalculateStats(items, _targetStatPredicates, _targetStats, out _destinationDeployItemCount);
                    RaiseDeployCommandCanExecuteChanged();
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

            _sourceStatPredicates = new Dictionary<string, Func<IExplorerItemModel, bool>>();
            _targetStatPredicates = new Dictionary<string, Func<IExplorerItemModel, bool>>();

            _sourceStatPredicates.Add("Services",
                                            n => _deployStatsCalculator
                                          .SelectForDeployPredicateWithTypeAndCategories
                                          (n, ResourceType.DbService | ResourceType.PluginService | ResourceType.WebService, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Workflows",
                                      n => _deployStatsCalculator.
                                               SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.WorkflowService, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Sources",
                                      n => _deployStatsCalculator
                                               .SelectForDeployPredicateWithTypeAndCategories
                                               (n, ResourceType.DbSource | ResourceType.PluginSource | ResourceType.WebSource | ResourceType.ServerSource | ResourceType.EmailSource, blankCategories, exclusionCategories));
            _sourceStatPredicates.Add("Unknown",
                                      n => _deployStatsCalculator.SelectForDeployPredicate(n));
            _targetStatPredicates.Add("New Resources",
                                      n => _deployStatsCalculator
                                               .DeploySummaryPredicateNew(n, SelectedDestinationServer));
            _targetStatPredicates.Add("Override",
                                      n => _deployStatsCalculator
                                               .DeploySummaryPredicateExisting(n, Target));
            // ReSharper restore ImplicitlyCapturedClosure
        }

        /// <summary>
        /// Create and assign commands
        /// </summary>
        private void SetupCommands()
        {
            DeployCommand = new RelayCommand(o => Deploy(), o => CanDeploy);
            SelectAllDependanciesCommand = new RelayCommand(SelectAllDependancies, o => CanSelectAllDependencies);
            SourceServerChangedCommand = new DelegateCommand(s =>
            {
                SelectedSourceServer = s as IEnvironmentModel;
            });

            TargetServerChangedCommand = new DelegateCommand(s =>
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
                    List<IExplorerItemModel> resourceTreeViewModels = Source.ExplorerItemModels.ToList();
                    List<IExplorerItemModel> selectedResourcesTreeViewModels = new List<IExplorerItemModel>();
                    foreach(var resourceTreeViewModel in resourceTreeViewModels)
                    {
                        if(resourceTreeViewModel != null)
                        {
                            IEnumerable<IExplorerItemModel> checkedITems = resourceTreeViewModel.Descendants().Where(model => model.IsChecked.GetValueOrDefault(false)&& model.ResourceType != ResourceType.Folder);
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

                if(SelectedSourceServer == null)
                {
                    SelectSourceServerFromInitialValue();
                }
            }
        }

        /// <summary>
        /// Change source server
        /// </summary>
        private void ChangeSourceServer(IEnvironmentModel server)
        {
            SelectedSourceServer = server;
        }

        /// <summary>
        /// Change destination server
        /// </summary>
        private void ChangeDestinationServer(IEnvironmentModel server)
        {
            SelectedDestinationServer = server;
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
                    var env = _getActive();
                    _setActive(SelectedDestinationServer);
                    deployResourceRepo.DeployResources(SelectedSourceServer, SelectedDestinationServer, deployDto, EventPublisher);
                    _setActive(env);
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

            _source.Update();
            _target.Update();
            CalculateStats(false);
        }

        /// <summary>
        /// Update active env
        /// </summary>
        /// <param name="env"></param>
        static void SetActiveEnvironment(IEnvironmentModel env)
        {
            Core.EnvironmentRepository.Instance.ActiveEnvironment = env;
        }

        /// <summary>
        /// Get active environment
        /// </summary>
        /// <returns></returns>
        static IEnvironmentModel GetActiveEnvironment()
        {
            return Core.EnvironmentRepository.Instance.ActiveEnvironment;
        }

        /// <summary>
        /// Destination server is no longer available on network
        /// </summary>
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

        /// <summary>
        /// Source server is no longer available on network
        /// </summary>
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
        /// <summary>
        /// Is disconnected message visible
        /// </summary>
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
        IConnectControlViewModel _sourceconnectControlViewModel;
        IConnectControlViewModel _targetConnectControlViewModel;

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
                if(SourceConnectControlViewModel != null)
                {
                    SourceConnectControlViewModel.UpdateActiveEnvironment(environment,false);
                    SourceConnectControlViewModel.SetTargetEnvironment();
                }
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
            IExplorerItemModel navigationItemViewModel = null;

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
        /// <summary>
        /// Handle deploy from main explorer
        /// </summary>
        /// <param name="message"></param>
        public void Handle(SelectItemInDeployMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            _initialItemResourceID = message.ResourceID;
            _initialItemEnvironmentID = message.EnvironmentID;
            SourceConnectControlViewModel.SetTargetEnvironment();

            SelectSourceServerFromInitialValue();
            var root = Source.ExplorerItemModels.FirstOrDefault();
            if (root != null)
            {
                var resourceTreeViewModels = root.Descendants().First(a => a.ResourceId == message.ResourceID);

                if (resourceTreeViewModels != null)
                {
                    resourceTreeViewModels.IsChecked = true;
                }
            }
        }
        /// <summary>
        /// handle environment deleted
        /// </summary>
        /// <param name="message"></param>
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
        /// <summary>
        /// Select dependencies. Calls server
        /// </summary>
        /// <param name="explorerItemModels"></param>
        public void SelectDependencies(List<IExplorerItemModel> explorerItemModels)
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
