#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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