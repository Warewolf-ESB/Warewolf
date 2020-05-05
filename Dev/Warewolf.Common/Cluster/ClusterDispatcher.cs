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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Warewolf.Data;
using Warewolf.Debugging;
using Warewolf.DistributedStore;
using Warewolf.Esb;

namespace Warewolf.Cluster
{
    public class NullClusterDispatcher : IClusterDispatcher
    {
        public void AddListener(Guid workspaceID, INotificationListener<ChangeNotification> listener)
        {
        }

        public int Count => 0;
        public void Remove(Guid workspaceId)
        {
        }

        public void Shutdown()
        {
        }

        public void Write<T>(T info) where T : class
        {
        }

        public INotificationListener<ChangeNotification>[] Followers { get; } = new INotificationListener<ChangeNotification>[] { };
    }

    /**
     * Uses EsbHub to write Cluster state data from Warewolf Server to any client that is following cluster events.
     *
     * This also updates the distributed list of server followers.
     */
    public class ClusterDispatcherImplementation : IClusterDispatcher
    {
        static ClusterDispatcherImplementation()
        {
            var t = new Thread(() =>
            {
                var list = ListRegistry.ClusterFollowers;
                var rand = new Random();
                while (true)
                {
                    var op = rand.Next(1,4);
                    switch (op)
                    {
                        case 1: list.Add(new ServerFollower()); break;
                        case 2: list.Remove(new ServerFollower()); break;
                        case 3: list.Clear(); break;
                    }

                    Thread.Sleep(1000);
                }
            });
            t.IsBackground = true;
            t.Start();
        }
        private readonly ConcurrentDictionary<Guid, INotificationListener<ChangeNotification>> _writers = new ConcurrentDictionary<Guid, INotificationListener<ChangeNotification>>();
        private bool _shutdownRequested;

        public int Count => _writers.Count;

        public void AddListener(Guid workspaceId, INotificationListener<ChangeNotification> listener)
        {
            if (listener == null || _shutdownRequested)
            {
                return;
            }
            _writers.TryAdd(workspaceId, listener);
            ListRegistry.ClusterFollowers.Add(new ServerFollower
            {
                ConnectedSince = DateTime.Now,
                HostName = "no eye dear",
                LastSync = DateTime.Now,// todo: update timestamp whenever follower synchronises
            });
        }

        public void Remove(Guid workspaceId)
        {
            _writers.TryRemove(workspaceId, out _);
        }

        public void Shutdown()
        {
            _shutdownRequested = true;
        }

        public void Write<T>(T info) where T : class
        {
            foreach (var writer in _writers.Values.ToList())
            {
                writer.Write(new ChangeNotification());
            }
        }

        public INotificationListener<ChangeNotification>[] Followers => _writers.Values.ToArray();
    }

    public static class ClusterDispatcher
    {
        static IClusterDispatcher _instance;
        static readonly object _lock = new object();
        public static IClusterDispatcher Instance
        {
            get
            {
                if (_instance is null)
                {
                    lock (_lock)
                    {
                        if (_instance is null)
                        {
                            _instance = new ClusterDispatcherImplementation();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
