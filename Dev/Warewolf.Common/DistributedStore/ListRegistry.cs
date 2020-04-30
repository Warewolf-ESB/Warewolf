using System.Collections.Concurrent;
using Warewolf.Data;

namespace Warewolf.DistributedStore
{
    public static class ListRegistry
    {
        private static readonly WatcherList _clusterFollowerWatchers = new WatcherList();
        public static DistributedList<ServerFollower> ClusterFollowers = new DistributedList<ServerFollower>(_clusterFollowerWatchers);
    }
}
