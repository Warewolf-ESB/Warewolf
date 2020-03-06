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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Newtonsoft.Json;
using Warewolf;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Options;

namespace Dev2.Settings.Clusters
{
    public class LeaderServerOptions : BindableBase
    {
        private NamedGuid _leader;

        [DataProvider(typeof(ResourceDataProvider))]
        public NamedGuid Leader
        {
            get => _leader;
            set => SetProperty(ref _leader, value);
        }

        public class ResourceDataProvider : IOptionDataList<INamedGuid>
        {
            public INamedGuid[] Items
            {
                get
                {
                    var shellViewModel = CustomContainer.Get<IShellViewModel>();
                    var activeServer = shellViewModel.ActiveServer;
                    var sources = activeServer?.ResourceRepository.FindSourcesByType<Data.ServiceModel.Connection>(
                        activeServer, enSourceType.Dev2Server);
                    
                    return sources?.Select(o => new NamedGuid {Name = o.ResourceName, Value = o.ResourceID})
                        .ToArray();
                }
            }
        }
    }
    public class Server
    {
        public string DisplayName { get; set; }
    }
    public class ServerFollower
    {
        public string HostName { get; set; }
        public DateTime ConnectedSince { get; set; }
        public DateTime LastSync { get; set; }
    }
    public class ClusterViewModel : SettingsItemViewModel, IUpdatesHelp
    {
        private string _filter;
        private IEnumerable<ServerFollower> _followers;
        private ClusterSettingsData _clusterSettings;
        private readonly IPopupController _popupController;
        private readonly IResourceRepository _resourceRepository;
        private readonly IServer _server;
        private ClusterViewModel _item;

        public ClusterViewModel()
        {

        }
        public ClusterViewModel(IResourceRepository resourceRepository, IServer server, IPopupController popupController)
        {
            _popupController = popupController;
            _resourceRepository = resourceRepository;
            _server = server;
            try
            {
                ClusterSettings = resourceRepository.GetClusterSettings(server);
                var leaderServerKey = ClusterSettings.LeaderServerKey;
                if (leaderServerKey is null)
                {
                    ClusterSettings.LeaderServerKey = string.Empty;
                }
                ClusterSettings.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(ClusterSettings.Key))
                    {
                        return;
                    }

                    IsDirty = !Equals(Item);
                    OnPropertyChanged(nameof(TestKeyCommand));
                };
                CopyKeyCommand = new DelegateCommand(o => CopyClusterKey());
                TestKeyCommand = new DelegateCommand(o => TestClusterKey());
                LoadServerFollowers();
                LeaderServerOptions = new LeaderServerOptions();
                LeaderServerOptions.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName != nameof(LeaderServerOptions.Leader))
                    {
                        return;
                    }

                    ClusterSettings.LeaderServerResourceId = LeaderServerOptions?.Leader?.Value ?? Guid.Empty;
                    IsDirty = !Equals(Item);
                };
            }
            catch (Exception e)
            {
                popupController.ShowErrorMessage(e.Message);
            }
        }

        public ClusterSettingsData ClusterSettings
        {
            get => _clusterSettings;
            set
            {
                _clusterSettings = value; 
                OnPropertyChanged(nameof(ClusterSettings));
            }
        }

        private void LoadServerFollowers()
        {
            Followers = new List<ServerFollower>
            {
                AddFollower("Server One", -3, 0),
                AddFollower("Server Two", -5, 1),
                AddFollower("Server Three", -7, 0),
                AddFollower("Server Four", 0, 0),
                AddFollower("Server Five", -1, 2),
                AddFollower("Server Six", -2, 0),
            };
        }
        
        private static ServerFollower AddFollower(string hostName, int date, int sync)
        {
            var follower = new ServerFollower
            {
                HostName = hostName,
                ConnectedSince = DateTime.Now.AddMonths(date),
                LastSync = DateTime.Now.AddMonths(sync),
            };
            return follower;
        }

        private void CopyClusterKey()
        {
            Clipboard.SetText(ClusterSettings.Key ?? "invalid key");
        }

        private void TestClusterKey()
        {
            _popupController?.Show("Success!", "Test Cluster Key", MessageBoxButton.OK, MessageBoxImage.Information,
                string.Empty, false, false, true, false, false, false);
        }

        public Type ResourceType => typeof(IServerSource);
        [JsonIgnore]
        public ICommand CopyKeyCommand { get; }
        [JsonIgnore]
        public ICommand TestKeyCommand { get; }
        [JsonIgnore]
        public IEnumerable<ServerFollower> Followers
        {
            get => _followers;
            set
            {
                _followers = value;
                OnPropertyChanged(nameof(Followers));
            }
        }
        [JsonIgnore]
        public string Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                OnPropertyChanged(nameof(Filter));
                LoadServerFollowers();
                FilterFollowers(value.ToLower());
            }
        }

        public LeaderServerOptions LeaderServerOptions { get; set; }

        private void FilterFollowers(string value)
        {
            var list = new List<ServerFollower>();
            foreach (var serverFollower in Followers)
            {
                var match = serverFollower.HostName.ToLower().Contains(value);

                if (!match && serverFollower.ConnectedSince.ToString(CultureInfo.InvariantCulture).Contains(value))
                {
                    match = true;
                }

                if (!match && serverFollower.LastSync.ToString(CultureInfo.InvariantCulture).Contains(value))
                {
                    match = true;
                }

                if (match)
                {
                    list.Add(serverFollower);
                }
            }
            Followers = new List<ServerFollower>(list);
        }

        protected override void CloseHelp()
        {
            
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public void Save()
        {
            _resourceRepository.SaveClusterSettings(_server, ClusterSettings);
            SetItem(this);
        }
        
        [JsonIgnore]
        private ClusterViewModel Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }
        public void SetItem(ClusterViewModel clusterViewModel)
        {
            Item = Clone(clusterViewModel);
        }

        private static ClusterViewModel Clone(ClusterViewModel clusterViewModel)
        {
            var resolver = new ShouldSerializeContractResolver();
            var ser = JsonConvert.SerializeObject(clusterViewModel, new JsonSerializerSettings { ContractResolver = resolver });
            var clone = JsonConvert.DeserializeObject<ClusterViewModel>(ser);
            return clone;
        }
    }
}