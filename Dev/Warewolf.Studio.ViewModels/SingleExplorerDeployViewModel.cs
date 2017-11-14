using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Data.ServiceModel;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Deploy;
using Dev2.Common.Interfaces.Threading;
using Dev2.Threading;
using System.Globalization;

namespace Warewolf.Studio.ViewModels
{
    public class SingleExplorerDeployViewModel : BindableBase, IDeployViewModel, IUpdatesHelp
    {
        IDeploySourceExplorerViewModel _source;
        readonly IDeployStatsViewerViewModel _stats;
        IDeployDestinationExplorerViewModel _destination;
        IConnectControlViewModel _sourceconnectControlViewModel;
        IConnectControlViewModel _destinationConnectControlViewModel;
        string _sourcesCount;
        string _servicesCount;
        string _newResourcesCount;
        string _overridesCount;
        bool _showConflicts;
        bool _isDeploying;
        bool _deployInProgress;
        bool _deploySuccessfull;
        string _conflictNewResourceText;
        readonly IShellViewModel _shell;
        public virtual IPopupController PopupController { get; set; }
        bool _showNewItemsList;
        bool _showConflictItemsList;
        IList<Conflict> _conflictItems;
        IList<IExplorerTreeItem> _newItems;
        string _errorMessage;
        string _deploySuccessMessage;

        #region Implementation of IDeployViewModel       

        public SingleExplorerDeployViewModel(IDeployDestinationExplorerViewModel destination, IDeploySourceExplorerViewModel source, IEnumerable<IExplorerTreeItem> selectedItems, IDeployStatsViewerViewModel stats, IShellViewModel shell, IPopupController popupController)
            : this(destination, source, selectedItems, stats, shell, popupController, null)
        {
        }

        public SingleExplorerDeployViewModel(IDeployDestinationExplorerViewModel destination, IDeploySourceExplorerViewModel source, IEnumerable<IExplorerTreeItem> selectedItems, IDeployStatsViewerViewModel stats, IShellViewModel shell, IPopupController popupController, IAsyncWorker asyncWorker)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "destination", destination }, { "source", source }, { "selectedItems", selectedItems }, { "stats", stats }, { "popupController", popupController } });
            _destination = destination;
            
            PopupController = popupController;

            _source = source;
            _errorMessage = "";
            _source.Preselected = selectedItems;
            _stats = stats;
            _shell = shell;
            _stats.CalculateAction = () =>
            {
                ServicesCount = _stats.Services.ToString();
                SourcesCount = _stats.Sources.ToString();
                NewResourcesCount = _stats.NewResources.ToString();
                OverridesCount = _stats.Overrides.ToString();
                ConflictItems = _stats.Conflicts;
                NewItems = _stats.New;
                ShowConflicts = false;
                ViewModelUtils.RaiseCanExecuteChanged(DeployCommand);
            };
            SourceConnectControlViewModel = _source.ConnectControlViewModel;
            DestinationConnectControlViewModel = _destination.ConnectControlViewModel;

            SourceConnectControlViewModel.SelectedEnvironmentChanged += UpdateServerCompareChanged;
            DestinationConnectControlViewModel.SelectedEnvironmentChanged += UpdateServerCompareChanged;
            DeployCommand = new DelegateCommand(Deploy, () => CanDeploy);
            SelectDependenciesCommand = new DelegateCommand(SelectDependencies, () => CanSelectDependencies);
            NewResourcesViewCommand = new DelegateCommand(ViewNewResources);
            OverridesViewCommand = new DelegateCommand(ViewOverrides);
            Destination.ServerStateChanged += DestinationServerStateChanged;
            Destination.PropertyChanged += DestinationOnPropertyChanged;
            ShowConflicts = false;
        }

        private void DestinationOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "IsConnected")
            {
                ViewModelUtils.RaiseCanExecuteChanged(DeployCommand);
            }
        }

        void DestinationServerStateChanged(object sender, IServer server)
        {
            RaiseCanExecuteDependencies();
            var items = !DestinationConnectControlViewModel.IsConnected ? _source?.SourceLoadedItems?.ToList() : GetItemsToUpdateStats();
            _stats.Calculate(items);
        }

        private List<IExplorerTreeItem> GetItemsToUpdateStats()
        {
            var items = Source?.SourceLoadedItems?.ToList();
            if (Source?.SelectedItems?.Count > 0)
            {
                items = Source?.SelectedItems?.ToList();
            }

            return items;
        }

        public bool CanSelectDependencies
        {
            get
            {
                var canSelectDependencies = Source.SelectedItems.Count > 0;
                _canDeployTests = canSelectDependencies;
                OnPropertyChanged(() => CanDeployTests);
                return canSelectDependencies;
            }
        }
        private bool _canDeployTests;
        public bool CanDeployTests => _canDeployTests;

        public IList<IExplorerTreeItem> NewItems
        {
            get
            {
                return _newItems;
            }
            private set
            {
                _newItems = value;
                OnPropertyChanged(() => NewItems);
            }
        }

        public IList<Conflict> ConflictItems
        {
            get
            {
                return _conflictItems;
            }
            private set
            {
                _conflictItems = value;
                OnPropertyChanged(() => ConflictItems);
            }
        }

        void UpdateServerCompareChanged(object sender, Guid environmentid)
        {
            ShowConflicts = false;
            ServicesCount = _stats.Services.ToString();
            SourcesCount = _stats.Sources.ToString();
            NewResourcesCount = _stats.NewResources.ToString();
            OverridesCount = _stats.Overrides.ToString();
            IEnvironmentViewModel environmentViewModel = Destination?.Environments?.FirstOrDefault(model => model.ResourceId == environmentid);
            if (environmentViewModel != null)
            {
                _destination.SelectedEnvironment = environmentViewModel;
            }
            ViewModelUtils.RaiseCanExecuteChanged(DeployCommand);
            _stats.Calculate(Source?.SourceLoadedItems?.ToList());
            OnPropertyChanged(() => CanDeploy);
        }

        void ViewOverrides()
        {
            ShowConflicts = true;
            ConflictNewResourceText = "List of Overrides";
            ShowNewItemsList = false;
            ShowConflictItemsList = true;
        }

        public bool ShowConflictItemsList
        {
            get
            {
                return _showConflictItemsList;
            }
            private set
            {
                _showConflictItemsList = value;
                OnPropertyChanged(() => ShowConflictItemsList);
            }
        }

        public bool ShowNewItemsList
        {
            get
            {
                return _showNewItemsList;
            }
            private set
            {
                _showNewItemsList = value;
                OnPropertyChanged(() => ShowNewItemsList);
            }
        }

        public string ConflictNewResourceText
        {
            get
            {
                return _conflictNewResourceText;
            }
            private set
            {
                _conflictNewResourceText = value;
                OnPropertyChanged(() => ConflictNewResourceText);
            }
        }

        public bool ShowConflicts
        {
            get
            {
                return _showConflicts;
            }
            private set
            {
                _showConflicts = value;
                OnPropertyChanged(() => ShowConflicts);
            }
        }

        void ViewNewResources()
        {
            ShowConflicts = true;
            ConflictNewResourceText = "List of New Resources";
            ShowNewItemsList = true;
            ShowConflictItemsList = false;
        }

        async void Deploy()
        {
            IsDeploying = true;
            DeployInProgress = true;

            try
            {
                ErrorMessage = "";
                CheckVersionConflict();
                if (!IsDeploying)
                {
                    DeployInProgress = false;
                    return;
                }
                if (!ValidateDeployConflicts())
                {
                    ViewOverrides();
                    IsDeploying = false;
                    DeployInProgress = false;
                    return;
                }
                var destEnv = Destination.ConnectControlViewModel.SelectedConnection;
                if (destEnv?.ProxyLayer?.UpdateManagerProxy != null && ConflictItems != null)
                {
                    foreach (var conflictItem in ConflictItems)
                    {
                        await destEnv.ProxyLayer.UpdateManagerProxy.MoveItem(conflictItem.DestinationId, conflictItem.DestinationName, conflictItem.SourceName).ConfigureAwait(true);
                    }
                }

                var notfolders = GetNotFoldersList();
                ValidateDirectDeploy(notfolders);
                await Destination.RefreshSelectedEnvironment().ConfigureAwait(true);
                UpdateDeploySuccess(notfolders);
            }
            catch (Exception e)
            {
                ErrorMessage = "Deploy error. " + e.Message;
            }
            IsDeploying = false;
            DeployInProgress = false;
        }

        private bool ValidateDeployConflicts()
        {
            if (ConflictItems != null && ConflictItems.Count >= 1)
            {
                var msgResult = PopupController.ShowDeployConflict(ConflictItems.Count);
                if (msgResult == MessageBoxResult.Yes || msgResult == MessageBoxResult.OK)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        private void UpdateDeploySuccess(List<Guid> notfolders)
        {
            DeploySuccessfull = true;
            DeploySuccessMessage = $"{notfolders.Count} Resource{(notfolders.Count == 1 ? "" : "s")} Deployed Successfully.";
            var showDeploySuccessful = PopupController.ShowDeploySuccessful(DeploySuccessMessage);
            if (showDeploySuccessful == MessageBoxResult.OK)
            {
                DeploySuccessfull = false;
            }
            UpdateServerCompareChanged(this, Guid.Empty);
            _stats.ReCalculate();
        }

        private void ValidateDirectDeploy(List<Guid> notfolders)
        {
            var destEnv = Destination.ConnectControlViewModel.SelectedConnection;
            var sourceEnv = Source.Environments.First();
            if (destEnv == null || sourceEnv == null)
            {
                return;
            }
            var deployResponse = new List<IDeployResult>();
            bool supportsDirectServerDeploy = GetServerInformation();
            if (supportsDirectServerDeploy)
            {
                var destConnection = CreateNewConnection(destEnv);
                deployResponse = sourceEnv?.Server?.UpdateRepository?.Deploy(notfolders, Destination.DeployTests, destConnection);
            }
            if (!supportsDirectServerDeploy || deployResponse.Any(r => r.HasError))
            {
                _shell.DeployResources(sourceEnv.Server.EnvironmentID, destEnv.EnvironmentID, notfolders, Destination.DeployTests);
            }
            Source.SelectedEnvironment.AsList().Apply(o => o.IsResourceChecked = false);
            Source.SelectedEnvironment.IsResourceChecked = false;
        }

        private bool GetServerInformation()
        {
            var serverInformation = Source.SelectedServer.GetServerInformation();
            var supportsDirectServerDeploy = false;
            if (serverInformation != null)
            {
                supportsDirectServerDeploy = serverInformation.ContainsKey("SupportsDirectServerDeploy") && bool.Parse(serverInformation["SupportsDirectServerDeploy"]);
            }

            return supportsDirectServerDeploy;
        }

        private static Connection CreateNewConnection(IServer destEnv)
        {
            return new Connection
            {
                Address = destEnv.Connection.AppServerUri.ToString(),
                AuthenticationType = destEnv.Connection.AuthenticationType,
                UserName = destEnv.Connection.UserName,
                Password = destEnv.Connection.Password
            };
        }

        private List<Guid> GetNotFoldersList()
        {
            var selectedItems = Source.SelectedItems.Where(a => a.ResourceType != "Folder");
            var explorerTreeItems = selectedItems as IExplorerTreeItem[] ?? selectedItems.ToArray();
            var notfolders = explorerTreeItems.Select(a => a.ResourceId).ToList();
            return notfolders;
        }
        
        void CheckVersionConflict()
        {
            var sourceVersionNumber = Source.ServerVersion;
            var destMinVersionNumber = Destination.MinSupportedVersion;
            var destVersionNumber = Destination.ServerVersion;

            if (sourceVersionNumber != null && destVersionNumber != null)
            {
                if (sourceVersionNumber < destMinVersionNumber)
                {
                    var msgResult = PopupController.ShowDeployServerMinVersionConflict(sourceVersionNumber.ToString(), destMinVersionNumber.ToString());
                    if (msgResult == MessageBoxResult.Cancel)
                    {
                        IsDeploying = false;
                        return;
                    }
                }
                if (sourceVersionNumber > destVersionNumber)
                {
                    var msgResult = PopupController.ShowDeployServerVersionConflict(sourceVersionNumber.ToString(), destVersionNumber.ToString());
                    if (msgResult == MessageBoxResult.Cancel)
                    {
                        IsDeploying = false;
                        return;
                    }
                }
            }
            CheckResourceNameConflict();
        }

        void CheckResourceNameConflict()
        {
            var selected = Source.SelectedItems.Where(a => a.ResourceType != "Folder");

            var itemViewModels = _destination.SelectedEnvironment.AsList();
            var conf = from b in itemViewModels
                       join explorerTreeItem in selected on b.ResourcePath.ToUpper(CultureInfo.InvariantCulture) equals explorerTreeItem.ResourcePath.ToUpper(CultureInfo.InvariantCulture)
                       where b.ResourceId != explorerTreeItem.ResourceId
                       select b;

            var explorerItemViewModels = conf as IExplorerItemViewModel[] ?? conf.ToArray();
            if (explorerItemViewModels.Any())
            {
                var msgResult = PopupController.ShowDeployResourceNameConflict(string.Join("; ", explorerItemViewModels.Select(a => a.ResourcePath)));
                if (msgResult == MessageBoxResult.OK)
                {
                    IsDeploying = false;
                }
            }
        }

        private void SelectDependencies()
        {
            if (Source?.SelectedEnvironment?.Server != null)
            {
                var guids = Source.SelectedEnvironment.Server.QueryProxy.FetchDependenciesOnList(Source.SelectedItems.Select(a => a.ResourceId));
                var explorerItemViewModels = Source.SelectedEnvironment.UnfilteredChildren.Flatten(model => model.UnfilteredChildren);
                explorerItemViewModels.Where(a => guids.Contains(a.ResourceId)).Apply(a => a.IsResourceChecked = true);
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
                OnPropertyChanged(() => DeploySuccessfull);
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
                OnPropertyChanged(() => IsDeploying);
                OnPropertyChanged(() => CanDeploy);
            }
        }

        /// <summary>
        /// Used to indicate if a deploy is in progress
        /// </summary>
        public bool DeployInProgress
        {
            get
            {
                return _deployInProgress;
            }
            private set
            {
                _deployInProgress = value;
                OnPropertyChanged(() => DeployInProgress);
            }
        }
        /// <summary>
        /// Can Deploy test to enable button
        /// </summary>
        private bool CanDeploy
        {
            get
            {
                if (IsDeploying)
                {
                    return false;
                }
                if (!SetErrorMessage())
                {
                    return false;
                }
                ErrorMessage = string.Empty;
                return true;
            }
        }

        private bool SetErrorMessage()
        {
            if (!SetDeploySourceNotConnectedMessage())
            {
                return false;
            }
            if (!SetDeployDestinationNotConnectedMessage())
            {
                return false;
            }
            if (Source.SelectedEnvironment.Server.EnvironmentID == Destination.ConnectControlViewModel.SelectedConnection.EnvironmentID)
            {
                ErrorMessage = Resources.Languages.Core.DeploySourceDestinationAreSame;
                return false;
            }

            if (Source.SelectedItems == null || Source.SelectedItems.Count <= 0)
            {
                ErrorMessage = Resources.Languages.Core.DeployNoResourcesSelected;
                return false;
            }

            if (!SetDeployPermissionsErrorMessage())
            {
                return false;
            }
            return true;
        }

        private bool SetDeploySourceNotConnectedMessage()
        {
            if (SourceConnectControlViewModel.SelectedConnection != null && !SourceConnectControlViewModel.SelectedConnection.IsConnected)
            {
                ErrorMessage = Resources.Languages.Core.DeploySourceNotConnected;
                return false;
            }
            if (Source.SelectedEnvironment == null || !Source.SelectedEnvironment.IsConnected)
            {
                ErrorMessage = Resources.Languages.Core.DeploySourceNotConnected;
                return false;
            }
            return true;
        }

        private bool SetDeployDestinationNotConnectedMessage()
        {
            if (Destination.SelectedEnvironment == null || !Destination.ConnectControlViewModel.SelectedConnection.IsConnected)
            {
                ErrorMessage = Resources.Languages.Core.DeployDestinationNotConnected;
                return false;
            }
            if (DestinationConnectControlViewModel.SelectedConnection != null && !DestinationConnectControlViewModel.SelectedConnection.IsConnected)
            {
                ErrorMessage = Resources.Languages.Core.DeployDestinationNotConnected;
                return false;
            }
            return true;
        }

        private bool SetDeployPermissionsErrorMessage()
        {
            if (Source.ConnectControlViewModel.SelectedConnection.Permissions == null || !Source.ConnectControlViewModel.SelectedConnection.CanDeployFrom)
            {
                ErrorMessage = StringResources.SourcePermission_Error;
                return false;
            }
            if (Destination.ConnectControlViewModel.SelectedConnection.Permissions == null || !Destination.ConnectControlViewModel.SelectedConnection.CanDeployTo)
            {
                ErrorMessage = StringResources.DestinationPermission_Error;
                return false;
            }
            return true;
        }

        public string OverridesCount
        {
            get
            {
                return _overridesCount;
            }
            set
            {
                if (_overridesCount != value)
                {
                    _overridesCount = value;
                    ErrorMessage = string.Empty;
                    RaiseCanExecuteDependencies();
                    OnPropertyChanged(() => OverridesCount);
                }
            }
        }

        void RaiseCanExecuteDependencies()
        {
            var delegateCommand = SelectDependenciesCommand as DelegateCommand;
            delegateCommand?.RaiseCanExecuteChanged();

            delegateCommand = DeployCommand as DelegateCommand;
            delegateCommand?.RaiseCanExecuteChanged();
        }

        public string NewResourcesCount
        {
            get
            {
                return _newResourcesCount;
            }
            set
            {
                if (_newResourcesCount != value)
                {
                    _newResourcesCount = value;
                    ErrorMessage = string.Empty;
                    RaiseCanExecuteDependencies();
                    OnPropertyChanged(() => NewResourcesCount);
                }
            }
        }

        public string SourcesCount
        {
            get
            {
                return _sourcesCount;
            }
            set
            {
                if (_sourcesCount != value)
                {
                    _sourcesCount = value;
                    ErrorMessage = string.Empty;
                    OnPropertyChanged(() => SourcesCount);
                }
            }
        }

        public string ServicesCount
        {
            get
            {
                return _servicesCount;
            }
            set
            {
                if (_servicesCount != value)
                {
                    _servicesCount = value;
                    ErrorMessage = string.Empty;
                    OnPropertyChanged(() => ServicesCount);
                }
            }
        }

        /// <summary>
        /// source connection
        /// </summary>
        public IConnectControlViewModel SourceConnectControlViewModel
        {
            get
            {
                return _sourceconnectControlViewModel;
            }
            private set
            {
                if (Equals(value, _sourceconnectControlViewModel))
                {
                    return;
                }
                _sourceconnectControlViewModel = value;
                OnPropertyChanged(() => SourceConnectControlViewModel);
            }
        }
        /// <summary>
        /// destination connection
        /// </summary>
        public IConnectControlViewModel DestinationConnectControlViewModel
        {
            get
            {
                return _destinationConnectControlViewModel;
            }
            private set
            {
                if (Equals(value, _destinationConnectControlViewModel))
                {
                    return;
                }
                _destinationConnectControlViewModel = value;
                OnPropertyChanged(() => DestinationConnectControlViewModel);
            }
        }
        /// <summary>
        /// Source Server
        /// </summary>
        public IDeploySourceExplorerViewModel Source
        {
            get
            {
                return _source;
            }
            set
            {
                if (!Equals(_source, value))
                {
                    _source = value;
                    ShowConflicts = false;
                }
                OnPropertyChanged("Source");
            }
        }
        /// <summary>
        /// Destination Server
        /// </summary>
        public IDeployDestinationExplorerViewModel Destination
        {
            get
            {
                return _destination;
            }
            set
            {
                _destination = value;
                OnPropertyChanged("Destination");
            }
        }

        /// <summary>
        /// Overrides Hyperlink Clicked
        /// Must show list of New Resources conflicts
        /// </summary>
        public ICommand NewResourcesViewCommand { get; private set; }
        /// <summary>
        /// Overrides Hyperlink Clicked
        /// Must show list of Override conflicts
        /// </summary>
        public ICommand OverridesViewCommand { get; private set; }
        /// <summary>
        /// Deploy Button Clicked
        /// Must bring up conflict screen. Conflict screen can modify collection
        /// refresh explorer
        /// </summary>
        public ICommand DeployCommand { get; set; }
        /// <summary>
        /// Select All Dependencies. Recursive Select
        /// </summary>
        public ICommand SelectDependenciesCommand { get; set; }
        /// <summary>
        /// Stats area shows:
        ///     Service count
        ///     Workflow Count
        ///     Source Count
        ///     Unknown
        ///     New Resources in Destination
        ///     Overridden resource in Destination
        ///     Static steps of how to deploy
        /// </summary>
        public IDeployStatsViewerViewModel StatsViewModel => _stats;

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                if (!string.IsNullOrEmpty(DeploySuccessMessage) && !string.IsNullOrEmpty(value))
                {
                    DeploySuccessMessage = string.Empty;
                    DeploySuccessfull = false;
                }
                OnPropertyChanged(() => ErrorMessage);
            }
        }
        public string DeploySuccessMessage
        {
            get
            {
                return _deploySuccessMessage;
            }
            set
            {
                _deploySuccessMessage = value;
                OnPropertyChanged(() => DeploySuccessMessage);
            }
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }

        #endregion
    }
}
