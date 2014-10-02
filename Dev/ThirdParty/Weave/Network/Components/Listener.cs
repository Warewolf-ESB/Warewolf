
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace System.Network
{
    public struct ListenerConfig
    {
        #region Instance Fields
        private string _address;
        private int _port;
        private int _backlog;
        #endregion

        #region Public Properties
        public string Address { get { return _address; } }
        public int Port { get { return _port; } }
        public int Backlog { get { return _backlog; } }
        #endregion

        #region Constructor
        public ListenerConfig(string address, int port, int backlog)
        {
            _address = address;
            _port = port;
            _backlog = backlog;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return String.Format("{0}, {1}", _address, _port);
        }
        #endregion
    }

    public sealed class Listener : IDisposable
    {
        #region Static Members
        private static Socket Bind(NetworkServer host, ref string addressString, int port, int backlog, out bool valid, out IPAddress address)
        {
            valid = IPAddress.TryParse(addressString, out address);

            if (!valid)
            {
                address = NetworkHelper.GetIPAddress(NetworkHelper.GetNICFromID(addressString), AddressFamily.InterNetwork);

                if (address == IPAddress.None) return null;
                else
                {
                    addressString = address.ToString();
                    valid = true;
                }
            }

            IPEndPoint ipEP = new IPEndPoint(address, port);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                s.LingerState.Enabled = false;
                s.ExclusiveAddressUse = false;
                s.Bind(ipEP);
                s.Listen(backlog);
                valid = true;
            }
            catch
            {
                NetworkHelper.ReleaseSocket(ref s);
                valid = false;
            }

            return s;
        }
        #endregion

        #region Instance Fields
        private bool _disposed;
        private bool _valid;
        private bool _listening;
        private NetworkServer _server;
        private Socket _listener;
        private IPAddress _address;
        private string _addressString;
        private int _port;
        private int _backlog;
        #endregion

        #region Public Properties
        public bool Disposed { get { return _disposed; } }
        public bool Valid { get { return _valid; } }
        public bool Listening { get { return _listening; } }
        public NetworkServer Server { get { return _server; } }
        public IPAddress Address { get { return _address; } }
        public int Port { get { return _port; } }
        public int Backlog { get { return _backlog; } }
        #endregion

        #region Constructor
        public Listener(NetworkServer server, ListenerConfig configuration)
        {
            _server = server;
            _server.Disposing += Dispose;
            _addressString = configuration.Address;
            _port = configuration.Port;
            _backlog = configuration.Backlog;
            _disposed = false;
            _listening = false;
            _valid = true;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            if (!_valid) return "Invalid";
            else return String.Format("{0}:{1}", _addressString, _port);
        }
        #endregion

        #region [Start/Stop] Handling
        public bool Start()
        {
            if (_disposed || _listening || !_valid) return false;

            if (_listener == null)
            {
                _listener = Bind(_server, ref _addressString, _port, _backlog, out _valid, out _address);
                if (!_valid) return false;
            }

            _listening = true;
            
            try 
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAccept);
                if (!_listener.AcceptAsync(args)) OnAccept(this, args);
            }
            catch
            {
                Dispose();
                return false;
            }

            _valid = true;
            return true;
        }

        public void Stop()
        {
            if (_disposed || !_listening) return;
            _listening = false;
        }
        #endregion

        #region Connection Handling
        private void OnAccept(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                Socket connection = args.AcceptSocket;

                if (_disposed || !_listening) NetworkHelper.ReleaseSocket(ref connection);
                else if (connection != null) _server.NotifySocketConnected(this, connection);
            }
            catch (ObjectDisposedException) { Dispose(); }
            catch (SocketException) { Dispose(); }
            catch (Exception) { Dispose(); }
            finally
            {
                if (!_disposed && _listener != null && _listening)
                {
                    args.AcceptSocket = null;
                    if (!_listener.AcceptAsync(args)) OnAccept(this, args);
                }
                else
                {
                    try { args.AcceptSocket = null; args.Dispose(); }
                    catch { }
                }
            }
        }
        #endregion

        #region Disposal Handling
        ~Listener()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
            _valid = _listening = false;

            if (_listener != null) NetworkHelper.ReleaseSocket(ref _listener);

            if (disposing)
            {
                _server.Disposing -= Dispose;
                _server = null;
                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}
