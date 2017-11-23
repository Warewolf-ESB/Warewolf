using System;
using System.Linq;
using System.Threading.Tasks;
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

        public DeployDestinationViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
            : base(shellViewModel, aggregator,false)
        {
            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer, aggregator, shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers);
            ConnectControlViewModel.ServerConnected += async (sender, server) => { await ServerConnected(sender, server).ConfigureAwait(true); };
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            SelectedEnvironment = _environments.FirstOrDefault();
            RefreshCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => RefreshEnvironment(SelectedEnvironment.ResourceId));
            ValidateEnvironments(shellViewModel);
        }

        private void ValidateEnvironments(IShellViewModel shellViewModel)
        {
            foreach (var env in shellViewModel?.ExplorerViewModel?.Environments)
            {
                var exists = Environments.FirstOrDefault(model => model.ResourceId == env.ResourceId);
                if (env.IsConnected && exists == null)
                {
                    Environments.Add(env);
                }
            }
        }

        private void ServerDisconnected(object sender, IServer server)
        {
            if (SelectedEnvironment != null)
            {
                ServerStateChanged?.Invoke(this, SelectedEnvironment.Server);
            }
        }

        private async Task<IEnvironmentViewModel> ServerConnected(object sender, IServer server)
        {
            var environmentViewModel = await CreateEnvironmentViewModel(sender, server.EnvironmentID, true).ConfigureAwait(true);
            environmentViewModel?.Server?.GetServerVersion();
            environmentViewModel?.Server?.GetMinSupportedVersion();
            SelectedEnvironment = environmentViewModel;
            StatsArea?.ReCalculate();
            if (environmentViewModel != null)
            {
                AfterLoad(environmentViewModel.ResourceId);
            }
            return environmentViewModel;
        }

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
    }
}