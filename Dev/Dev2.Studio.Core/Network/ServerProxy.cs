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
using System.Collections.Generic;
using System.Net;
using System.Network;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.SignalR.Wrappers;
using Dev2.Studio.Interfaces;



namespace Dev2.Network
{
    public interface IServerProxyFactory
    {
        IEnvironmentConnection New(Uri serverUri);
    }

    public class ServerProxyFactory : IServerProxyFactory
    {
        public IEnvironmentConnection New(Uri serverUri)
        {
            return new ServerProxy(serverUri);
        }
    }

    public class ServerProxy :  IEnvironmentConnection
    {
        readonly IEnvironmentConnection _wrappedConnection;
        public ServerProxy(Uri serverUri)
        {
           _wrappedConnection = new ServerProxyPersistentConnection(serverUri);
            SetupPassthroughEvents();
        }

        void SetupPassthroughEvents()
        {
            
            _wrappedConnection.PermissionsChanged += (sender, args) => RaisePermissionsChanged();
            _wrappedConnection.PermissionsModified += (sender, list) => RaisePermissionsModified(list);
            _wrappedConnection.NetworkStateChanged += (sender, args) => OnNetworkStateChanged(args);           
        }

        
        public ServerProxy(string serverUri, ICredentials credentials, IAsyncWorker worker)
        {
            _wrappedConnection = new ServerProxyPersistentConnection(serverUri,credentials,worker);
            SetupPassthroughEvents();
        }
        
        public ServerProxy(string webAddress, string userName, string password)
        {
            _wrappedConnection = new ServerProxyPersistentConnection(webAddress, userName, password);
            SetupPassthroughEvents();
        }

        #region Implementation of IEnvironmentConnection

        public IEventPublisher ServerEvents => _wrappedConnection.ServerEvents;

        public Guid ServerID
        {
            get
            {
                return _wrappedConnection.ServerID;
            }
            set
            {
                _wrappedConnection.ServerID = value;
            }
        }
        public Guid WorkspaceID => _wrappedConnection.WorkspaceID;

        public Uri AppServerUri => _wrappedConnection.AppServerUri;

        public Uri WebServerUri => _wrappedConnection.WebServerUri;

        public AuthenticationType AuthenticationType => _wrappedConnection.AuthenticationType;

        public string UserName => _wrappedConnection.UserName;

        public string Password => _wrappedConnection.Password;


        public bool IsAuthorized
        {
            get
            {
                return _wrappedConnection.IsAuthorized;
            }

            set
            {
                _wrappedConnection.IsAuthorized = value;
            }
        }

        public StringBuilder ExecuteCommand(StringBuilder xmlRequest, Guid workspaceId) => _wrappedConnection.ExecuteCommand(xmlRequest, workspaceId);

        public async Task<StringBuilder> ExecuteCommandAsync(StringBuilder xmlRequest, Guid workspaceId) => await _wrappedConnection.ExecuteCommandAsync(xmlRequest, workspaceId).ConfigureAwait(true);

        public IHubProxyWrapper EsbProxy => _wrappedConnection.EsbProxy;

        public bool IsConnected => _wrappedConnection.IsConnected;

        public bool IsConnecting => _wrappedConnection.IsConnecting;

        public string Alias
        {
            get
            {
                return _wrappedConnection.Alias;
            }
            set
            {
                _wrappedConnection.Alias = value;
            }
        }
        public string DisplayName
        {
            get
            {
                return _wrappedConnection.DisplayName;
            }
            set
            {
                _wrappedConnection.DisplayName = value;
            }
        }

        public void Connect(Guid id)
        {
            try
            {
                _wrappedConnection.Connect(_wrappedConnection.ID);
            }
           
            catch (Exception err)
            {
                Dev2Logger.Error(err, "Warewolf Error");
            }
        }
        
        public async Task<bool> ConnectAsync(Guid id)
        {
            try
            {
                return await _wrappedConnection.ConnectAsync(_wrappedConnection.ID).ConfigureAwait(true);
            }
             catch( FallbackException)
            {
                Dev2Logger.Info("Falling Back to previous signal r client", "Warewolf Info");
                var name = _wrappedConnection.DisplayName;
                
                SetupPassthroughEvents();
                _wrappedConnection.Connect(_wrappedConnection.ID);
                _wrappedConnection.DisplayName = name;
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, "Warewolf Error");
                throw;
            }
            return false;
        }
        public Guid ID => _wrappedConnection.ID;

        public void Disconnect()
        {
            _wrappedConnection.Disconnect();
        }

        public void Verify(Action<ConnectResult> callback) => Verify(callback, true);

        public void Verify(Action<ConnectResult> callback, bool wait)
        {
            _wrappedConnection.Verify(callback,wait);
        }

        public void StartAutoConnect()
        {
            _wrappedConnection.StartAutoConnect();
        }

        public bool IsLocalHost => _wrappedConnection.IsLocalHost;

        public Action<IExplorerItem> ItemAddedMessageAction
        {
            get
            {
                return _wrappedConnection.ItemAddedMessageAction;
            }
            set
            {
                _wrappedConnection.ItemAddedMessageAction = value;
            }
        }
        public IAsyncWorker AsyncWorker => _wrappedConnection.AsyncWorker;

        public IPrincipal Principal => _wrappedConnection.Principal;

        public Action<Guid, CompileMessageList> ReceivedResourceAffectedMessage
        {
            get
            {
                return _wrappedConnection.ReceivedResourceAffectedMessage;
            }
            set
            {
                _wrappedConnection.ReceivedResourceAffectedMessage = value;
            }
        }
        public IHubConnectionWrapper HubConnection => _wrappedConnection.HubConnection;

        public void FetchResourcesAffectedMemo(Guid resourceId)
        {
            _wrappedConnection.FetchResourcesAffectedMemo(resourceId);
        }

        public event EventHandler<NetworkStateEventArgs> NetworkStateChanged;
        public event EventHandler PermissionsChanged;

        void RaisePermissionsChanged()
        {
            PermissionsChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<List<WindowsGroupPermission>> PermissionsModified;

        void RaisePermissionsModified(List<WindowsGroupPermission> args)
        {
            if (PermissionsModified != null)
            {
                Dev2Logger.Debug("Permissions Modified: "+args, "Warewolf Debug");
                PermissionsModified(this, args);
            }
        }

        protected void OnNetworkStateChanged(NetworkStateEventArgs e)
        {
            var handler = NetworkStateChanged;
            handler?.Invoke(this, e);
        }
        #endregion

        public bool Equals(IEnvironmentConnection other)
        {
            if (other == null)
            {
                return false;
            }
            var isEqual = other.ID == ID && other.AuthenticationType == AuthenticationType &&
                          other.AppServerUri.Equals(AppServerUri) && other.WebServerUri.Equals(WebServerUri);
            return isEqual;
        }

        public override bool Equals(object obj) => Equals(obj as IEnvironmentConnection);

        public override int GetHashCode() => ID.GetHashCode();
    }
}
