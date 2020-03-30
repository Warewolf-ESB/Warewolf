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
using Dev2.Common;
using Dev2.Common.Interfaces.Logging;
using Warewolf.Esb;

namespace Dev2.Runtime.Network
{
    /**
     * Uses EsbHub to write DebugState data from Warewolf Server to the Studio. This state data
     * is received by the Studio as events in ServerProxyWithoutChunking as a SendDebugState event
     */
    internal class ClusterDispatcherImplementation : IClusterDispatcher
    {
        readonly ConcurrentDictionary<Guid, IClusterNotificationWriter> _writers = new ConcurrentDictionary<Guid, IClusterNotificationWriter>();
        readonly ILogger _dev2Logger;
        bool _shutdownRequested;

        public ClusterDispatcherImplementation()
            : this(new DefaultLogger())
        { }

        internal ClusterDispatcherImplementation(ILogger logger)
        {
            _dev2Logger = logger;
        }

        public int Count => _writers.Count;

        public void Add(Guid workspaceId, IClusterNotificationWriter writer)
        {
            if (writer == null || _shutdownRequested)
            {
                return;
            }
            _writers.TryAdd(workspaceId, writer);
        }

        public void Remove(Guid workspaceId)
        {
            _writers.TryRemove(workspaceId, out IClusterNotificationWriter _);
        }

        public void Shutdown()
        {
            _shutdownRequested = true;
        }
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
