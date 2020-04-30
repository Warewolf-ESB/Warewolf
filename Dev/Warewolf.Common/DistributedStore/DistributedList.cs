/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Warewolf.Common;

namespace Warewolf.DistributedStore
{
    public class DistributedList<T> where T : class, new()
    {
        private readonly List<T> _list = new List<T>();
        private readonly WatcherList _watchers;
        public WatcherList Watchers => _watchers;

        public DistributedList(WatcherList watchers)
        {
            _watchers = watchers;
        }

        public void Add(T item)
        {
            _list.Add(item);
            _watchers.Notify<Add<T>>(new Add<T>(item));
        }

        public void Clear()
        {
            _list.Clear();
            _watchers.Notify<Clear>(new Clear());
        }

        public bool Remove(T item)
        {
            var result = _list.Remove(item);
            _watchers.Notify<Remove<T>>(new Remove<T>(item));

            return result;
        }

        public int Count => _list.Count;
    }
}
