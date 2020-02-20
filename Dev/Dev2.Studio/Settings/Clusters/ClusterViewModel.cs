/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Warewolf;
using Warewolf.Data;
using Warewolf.Options;

namespace Dev2.Settings.Clusters
{
    public class ServerOptions
    {
        [DataProvider(typeof(ResourceDataProvider))]
        public NamedGuid Leader { get; set; }

        public class ResourceDataProvider : IOptionDataList<INamedGuid>
        {
            public INamedGuid[] Items
            {
                get
                {
                    var shellViewModel = CustomContainer.Get<IShellViewModel>();
                    var sources = shellViewModel.ActiveServer.ResourceRepository.FindSourcesByType<IServerSource>(
                        shellViewModel.ActiveServer, enSourceType.Dev2Server);
                    
                    return sources?.Select(o => new NamedGuid {Name = o.Name, Value = o.ID})
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
        private ObservableCollection<Server> _servers;
        private string _clusterKey;

        public ClusterViewModel()
        {
            CopyKeyCommand = new DelegateCommand(o => CopyClusterKey());
            EditServerCommand = new DelegateCommand(o => EditServer());
            TestKeyCommand = new DelegateCommand(o => TestClusterKey());
            LoadServerFollowers();
            ServerOptions = new ServerOptions();
        }

        private static void EditServer()
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.NewServerSource(string.Empty);
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

        private static void CopyClusterKey()
        {
            Clipboard.SetText(Guid.NewGuid().ToString());
        }

        private static void TestClusterKey()
        {
            var popupController = CustomContainer.Get<IPopupController>();
            popupController?.Show("Success!", "Test Cluster Key", MessageBoxButton.OK, MessageBoxImage.Information,
                string.Empty, false, false, true, false, false, false);
        }

        public Type ResourceType => typeof(IServerSource);
        public ICommand CopyKeyCommand { get; }
        public ICommand EditServerCommand { get; }
        public ICommand TestKeyCommand { get; }

        public string ClusterKey
        {
            get => _clusterKey;
            set
            {
                _clusterKey = value;
                OnPropertyChanged(nameof(ClusterKey));
                OnPropertyChanged(nameof(TestKeyCommand));
            }
        }

        public ObservableCollection<Server> Servers
        {
            get => _servers;
            private set
            {
                _servers = value;
                OnPropertyChanged(nameof(Servers));
            }
        }
        
        public IEnumerable<ServerFollower> Followers
        {
            get => _followers;
            set
            {
                _followers = value;
                OnPropertyChanged(nameof(Followers));
            }
        }

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

        public ServerOptions ServerOptions
        {
            get;
            set;
        }

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
    }
}