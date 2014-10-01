
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Net.Sockets;

namespace HttpFramework
{
    /// <summary>
    /// Class that receives Requests from a <see cref="IHttpClientContext"/>.
    /// </summary>
    public interface IHttpContextHandler
    {
        /// <summary>
        /// Client have been disconnected.
        /// </summary>
        /// <param name="client">Client that was disconnected.</param>
        /// <param name="error">Reason</param>
        /// <see cref="IHttpClientContext"/>
        void ClientDisconnected(IHttpClientContext client, SocketError error);

        /// <summary>
        /// Invoked when a client context have received a new HTTP request
        /// </summary>
        /// <param name="client">Client that received the request.</param>
        /// <param name="request">Request that was received.</param>
        /// <see cref="IHttpClientContext"/>
        void RequestReceived(IHttpClientContext client, IHttpRequest request);
    }
}
