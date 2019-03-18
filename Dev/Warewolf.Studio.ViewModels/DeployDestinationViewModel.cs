#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Linq;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Deploy;
using Microsoft.Practices.Prism.PubSubEvents;
using System.Threading.Tasks;

namespace Warewolf.Studio.ViewModels
{
    public class DeployDestinationViewModel : ExplorerViewModel, IDeployDestinationExplorerViewModel
    {
        bool _isLoading;
        bool _deployTests;
        Version _serverVersion;
        public IDeployStatsViewerViewModel StatsArea { private get; set; }

        public DeployDestinationViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
            : base(shellViewModel, aggregator,false)
        {
            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer, aggregator, shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers);
            ConnectControlViewModel.ServerConnected += async (sender, server) => { await ServerConnectedAsync(sender, server).ConfigureAwait(true); };
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            SelectedEnvironment = _environments.FirstOrDefault();
            RefreshCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => RefreshEnvironment(SelectedEnvironment.ResourceId));
            ValidateEnvironments(shellViewModel);
        }

        void ValidateEnvironments(IShellViewModel shellViewModel)
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

        void ServerDisconnected(object sender, IServer server)
        {
            if (SelectedEnvironment != null)
            {
                ServerStateChanged?.Invoke(this, SelectedEnvironment.Server);
            }
        }

        async Task<IEnvironmentViewModel> ServerConnectedAsync(object sender, IServer server)
        {
            var environmentViewModel = await CreateEnvironmentViewModelAsync(sender, server.EnvironmentID, true).ConfigureAwait(true);
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
            get => _isLoading;
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
            get => _deployTests;
            set
            {
                _deployTests = value;
                OnPropertyChanged(()=> DeployTests);
            }
        }
    }
}