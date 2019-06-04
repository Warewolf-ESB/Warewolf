#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Studio.Interfaces;

namespace Dev2.ConnectionHelpers
{
    public class ConnectControlEnvironment : INotifyPropertyChanged, IConnectControlEnvironment
    {
        #region |Fields|
        IServer _server;
        bool _isConnected;
        string _connectedText;
        string _displayName;
        bool _allowEdit;
        #endregion

        #region |Properties|
        public IServer Server
        {
            get
            {
                return _server;
            }
            set
            {
                _server = value;
                DisplayName = _server.Name;
                OnPropertyChanged();
            }
        }

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                _isConnected = value;
                ConnectedText = _isConnected ? "Disconnect" : "Connect";
                OnPropertyChanged();
            }
        }

        public string ConnectedText
        {
            get
            {
                return _connectedText;
            }
            set
            {
                _connectedText = value;
                OnPropertyChanged();
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                OnPropertyChanged();
            }
        }

        public bool AllowEdit
        {
            get
            {
                return _allowEdit;
            }
            set
            {
                _allowEdit = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
