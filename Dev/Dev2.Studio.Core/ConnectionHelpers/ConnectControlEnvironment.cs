using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.ConnectionHelpers
{
    public class ConnectControlEnvironment : INotifyPropertyChanged, IConnectControlEnvironment
    {
        #region |Fields|
        IEnvironmentModel _environmentModel;
        bool _isConnected;
        string _connectedText;
        string _displayName;
        bool _allowEdit;
        #endregion

        #region |Properties|
        public IEnvironmentModel EnvironmentModel
        {
            get
            {
                return _environmentModel;
            }
            set
            {
                _environmentModel = value;
                DisplayName = _environmentModel.Name;
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
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
