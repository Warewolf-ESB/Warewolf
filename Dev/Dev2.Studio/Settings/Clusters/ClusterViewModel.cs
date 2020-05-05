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
using ServiceStack.Common.Extensions;
using Warewolf;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Options;

namespace Dev2.Settings.Clusters
{
    public class LeaderServerOptions : BindableBase
    {
        private NamedGuid _leader = new NamedGuid();

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

        public LeaderServerOptions Clone()
        {
            var result = (LeaderServerOptions)MemberwiseClone(); ;
            if (result.Leader is null)
            {
                result.Leader = new NamedGuid();
            }
            return result;
        }
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
        private bool _isTestKeyEnabled;

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
                    IsValidKey = false;
                    UpdateIsDirty();
                    IsTestKeyEnabled = CanTestKey();
                };
                CopyKeyCommand = new DelegateCommand(o => CopyClusterKey());
                TestKeyCommand = new DelegateCommand(o => TestClusterKey());
                LoadServerFollowers();
                LeaderServerOptions = new LeaderServerOptions();
                if (ClusterSettings.LeaderServerResource != null)
                {
                    LeaderServerOptions.Leader = new NamedGuid
                    {
                        Name = ClusterSettings.LeaderServerResource.Name,
                        Value = ClusterSettings.LeaderServerResource.Value,
                    };
                }

                LeaderServerOptions.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName != nameof(LeaderServerOptions.Leader))
                    {
                        return;
                    }

                    ClusterSettings.LeaderServerResource = LeaderServerOptions?.Leader;
                    IsValidKey = false;
                    UpdateIsDirty();
                    IsTestKeyEnabled = CanTestKey();
                };
            }
            catch (Exception e)
            {
                popupController.ShowErrorMessage(e.Message);
            }
        }

        private void UpdateIsDirty()
        {
            if (Item is null)
            {
                return;
            }
            var isDirty = !ClusterSettings.LeaderServerKey.Equals(Item.ClusterSettings.LeaderServerKey);
            isDirty |= !LeaderServerOptions.Leader.Value.Equals(Item.LeaderServerOptions.Leader.Value);
            IsDirty = isDirty;
        }

        public bool IsValidKey { get; set; }

        private bool CanTestKey() => LeaderServerOptions.Leader != null && !string.IsNullOrWhiteSpace(ClusterSettings.LeaderServerKey);

        public bool IsTestKeyEnabled
        {
            get => _isTestKeyEnabled;
            set
            {
                _isTestKeyEnabled = value; 
                OnPropertyChanged(nameof(IsTestKeyEnabled));
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
            var list = _server.Connection.ServerFollowerList;
            list.CollectionChanged += (sender, args) =>
            {
                Followers = list.ToList<ServerFollower>();
            };
        }

        private void CopyClusterKey()
        {
            Clipboard.SetText(ClusterSettings.Key ?? "invalid key");
        }

        private void TestClusterKey()
        {
            try
            {
                IsValidKey = false;
                var result = _resourceRepository.TestClusterSettings(_server, ClusterSettings);
                if (result.HasError)
                {
                    _popupController.ShowErrorMessage(result.Message.ToString());
                }
                else
                {
                    IsValidKey = true;
                    _popupController.Show("Success!", "Test Cluster Settings", MessageBoxButton.OK, MessageBoxImage.Information,
                        string.Empty, false, false, true, false, false, false);
                }
            }
            catch (Exception ex)
            {
                _popupController.ShowErrorMessage(ex.Message);
            }

            UpdateIsDirty();
        }

        public Type ResourceType => typeof(IServerSource);
        public ICommand CopyKeyCommand { get; }
        public ICommand TestKeyCommand { get; }
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

        private ClusterViewModel Clone(ClusterViewModel clusterViewModel)
        {
            if (MemberwiseClone() is ClusterViewModel clone)
            {
                clone.LeaderServerOptions = clusterViewModel.LeaderServerOptions.Clone();
                clone.ClusterSettings = this.ClusterSettings.Clone();

                return clone;
            }
            return this;
        }
    }
}