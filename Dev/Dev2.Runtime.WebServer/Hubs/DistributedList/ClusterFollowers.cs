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
using System.Threading.Tasks;
using Dev2.Runtime.WebServer.Security;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Warewolf.DistributedStore;

namespace Warewolf.DistributedList
{
    public interface IClient
    {
        Task NewMessage(string message);
        Task Notify<T>(T item) where T : ListNotification;
    }

    /**
     * SignalR hub that allows notifications of server changes to be be carried to clients (followers)
     */
    //[AuthorizeHub] // commented out to allow signalr client to be notified without needing authentication
    [HubName(Warewolf.Service.DistributedLists.ClusterFollowers)]
    public class ClusterFollowers : Hub<IClient>, IListWatcher
    {
        private static readonly WatcherList _watchers;
        static ClusterFollowers()
        {
            Console.WriteLine(Warewolf.Service.DistributedLists.ClusterFollowers);
            _watchers = ListRegistry.ClusterFollowers.Watchers;
        }
        public Task<bool> Register(string listName)
        {
            _watchers.AddWatcher(this);

            return Task.FromResult(true);
        }

        public async void Notify<T>(T item) where T : ListNotification
//        public async void ItemAdded(string item)
        {
            await Clients.Caller.Notify(item);
        }
    }
}