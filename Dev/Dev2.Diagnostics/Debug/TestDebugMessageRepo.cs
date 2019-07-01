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
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Diagnostics.Debug
{
    public class TestDebugMessageRepo
    {
        readonly IDictionary<Tuple<Guid,string>, IList<IDebugState>> _data = new Dictionary<Tuple<Guid, string>, IList<IDebugState>>();

        static readonly object Lock = new object();

        private readonly static TestDebugMessageRepo _instance = new TestDebugMessageRepo();
        public static TestDebugMessageRepo Instance { get => _instance; }
        
        public void AddDebugItem(Guid resourceID,string testName, IDebugState ds)
        {
            if(resourceID != Guid.Empty)
            {
                lock(Lock)
                {
                    var key = new Tuple<Guid, string>(resourceID, testName);
                    if (_data.TryGetValue(key, out IList<IDebugState> list))
                    {
                        if (list.Contains(ds))
                        {
                            UpdateExistingItem(ds, list);
                            return;
                        }
                        list.Add(ds);
                    }
                    else
                    {
                        list = new List<IDebugState> { ds };
                        _data[key] = list;
                    }
                }
            }
        }

        private static void UpdateExistingItem(IDebugState ds, IList<IDebugState> list)
        {
            var existingItem = list.FirstOrDefault(state => state.Equals(ds));
            if (existingItem != null && ds.StateType != StateType.Duration)
            {
                list.Add(ds);
            }
        }

        public IList<IDebugState> FetchDebugItems(Guid resourceId,string testName)
        {
            lock(Lock)
            {
                var key = new Tuple<Guid, string>(resourceId, testName);
                if (_data.TryGetValue(key, out IList<IDebugState> list))
                {
                    _data.Remove(key);
                    return list;
                }
            }

            return null;
        }

        public IList<IDebugState> GetDebugItems(Guid resourceId, string testName)
        {
            lock (Lock)
            {
                var key = new Tuple<Guid, string>(resourceId, testName);
                if (_data.TryGetValue(key, out IList<IDebugState> list))
                {
                    return list;
                }
            }

            return null;
        }
    }
}
