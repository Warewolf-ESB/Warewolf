using System;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public class DeployDestinationViewModel : ExplorerViewModel, IDeployDestinationExplorerViewModel
    {
        bool _isLoading;
        private bool _deployTests;
        private Version _serverVersion;
        public IDeployStatsViewerViewModel StatsArea { private get; set; }

        #region Implementation of IDeployDestinationExplorerViewModel

        public DeployDestinationViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
            : base(shellViewModel, aggregator,false)
        {
            ConnectControlViewModel.SelectedEnvironmentChanged += DeploySourceExplorerViewModelSelectedEnvironmentChanged;
            ConnectControlViewModel.ServerConnected+=ServerConnected;
            ConnectControlViewModel.ServerDisconnected+=ServerDisconnected;
            SelectedEnvironment = _environments.FirstOrDefault();
            RefreshCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => RefreshEnvironment(SelectedEnvironment.ResourceId));
        }

        private void ServerDisconnected(object sender, IServer server)
        {
            if (SelectedEnvironment != null) ServerStateChanged?.Invoke(this, SelectedEnvironment.Server);
        }

        private void ServerConnected(object sender, IServer server)
        {
            var environmentViewModel = _environments.FirstOrDefault(a => a.Server.EnvironmentID == server.EnvironmentID);
            environmentViewModel?.Server?.GetServerVersion();
            environmentViewModel?.Server?.GetMinSupportedVersion();
            SelectedEnvironment = environmentViewModel;
            StatsArea?.ReCalculate();
        }

        void DeploySourceExplorerViewModelSelectedEnvironmentChanged(object sender, Guid environmentid)
        {
            var environmentViewModel = _environments.FirstOrDefault(a => a.Server.EnvironmentID == environmentid);
            SelectedEnvironment = environmentViewModel;
            StatsArea?.ReCalculate();
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