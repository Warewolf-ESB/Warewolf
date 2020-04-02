#pragma warning disable
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
using Microsoft.AspNet.SignalR;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer.Hubs
{
    public interface IWarewolfServerHub
    {
        void ItemAddedMessage(string item);
        void LeaderConfigChange();
        void SendPermissionsMemo(string serializedMemo);
        void SendDebugState(string serializedDebugState);
        void SendWorkspaceID(Guid workspaceId);
        void SendServerID(Guid serverId);
        void ChangeNotification(ChangeNotification changeNotification);
    }
    // Instances of the Hub class are transient, you can't use them 
    // to maintain state from one method call to the next. Each time 
    // the server receives a method call from a client, a new instance
    // of your Hub class processes the message.
    public abstract class ServerHub : Hub<IWarewolfServerHub>
    {
        protected ServerHub(Server server)
        {
            Server = server;
        }

        protected Server Server { get; private set; }
    }
}
