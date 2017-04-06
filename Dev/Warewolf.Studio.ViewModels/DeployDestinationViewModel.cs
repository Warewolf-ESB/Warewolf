using System;
using System.Linq;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Deploy;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public class DeployDestinationViewModel : ExplorerViewModel, IDeployDestinationExplorerViewModel
    {
        bool _isLoading;
        private bool _deployTests;
        private Version _serverVersion;
        public IDeployStatsViewerViewModel StatsArea { private get; set; }
        readonly IShellViewModel _shellViewModel;

        #region Implementation of IDeployDestinationExplorerViewModel

        public DeployDestinationViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
            : base(shellViewModel, aggregator,false)
        {
            _shellViewModel = shellViewModel;
            ConnectControlViewModel = new ConnectControlViewModel(_shellViewModel.LocalhostServer, aggregator, _shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers);
            ConnectControlViewModel.SelectedEnvironmentChanged += DeploySourceExplorerViewModelSelectedEnvironmentChanged;
            ConnectControlViewModel.ServerConnected += ServerConnected;
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            SelectedEnvironment = _environments.FirstOrDefault();
            RefreshCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => RefreshEnvironment(SelectedEnvironment.ResourceId));
        }

        private void ServerDisconnected(object sender, IServer server)
        {
            if (SelectedEnvironment != null)
            {
                ServerStateChanged?.Invoke(this, SelectedEnvironment.Server);
            }
        }

        private async void ServerConnected(object sender, IServer server)
        {
            var environmentViewModel = await CreateEnvironmentViewModel(sender, server.EnvironmentID, true);
            environmentViewModel?.Server?.GetServerVersion();
            environmentViewModel?.Server?.GetMinSupportedVersion();
            SelectedEnvironment = environmentViewModel;
            StatsArea?.ReCalculate();
            if (environmentViewModel != null)
            {
                AfterLoad(environmentViewModel.ResourceId);
            }
        }

        private async void DeploySourceExplorerViewModelSelectedEnvironmentChanged(object sender, Guid environmentid)
        {
            var environmentViewModel = await CreateEnvironmentViewModel(sender, environmentid);
            SelectedEnvironment = environmentViewModel;
            StatsArea?.ReCalculate();
            if (environmentViewModel != null && environmentViewModel.Children.Count <= 0)
            {
                AfterLoad(environmentViewModel.ResourceId);
            }
        }

        #region Overrides of ExplorerViewModel

        public override bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                OnPropertyChanged(() => IsLoading);
            }
        }

        #endregion

        #region Overrides of ExplorerViewModel

        protected override void AfterLoad(Guid environmentId)
        {
            var environmentViewModel = _environments.FirstOrDefault(a => a.Server.EnvironmentID == environmentId);
            SelectedEnvironment = environmentViewModel;
            if (ServerStateChanged != null)
            {
                if(SelectedEnvironment != null)
                {
                    ServerStateChanged(this, SelectedEnvironment.Server);
                }
                ConnectControlViewModel.IsLoading = false;
            }
            StatsArea?.ReCalculate();
        }

        #endregion

        #endregion

        #region Implementation of IDeployDestinationExplorerViewModel

        public event ServerSate ServerStateChanged;
        public virtual Version MinSupportedVersion => Version.Parse(SelectedEnvironment.Server.GetMinSupportedVersion());

        public virtual Version ServerVersion => _serverVersion ?? (_serverVersion = Version.Parse(SelectedEnvironment.Server.GetServerVersion()));

        public bool DeployTests
        {
            get
            {
                return _deployTests;
            }
            set
            {
                _deployTests = value;
                OnPropertyChanged(()=> DeployTests);
            }
        }

        #endregion
    }
}