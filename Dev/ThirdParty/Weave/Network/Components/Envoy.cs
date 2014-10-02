
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
using System.Net;

namespace System.Network
{
    public sealed class Envoy : IDisposable
    {
        #region Instance Fields
        private IAsyncResult _currentOperation;
        private string _hostNameOrAddress;
        private int _port;
        #endregion

        #region Public Properties
        public string HostNameOrAddress { get { return _hostNameOrAddress; } }
        public int Port { get { return _port; } }
        #endregion

        #region Events
        public event EnvoyReturnedEventHandler EnvoyReturned;
        #endregion

        #region Constructor
        public Envoy(string hostNameOrAddress)
        {
            _hostNameOrAddress = hostNameOrAddress;
        }

        public Envoy(string hostNameOrAddress, int port)
        {
            _hostNameOrAddress = hostNameOrAddress;
            _port = port;
        }
        #endregion

        #region DNS Resolution Handling
        public Exception BeginResolution()
        {
            Exception toReturn = null;

            try { _currentOperation = Dns.BeginGetHostEntry(_hostNameOrAddress, new AsyncCallback(EndResolution), null); }
            catch (Exception e)
            {
                toReturn = e;
            }

            return toReturn;
        }

        private void EndResolution(IAsyncResult result)
        {
            bool successful = true;
            IPHostEntry resolvedHostEntry = null;

            try
            {
                resolvedHostEntry = Dns.EndGetHostEntry(result);
            }
            catch
            {
                successful = false;
            }

            if (successful) successful = (resolvedHostEntry != null);
            _currentOperation = null;

            if (EnvoyReturned != null) EnvoyReturned(this, successful, _hostNameOrAddress, resolvedHostEntry);
        }
        #endregion

        #region Disposal Handling
        public void Dispose()
        {
            if (_currentOperation != null)
            {
                try { Dns.EndGetHostEntry(_currentOperation); }
                catch { }

                if (EnvoyReturned != null) EnvoyReturned(this, false, _hostNameOrAddress, null);
                _currentOperation = null;
            }

            EnvoyReturned = null;
        }
        #endregion
    }
}
