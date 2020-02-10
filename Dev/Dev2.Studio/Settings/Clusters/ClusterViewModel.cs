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
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;

namespace Dev2.Settings.Clusters
{
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

        public ClusterViewModel()
        {
            CopyKeyCommand = new DelegateCommand(o => CopySecurityKey());
            NewServerCommand = new DelegateCommand(o => NewServer());
            LoadServerFollowers();
        }

        private static void NewServer()
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

        private static void CopySecurityKey()
        {
            Clipboard.SetText(Guid.NewGuid().ToString());
        }

        public ICommand CopyKeyCommand { get; }
        public ICommand NewServerCommand { get; }

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