/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net.WebSockets;

namespace Warewolf.Interfaces.Auditing
{
    // TODO: move to Warewolf.Net
    public interface IWebSocketWrapper : IDisposable
    {
        Uri Uri { get; }
        IWebSocketWrapper Connect();
        WebSocketState State { get; }
        IWebSocketWrapper OnConnect(Action<IWebSocketWrapper> onConnect);
        IWebSocketWrapper OnDisconnect(Action<IWebSocketWrapper> onDisconnect);
        IWebSocketWrapper OnMessage(Action<string, IWebSocketWrapper> onMessage);
        bool IsOpen();
        IWebSocketWrapper Close();
        void SendMessage(string message);
        void SendMessage(byte[] message);
    }
}
