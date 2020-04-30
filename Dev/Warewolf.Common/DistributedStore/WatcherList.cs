/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Concurrent;

namespace Warewolf.DistributedStore
{
    public interface IListWatcher
    {
        void Notify<T>(T item) where T : ListNotification;
    }
    public class WatcherList
    {
        ConcurrentQueue<IListWatcher> _watchers = new ConcurrentQueue<IListWatcher>();
        public void AddWatcher(IListWatcher hub)
        {
            _watchers.Enqueue(hub);
        }

        public void Notify<T>(T item) where T : ListNotification
        {
            var watchers = _watchers.ToArray();
            foreach (var watcher in watchers)
            {
                watcher.Notify<T>(item);
            }
        }
    }
}