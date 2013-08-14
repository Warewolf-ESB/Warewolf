using System;
using System.Network;
using System.Windows;
using Caliburn.Micro;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.Network
{
    public class TcpClientAdapter
    {
        public TcpClientAdapter()
        {
            //_clientAdapter = new TCPDispatchedClient("Studio Client");
            //_clientAdapter.LoginStateChanged += _client_LoginStateChanged;

        }

        public bool IsConnected { get; private set; } //{ get { return _clientAdapter.NetworkState == NetworkState.Online && _clientAdapter.LoggedIn; } }
        public bool IsAuxiliary { get; private set; } //{ get { return _clientAdapter.IsAuxiliary; } }


        public bool Connect(Uri address)
        {
            if(address == null)
            {
                throw new ArgumentNullException("address");
            }

            var dns = address.DnsSafeHost;
            var port = address.Port;

            //_clientAdapter = new TCPDispatchedClient("Studio Client");
            //_clientAdapter.LoginStateChanged += _client_LoginStateChanged;

            ////TODO Brendon.Page, 2012-10-24, Check for null client, this happens when the studio is closed and the environment isn't connected
            //NetworkStateEventArgs args = _clientAdapter.Connect(dns, port);

            //if(args.ToState == NetworkState.Online)
            //{
            //    LoginStateEventArgs lArgs = _clientAdapter.Login(_username, _password);
            //    if(!_clientAdapter.WaitForClientDetails()) //Bug 8796, After logging in wait for client details to come through before proceeding
            //    {
            //        throw new Exception("Retrieving client details from the server timed out.");
            //    }

            //    if(lArgs.LoggedIn)
            //    {
            //        DataChannel = new EnvironmentConnection.FrameworkDataChannelWrapper(this, _clientAdapter, dns, port);
            //        ExecutionChannel = new ExecutionClientChannel(_clientAdapter);
            //        DataListChannel = new DataListClientChannel(_clientAdapter);
            //    }
            //    else
            //    {
            //        if(!string.IsNullOrEmpty(Alias))
            //        {
            //            DisplayName = string.Format("{0} - (Unavailable) ", Alias);
            //        }

            //        _clientAdapter.LoginStateChanged -= _client_LoginStateChanged;
            //        _clientAdapter.Dispose();
            //        _clientAdapter = null;
            //    }
            //}
            //else
            //{
            //    DataChannel = new EnvironmentConnection.FrameworkDataChannelWrapper(this, _clientAdapter, dns, port);
            //    ExecutionChannel = new ExecutionClientChannel(_clientAdapter);
            //    DataListChannel = new DataListClientChannel(_clientAdapter);

            //    if(!string.IsNullOrEmpty(Alias))
            //    {
            //        DisplayName = string.Format("{0} - (Unavailable) ", Alias);
            //    }
            //}
            return false;
        }

        private void EnvironmentConnection_LoginStateChanged(object sender, LoginStateEventArgs e)
        {
            ////
            //// If application in shutdown do nothing
            ////
            //if(Application.Current == null)
            //{
            //    return;
            //}

            ////
            //// If auxilliry connection then do nothing
            ////
            //EnvironmentModel.WrappedEnvironmentConnection connection = EnvironmentConnection as EnvironmentModel.WrappedEnvironmentConnection;
            //if(connection != null && connection.IsAuxiliry)
            //{
            //    return;
            //}

            //if(e.LoggedIn)
            //{
            //    Application.Current.Dispatcher.BeginInvoke(new System.Action(() => _eventPublisher.Publish(new EnvironmentConnectedMessage(this))), null);
            //}
            //else
            //{
            //    Application.Current.Dispatcher.BeginInvoke(new System.Action(() => _eventPublisher.Publish(new EnvironmentDisconnectedMessage(this))), null);
            //}
        }
    }
}
