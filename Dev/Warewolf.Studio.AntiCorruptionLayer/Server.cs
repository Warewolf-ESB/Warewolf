using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Controller;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class Server : Resource,IServer
    {
        readonly ServerProxy _environmentConnection;
        readonly Guid _serverId;
        readonly StudioServerProxy _proxyLayer;
        IStudioUpdateManager _updateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public Server(string uri,string userName,string password):this(uri,new NetworkCredential(userName,password))
        {
        }

        public Server(Uri uri)
            : this(uri.ToString(), CredentialCache.DefaultNetworkCredentials)
        {
        }
        
        public Server(string uri,ICredentials credentials)
        {
            _environmentConnection = new ServerProxy(uri,credentials,new AsyncWorker());
            _serverId = Guid.NewGuid();
            _proxyLayer = new StudioServerProxy(new CommunicationControllerFactory(), _environmentConnection);
            UpdateRepository = new StudioResourceUpdateManager(new CommunicationControllerFactory(), _environmentConnection);
            _environmentConnection.PermissionsModified += RaisePermissionsModifiedEvent;
            _environmentConnection.NetworkStateChanged += RaiseNetworkStateChangeEvent;
        }

        void RaiseNetworkStateChangeEvent(object sender, System.Network.NetworkStateEventArgs e)
        {
            if(NetworkStateChanged!= null)
            {
                NetworkStateChanged(new NetworkStateChangedEventArgs(e));
            }
        }

        void RaisePermissionsModifiedEvent(object sender, List<IWindowsGroupPermission> e)
        {
            if (PermissionsChanged != null)
            {
                PermissionsChanged(new PermissionsChangedArgs(e));
            }
            Permissions = e;
        }

        #region Implementation of IServer

        public async Task<bool> Connect()
        {
            return await _environmentConnection.ConnectAsync(_serverId);
        }

        public List<IResource> Load()
        {
            return null;
        }

        public async Task<IExplorerItem> LoadExplorer()
        {
            var result = await _proxyLayer.QueryManagerProxy.Load();            
            return result;
        }

        public IList<IServer> GetServerConnections()
        {
            return null;
        }

        public IList<IToolDescriptor> LoadTools()
        {
            return null;
        }

        public IExplorerRepository ExplorerRepository
        {
            get
            {
                return _proxyLayer;
            }
        }

        public bool IsConnected()
        {
            return _environmentConnection.IsConnected;
        }

        public void ReloadTools()
        {
        }

        public void Disconnect()
        {
            _environmentConnection.Disconnect();
        }

        public void Edit()
        {
        }

        public List<IWindowsGroupPermission> Permissions { get; private set; }

        public event PermissionsChanged PermissionsChanged;
        public event NetworkStateChanged NetworkStateChanged;
        public IStudioUpdateManager UpdateRepository
        {
            get
            {
                return _updateRepository;
            }
            private set
            {
                _updateRepository = value;
            }
        }

        #endregion

        #region Overrides of Resource

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return "localhost";
        }

        #endregion
    }
}