
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
using System.Net.Sockets;

namespace HttpFramework
{
    /// <summary>
    /// Invoked when a client have been accepted by the <see cref="HttpListener"/>
    /// </summary>
    /// <remarks>
    /// Can be used to revoke incoming connections
    /// </remarks>
    public class ClientAcceptedEventArgs : EventArgs
    {
        private readonly Socket _socket;
        private bool _revoke;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientAcceptedEventArgs"/> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        public ClientAcceptedEventArgs(Socket socket)
        {
            _socket = socket;
        }

        /// <summary>
        /// Accepted socket.
        /// </summary>
        public Socket Socket
        {
            get { return _socket; }
        }

        /// <summary>
        /// Client should be revoked.
        /// </summary>
        public bool Revoked
        {
            get { return _revoke; }
        }

        /// <summary>
        /// Client may not be handled.
        /// </summary>
        public void Revoke()
        {
            _revoke = true;
        }
    }
}
