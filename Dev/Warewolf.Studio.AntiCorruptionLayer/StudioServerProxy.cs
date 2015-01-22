using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Controller;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Threading;
using Warewolf.Studio.ServerProxyLayer;

namespace Warewolf.Studio.AntiCorruptionLayer
{

    public class Server : Resource,IServer
    {
        readonly ServerProxy _environmentConnection;
        readonly Guid _serverId;
        StudioServerProxy _proxyLayer;
        IExplorerRepository _explorerRepository;

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
        }

        #region Implementation of IServer

        public async Task<bool> Connect()
        {
            return await _environmentConnection.ConnectAsync(_serverId);
        }

        public IList<IResource> Load()
        {
            _proxyLayer.QueryManagerProxy.Load();
            return null;
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
                return _explorerRepository;
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

        public event PermissionsChanged PermissionsChanged;

        #endregion
    }

    public class StudioServerProxy:IExplorerRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public StudioServerProxy(ICommunicationControllerFactory controllerFactory,IEnvironmentConnection environmentConnection)
        {
            if (controllerFactory == null)
            {
                throw new ArgumentNullException("controllerFactory");
            }
            if (environmentConnection == null)
            {
                throw new ArgumentNullException("environmentConnection");
            }
            QueryManagerProxy = new QueryManagerProxy(controllerFactory, environmentConnection);
            UpdateManagerProxy = new ExplorerUpdateManagerProxy(controllerFactory,environmentConnection);
        }

        public QueryManagerProxy QueryManagerProxy { get; set; }
        public ExplorerUpdateManagerProxy UpdateManagerProxy { get; set; }

        #region Implementation of IExplorerRepository

        public bool Rename(IExplorerItemViewModel vm, string newName)
        {
            try
            {
                UpdateManagerProxy.Rename(vm.ResourceId, newName);
                return false;
            }
            catch(Exception err)
            {
                //todo:log
                return false;
               
            }
            
        }

        public bool Move(IExplorerItemViewModel explorerItemViewModel, IExplorerItemViewModel destination)
        {
   
              
                UpdateManagerProxy.MoveItem(explorerItemViewModel.ResourceId,destination.ResourceId);
                return true;

        }

        #endregion
    }
}
