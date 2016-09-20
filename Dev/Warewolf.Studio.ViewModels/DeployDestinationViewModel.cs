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
        public IDeployStatsViewerViewModel StatsArea { private get; set; }

        #region Implementation of IDeployDestinationExplorerViewModel

        public DeployDestinationViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
            : base(shellViewModel, aggregator)
        {
            ConnectControlViewModel.SelectedEnvironmentChanged += DeploySourceExplorerViewModelSelectedEnvironmentChanged;
            ConnectControlViewModel.ServerConnected+=ServerConnected;
            SelectedEnvironment = _environments.FirstOrDefault();
        }

        private void ServerConnected(object sender, IServer server)
        {
            var environmentViewModel = _environments.FirstOrDefault(a => a.Server.EnvironmentID == server.EnvironmentID);
            SelectedEnvironment = environmentViewModel;           
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

        public virtual Version ServerVersion => Version.Parse(SelectedEnvironment.Server.GetServerVersion());
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